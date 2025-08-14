using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace FenDB.Bot;

public class EventDispatcher
{
    private readonly List<IChatCommand> _chatCommands = [new TestChatCommand()];

    public void Start(GatewayConnection connection, CommandRegistrar _commandRegistrar)
    {
        _ = Task.Run(async () =>
        {
            while (true)
            {
                try
                {
                    var msg = await connection.ReceiveAsync();
                    if (msg == null)
                        continue;

                    var payload = JsonNode.Parse(msg);
                    if (payload == null)
                        continue;

                    var type = payload["t"]?.GetValue<String>();
                    if (type == "MESSAGE_CREATE")
                    {

                        var data = payload["d"];
                        var content = data?["content"]?.GetValue<string>()?.Trim();
                        var isAuthorBot = data?["author"]?["bot"]?.GetValue<bool>() ?? false;
                        var channelId = data?["channel_id"]?.GetValue<string>();
                        if (isAuthorBot || string.IsNullOrWhiteSpace(content) || channelId is null)
                            continue;



                        var cmdName = content.ToLowerInvariant();
                        var cmd = _chatCommands.FirstOrDefault(c => (c.CommandPrefix + c.Name.ToLowerInvariant()) == cmdName);
                        if (cmd != null)
                        {
                            Console.WriteLine($"ChatCommand - {cmdName} by user");
                            await cmd.ExecuteAsync(channelId);
                        }
                    }
                    else if (type == "INTERACTION_CREATE")
                    {

                        //Console.WriteLine(payload);
                        var data = payload["d"];
                        int interactionType = (int)data["type"];
                        var id = data?["id"]?.GetValue<string>();
                        var token = data?["token"]?.GetValue<string>();
                        if (interactionType == 2)   //APPLICATION_COMMAND - slash / user / message command
                        {
                            string commandName = data["data"]["name"].ToString().ToLowerInvariant();
                            var cmd = _commandRegistrar._slashCommands.FirstOrDefault(c => c.Name.ToLowerInvariant() == commandName);
                            if (cmd != null)
                            {
                                Console.WriteLine($"SlashCommand - {commandName} by user");
                                await cmd.ExecuteAsync(id, token, msg);
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        });
    }
}