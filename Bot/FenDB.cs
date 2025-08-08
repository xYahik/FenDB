namespace FenDB.Bot;

public class FenDB {

    private readonly GatewayConnection _connection = new();
    private readonly HeartbeatService _heartbeatService = new();
    private readonly EventDispatcher _eventDispatcher = new();
    private readonly CommandRegistrar _commandRegistrar = new();

    public async Task StartAsync() {
        Console.WriteLine("Starting FenDB...");

        await _commandRegistrar.RegisterCommandAsync();
        await _connection.ConnectAsync();

        _heartbeatService.Start(_connection);
        _eventDispatcher.Start(_connection, _commandRegistrar);

        Console.WriteLine("Bot is running");
        await Task.Delay(-1);
    }
}