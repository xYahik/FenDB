using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace FenDB.Bot;

[AdminOnly]
public class PurgeSlashCommand : ISlashCommand{
    public string Name => "Purge";
    public string Description => "Delete specific amount of messages";
    public int Type => 1;

    public async Task ExecuteAsync(string id,string token){
        await SendMessage(id,token);
    }

    private async Task SendMessage(string id,string token){


    }
}