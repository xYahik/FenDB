using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace FenDB.Bot;

[AdminOnly]
public class PurgeSlashCommand: ISlashCommand {
    public string Name => "Purge";
    public string Description => "Delete specific amount of messages";
    public int Type => 1;
    public List < CommandParam > Parameters => new ParamBuilder()
        .CreateIntParam("amount", "How many messages should be deleted", true)
        .Build();

    public async Task ExecuteAsync(string id, string token, string data) {
        var payload = JsonNode.Parse(data);
        var _data = payload["d"];
        var _channelId = _data["channel_id"].GetValue < String > ();
        var _messagesAmount = _data["data"]["options"][0]["value"].GetValue < int > ();
        string messagesJson = await GetLastMassages(_channelId, _messagesAmount);
        await DeleteMessages(messagesJson, _channelId, id, token);
    }

    private async Task < string > GetLastMassages(string channelId, int amount) {

        var client = new HttpClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bot", CONFIG.Token);
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        var response = await client.GetAsync($"https://discord.com/api/v10/channels/{channelId}/messages?limit={amount}");
        var json = await response.Content.ReadAsStringAsync();
        return json;
    }

    private async Task DeleteMessages(string messagesJson, string channelId, string id, string token) {
        //bulk-delete only work with range 2-100

        List<string> _messages = new List<string> ();
        var messages = JsonNode.Parse(messagesJson).AsArray();
        foreach(var message in messages) {
            _messages.Add(message["id"].GetValue < string > ());
        }

        var client = new HttpClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bot", CONFIG.Token);
        var payload = JsonSerializer.Serialize(new {
            messages = _messages
        });

        if (_messages.Count >= 2) {
            var response = await client.PostAsync($"https://discord.com/api/v10/channels/{channelId}/messages/bulk-delete",
                new StringContent(payload, Encoding.UTF8, "application/json"));
        } else {
            await client.DeleteAsync($"https://discord.com/api/v10/channels/{channelId}/messages/{_messages[0]}");
        }

        payload = JsonSerializer.Serialize(new {
            type = 4, data = new {
                content = "Purge Completed", flags = 64
            }
        });
        await client.PostAsync($"https://discord.com/api/v10/interactions/{id}/{token}/callback",
            new StringContent(payload, Encoding.UTF8, "application/json"));
    }
}