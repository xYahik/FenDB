using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace FenDB.Bot;

[AdminOnly]
public class ConfigSlashCommand : ISlashCommand
{
    public string Name => "Config";
    public string Description => "Return Embed msg with current config for server";
    public int Type => 1;
    public List<CommandParam> Parameters => null;

    public async Task ExecuteAsync(string id, string token, string data)
    {
        var payload = JsonNode.Parse(data);
        var _data = payload["d"];
        var _channelId = _data["channel_id"].GetValue<String>();
        var _guildId = _data["data"]["guild_id"]?.GetValue<String>();

        var embed = ServerSettingsManager.GenerateSettingsEmbed(_guildId);

        using (var client = new HttpClient())
        {
            var payloadSend = JsonSerializer.Serialize(new
            {
                type = 4,
                data = new
                {
                    content = "YourCurrentConfig:",
                    embeds = new[] { embed },
                    flags = 64
                }
            });
            await client.PostAsync($"https://discord.com/api/v10/interactions/{id}/{token}/callback",
                new StringContent(payloadSend, Encoding.UTF8, "application/json"));
        }
    }
}