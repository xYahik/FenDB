using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace FenDB.Bot;

public class GatewayConnection {
    private readonly ClientWebSocket _webSocket = new();
    private readonly Uri _gatewayUri = new("wss://gateway.discord.gg/?v=10&encoding=json");
    private readonly byte[] _buffer = new byte[8192];

    public ClientWebSocket Socket => _webSocket;

    public async Task ConnectAsync() {
        await _webSocket.ConnectAsync(_gatewayUri, CancellationToken.None);

        var identify = new {
            op = 2,
            d = new {
                token = CONFIG.Token,
                intents = DiscordIntents.GUILD_MESSAGES | DiscordIntents.MESSAGE_CONTENT | DiscordIntents.GUILD_MEMBERS,
                properties = new {
                    os = "linus", browser = "custom", device = "custom"
                }
            }
        };

        var payload = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(identify));
        await _webSocket.SendAsync(payload, WebSocketMessageType.Text, true, CancellationToken.None);

        //TODO: Checking if send is successful, could fail if token is wrong etc. need to add task failed and procees it as but couldnt start etc, maybe different problems if can handle it
        //getting 2 opcodes there, 10 and then 0
        // 10 is for heartbeat interval - need to read more but probably it give me information how often should send hearbeat
        // 0 generall information about bot, also not receiveing it if token is wrong

        var result = await _webSocket.ReceiveAsync(_buffer, CancellationToken.None);

        if (result.MessageType == WebSocketMessageType.Text) {
            var response = Encoding.UTF8.GetString(_buffer, 0, result.Count);
            //Console.WriteLine(response);
        } else if (result.MessageType == WebSocketMessageType.Close) {

        }

        result = await _webSocket.ReceiveAsync(_buffer, CancellationToken.None);

        if (result.MessageType == WebSocketMessageType.Text) {
            var response = Encoding.UTF8.GetString(_buffer, 0, result.Count);
            //Console.WriteLine(response);
        } else if (result.MessageType == WebSocketMessageType.Close) {

        }

    }

    public async Task SendAsync(string json) {
        var bytes = Encoding.UTF8.GetBytes(json);
        await _webSocket.SendAsync(bytes, WebSocketMessageType.Text, true, CancellationToken.None);
    }

    public async Task < string ? > ReceiveAsync() {
        var result = await _webSocket.ReceiveAsync(_buffer, CancellationToken.None);
        if (result.MessageType == WebSocketMessageType.Close) {
            return null;
        }
        return Encoding.UTF8.GetString(_buffer, 0, result.Count);
    }
}