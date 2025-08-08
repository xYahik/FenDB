using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

[Disabled]
public class HiCommand : ISlashCommand{
    public string Name => "hi";
    public string Description => "Bot will answer Hello :3";
    public int Type => 1;

    public async Task ExecuteAsync(string id,string token){
        Console.WriteLine("Hi");
        await Task.CompletedTask;
    }
}