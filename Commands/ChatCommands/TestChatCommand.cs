using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace FenDB.Bot;

public class TestChatCommand : IChatCommand{
    public string CommandPrefix => "@";
    public string Name => "TestChatCommand";

    public async Task ExecuteAsync(string channelId){
        await SendMessage(channelId);
    }

    private async Task SendMessage(string channelId){

        //TODO: tahts not how should work while we have gatewayconnection etc. should i send it by it.
        //then i also not need to use using above
        using var client = new HttpClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bot", CONFIG.Token);
        var payload = JsonSerializer.Serialize(new { content = "TestChatCommand Working" });
        await client.PostAsync($"https://discord.com/api/v10/channels/{channelId}/messages",
        new StringContent(payload, Encoding.UTF8, "application/json"));
    }
}