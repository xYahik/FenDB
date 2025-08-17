

using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace FenDB.Bot;

public static class Logger
{
    /*SendLog(string text){
        
    }*/
    public static async Task SendEmbed(string serverId, object? embed)
    {

        var payload = new
        {
            embeds = new[] { embed }
        };

        string? _logChannelId = ServerSettingsManager.GetOption(serverId, "ChannelLogId");
        if (_logChannelId != null)
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Authorization", $"Bot {CONFIG.Token}");
                var json = JsonSerializer.Serialize(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync($"https://discord.com/api/v10/channels/{_logChannelId}/messages", content);

                if (response.IsSuccessStatusCode)
                    Console.WriteLine("Sent embed!");
                else
                    Console.WriteLine("Error: " + response.StatusCode);
            }
        }
    }
    public static async Task SendEmbed(string serverId, string text)
    {
        var embed = new
        {
            title = "Logger",
            description = text,
            color = 0xFF0000
        };
        await SendEmbed(serverId, embed);
    }

    public static async Task SendEmbedWithAvatar(string serverId, string userId, string text)
    {
        var client = new HttpClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bot", CONFIG.Token);

        var response = await client.GetAsync($"https://discord.com/api/v10/users/{userId}");
        if (!response.IsSuccessStatusCode)
        {
            Console.WriteLine($"Error: {response.StatusCode}");
            return;
        }

        var json = await response.Content.ReadAsStringAsync();
        var user = JsonNode.Parse(json);

        string avatarHash = user["avatar"]?.ToString();

        var embed = new
        {
            title = "Logger",
            description = text,
            color = 0xFF0000,
            thumbnail = new
            {
                url = $"https://cdn.discordapp.com/avatars/{userId}/{avatarHash}.png"
            }
        };
        await SendEmbed(serverId, embed);
    }
}