using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace FenDB.Bot;

public class TestSlashCommand : ISlashCommand{
    public string Name => "TestSlashCommand";
    public string Description => "Test Slash Command Description";
    public int Type => 1;

    public async Task ExecuteAsync(string id,string token){
        await SendMessage(id,token);
    }

    private async Task SendMessage(string id,string token){
        
         using var client = new HttpClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bot", CONFIG.Token);
        var payload = JsonSerializer.Serialize(new { type = 4, data = new {content ="Hello"/*, flags=64 visible only for user*/} });
        await client.PostAsync($"https://discord.com/api/v10/interactions/{id}/{token}/callback",
            new StringContent(payload, Encoding.UTF8, "application/json"));
    }
}