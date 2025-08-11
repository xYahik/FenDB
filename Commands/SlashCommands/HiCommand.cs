using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

[Disabled]
public class HiCommand : ISlashCommand{
    public string Name => "hi";
    public string Description => "Bot will answer Hello :3";
    public int Type => 1;
    public List<CommandParam> Parameters => null;

    public async Task ExecuteAsync(string id,string token, string data){
        Console.WriteLine("Hi");
        await Task.CompletedTask;
    }
}