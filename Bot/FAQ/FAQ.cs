
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace FenDB.Bot;

public class FAQ : IModules
{
    List<(string Question, string Answer, float[] Vector)> faqVectors = new();

    public async Task Initialize()
    {
        Console.WriteLine("Initializing FAQ Module");

        await InitializeFAQQuestions();

        EventDispatcher.OnMessageCreated += async (sender, data) =>
        {
            await HandleMessage(data);
        };
    }

    public async Task InitializeFAQQuestions()
    {
        var faqQuestions = new List<(string Question, string Answer)>
        {
            //Test of greetings as a alternative to detailed FAQ questions
            //If there's multiple combination of question, higher chance for model to find connection
            ("Hey FenDB", "Hey"),
            ("Hi FenDB", "Hi"),
            ("Hello FenDB", "Hello"),
            ("What's this server about?", "A server for everyone, where you can relax"),
        };

        foreach (var faq in faqQuestions)
        {
            var vector = await GetEmbedding(faq.Question);
            faqVectors.Add((faq.Question, faq.Answer, vector));
        }
    }

    public async Task HandleMessage(JsonNode data)
    {
        var msg = data["content"];
        var msgVector = await GetEmbedding(msg.GetValue<string>());

        var bestSimilarity = faqVectors.Select(faq => new
        {
            faq.Answer,
            Similarity = CosineSimilarity(msgVector, faq.Vector)
        })
        .OrderByDescending(x => x.Similarity)
        .First();

        if (bestSimilarity.Similarity > 0.8)
        {
            var channelId = data["channel_id"].GetValue<string>();
            var msgId = data["id"].GetValue<string>();
            await SendMessage(channelId, msgId, $"{bestSimilarity.Answer} \n\n-# Confidence: {Math.Round(bestSimilarity.Similarity * 100)}%");
        }
    }

    public async Task<float[]> GetEmbedding(string text)
    {
        var input = new { message = text };
        var json = JsonSerializer.Serialize(input);
        var client = new HttpClient();
        var response = await client.PostAsync("http://127.0.0.1:5000/embed", new StringContent(json, Encoding.UTF8, "application/json"));

        var result = await response.Content.ReadAsStringAsync();

        var data = JsonNode.Parse(result);
        var jsonArray = data["embedding"].AsArray();
        float[] array = jsonArray.Select(j => j!.GetValue<float>()).ToArray();

        return array;
    }

    private double CosineSimilarity(float[] v1, float[] v2)
    {
        double dot = 0.0d;
        double mag1 = 0.0d;
        double mag2 = 0.0d;

        for (int i = 0; i < v1.Length; i++)
        {
            dot += v1[i] * v2[i];
            mag1 += v1[i] * v1[i];
            mag2 += v2[i] * v2[i];
        }
        return dot / (Math.Sqrt(mag1) * Math.Sqrt(mag2));
    }
    private async Task SendMessage(string channelId, string msgIdToReply, string answer)
    {

        //TODO: tahts not how it should work while we have gatewayconnection etc. should i send it by it.
        using var client = new HttpClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bot", CONFIG.Token);

        var payload = JsonSerializer.Serialize(new
        {
            content = answer,
            message_reference = new { message_id = msgIdToReply }
        });
        await client.PostAsync($"https://discord.com/api/v10/channels/{channelId}/messages",
        new StringContent(payload, Encoding.UTF8, "application/json"));
    }
}