using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Npgsql;

namespace FenDB.Bot;

[AdminOnly]
public class WarnSlashCommand : ISlashCommand
{
    public string Name => "Warn";
    public string Description => "Warn user";
    public int Type => 1;
    public List<CommandParam> Parameters => new ParamBuilder()
        .CreateParam(SlashParamType.USER, "User", "Username", true)
        .CreateParam(SlashParamType.STRING, "Reason", "Reason of warn")
        .Build();

    public async Task ExecuteAsync(string id, string token, string data)
    {
        var payload = JsonNode.Parse(data);
        var _data = payload["d"];
        var _channelId = _data["channel_id"].GetValue<String>();
        var _guildId = _data["data"]["guild_id"]?.GetValue<String>();
        string _userId = "";
        string _reason = "-";

        foreach (var param in _data["data"]["options"].AsArray())
        {
            if (param["name"].GetValue<String>() == "user")
            {
                _userId = param["value"].GetValue<String>();
            }
            else if (param["name"].GetValue<String>() == "reason")
            {
                _reason = param["value"].GetValue<String>();
            }
        }

        await GiveWarnToUser(_guildId, _userId, _channelId, _reason, 172800);
        await SendEmbed(id, token, _userId, _channelId, _reason);

    }

    private async Task GiveWarnToUser(string serverId, string userId, string channelId, string reason, int warnTimeExpirationSeconds)
    {
        DateTime warnExpirationDate = DateTime.Now.AddSeconds(warnTimeExpirationSeconds);
        using (var cmd = new NpgsqlCommand("INSERT INTO warns (server_id, user_id, expire_date, reason) VALUES (@serverId, @userId, @expireDate, @reason)", SQLController.connection))
        {
            cmd.Parameters.AddWithValue("serverId", serverId);
            cmd.Parameters.AddWithValue("userId", userId);
            cmd.Parameters.AddWithValue("expireDate", warnExpirationDate);
            cmd.Parameters.AddWithValue("reason", reason);

            cmd.ExecuteNonQuery();
        }
    }
    private async Task SendEmbed(string id, string token, string userId, string channelId, string reason)
    {

        /*Download user avatar */
        //TODO: Make static class for that, probably will be used more often
        var client2 = new HttpClient();
        client2.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bot", CONFIG.Token);

        var response2 = await client2.GetAsync($"https://discord.com/api/v10/users/{userId}");
        if (!response2.IsSuccessStatusCode)
        {
            Console.WriteLine($"Błąd: {response2.StatusCode}");
            return;
        }

        var json2 = await response2.Content.ReadAsStringAsync();
        var user = JsonNode.Parse(json2);

        string avatarHash = user["avatar"]?.ToString();
        string discriminator = user["discriminator"]?.ToString();

        /*SendingEMBED*/
        var embed = new
        {
            title = "Warn",
            description = $"**User:** <@{userId}>\n**Reason:** {reason}\n**Date:** {DateTime.Now}",
            color = 0xFF0000,
            thumbnail = new
            {
                url = $"https://cdn.discordapp.com/avatars/{userId}/{avatarHash}.png"
            }
        };
        var payload2 = new
        {
            embeds = new[] { embed }
        };

        using (var client = new HttpClient())
        {
            client.DefaultRequestHeaders.Add("Authorization", $"Bot {CONFIG.Token}");
            var json = JsonSerializer.Serialize(payload2);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync($"https://discord.com/api/v10/channels/{channelId}/messages", content);

            if (response.IsSuccessStatusCode)
                Console.WriteLine("Sent embed!");
            else
                Console.WriteLine("Error: " + response.StatusCode);
        }


        using (var client = new HttpClient())
        {
            var payload3 = JsonSerializer.Serialize(new
            {
                type = 4,
                data = new
                {
                    content = "Warned",
                    flags = 64
                }
            });
            await client.PostAsync($"https://discord.com/api/v10/interactions/{id}/{token}/callback",
                new StringContent(payload3, Encoding.UTF8, "application/json"));
        }
    }
}