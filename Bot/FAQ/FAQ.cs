
using System.Text.Json.Nodes;

namespace FenDB.Bot;

public class FAQ : IModules
{
    public async Task Initialize()
    {
        Console.WriteLine("Initializing FAQ Module");

        await InitializeFAQQuestions();

        EventDispatcher.OnMessageCreated += (sender, data) =>
        {
            HandleMessage(data);
        };
    }

    public async Task InitializeFAQQuestions()
    {

    }

    public void HandleMessage(JsonNode data)
    {
        Console.WriteLine(data);
    }
}