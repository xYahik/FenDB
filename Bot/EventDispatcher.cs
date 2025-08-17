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
                        var msgId = data?["id"]?.GetValue<string>();
                        var creationTimestamp = data?["timestamp"]?.GetValue<string>();
                        var isAuthorBot = data?["author"]?["bot"]?.GetValue<bool>() ?? false;
                        var channelId = data?["channel_id"]?.GetValue<string>();
                        var authorId = data?["author"]?["id"]?.GetValue<string>();

                        if (msgId == null || content == null || creationTimestamp == null)
                            continue;

                        MessageManager.OnMessageCreated(msgId, content, creationTimestamp, authorId);

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
                    else if (type == "GUILD_MEMBER_ADD")
                    {
                        var data = payload["d"];
                        var _guildId = data["guild_id"]?.GetValue<string>();
                        var _userId = data["user"]?["id"].GetValue<string>();
                        //var _userAvatarHash = data["user"]?["avatar"].GetValue<string>();
                        //var _userName = data["user"]?["username"].GetValue<string>();

                        string newEmbedContent = $"New User Joined!\n <@{_userId}>\n";
                        await Logger.SendEmbedWithAvatar(_guildId, _userId, newEmbedContent);
                    }
                    else if (type == "GUILD_MEMBER_REMOVE")
                    {
                        var data = payload["d"];
                        var _guildId = data["guild_id"]?.GetValue<string>();
                        var _userId = data["user"]?["id"].GetValue<string>();

                        string newEmbedContent = $"User left the server!\n <@{_userId}>\n";
                        await Logger.SendEmbedWithAvatar(_guildId, _userId, newEmbedContent);
                    }
                    else if (type == "MESSAGE_DELETE")
                    {
                        var data = payload["d"];
                        var _guildId = data["guild_id"]?.GetValue<string>();
                        var _msgId = data["id"]?.GetValue<string>();
                        var channelId = data?["channel_id"]?.GetValue<string>();
                        var content = data?["content"]?.GetValue<string>()?.Trim();
                        var creationTimestamp = data?["timestamp"]?.GetValue<string>();

                        if (!MessageManager.IsMessageCached(_msgId))
                            continue;

                        string? oldMsg = MessageManager.GetCachedMessage(_msgId);
                        string? authorId = MessageManager.GetCachedMessageAuthor(_msgId);
                        MessageManager.OnMessageDelete(_msgId);
                        string newEmbedContent = $"Message Deleted: \n Author:<@{authorId}>\n```{oldMsg}```";

                        await Logger.SendEmbed(_guildId, newEmbedContent);
                    }
                    else if (type == "MESSAGE_UPDATE")
                    {
                        var data = payload["d"];
                        var _guildId = data["guild_id"]?.GetValue<string>();
                        var _msgId = data["id"]?.GetValue<string>();
                        var channelId = data?["channel_id"]?.GetValue<string>();
                        var content = data?["content"]?.GetValue<string>()?.Trim();
                        var creationTimestamp = data?["timestamp"]?.GetValue<string>();
                        var authorId = data?["author"]?["id"]?.GetValue<string>();

                        if (!MessageManager.IsMessageCached(_msgId))
                            continue;

                        string? oldMsg = MessageManager.GetCachedMessage(_msgId);
                        MessageManager.OnMessageUpdate(_msgId, content, creationTimestamp);
                        string msgLink = $"https://discord.com/channels/{_guildId}/{channelId}/{_msgId}";
                        string newEmbedContent = $"Message Edited: {msgLink}\n Author:<@{authorId}>\n```{oldMsg}```\n to: \n ```{content}````";
                        await Logger.SendEmbed(_guildId, newEmbedContent);

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