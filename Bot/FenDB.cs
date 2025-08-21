namespace FenDB.Bot;

public class FenDB
{

    private readonly GatewayConnection _connection = new();
    private readonly HeartbeatService _heartbeatService = new();
    private readonly EventDispatcher _eventDispatcher = new();
    private readonly CommandRegistrar _commandRegistrar = new();
    private readonly PythonService _pythonService = new();
    private readonly ModulesLoader _modulesLoader = new();
    private SQLManager _sqlController = new();

    public async Task StartAsync()
    {
        Console.WriteLine("Starting FenDB...");
        await _sqlController.Initialize();
        await _pythonService.Initialize();
        await ServerSettingsManager.LoadOptionsFromDB();
        await _modulesLoader.InitializeModules();
        await _commandRegistrar.RegisterCommandAsync();
        await _connection.ConnectAsync();

        _heartbeatService.Start(_connection);
        _eventDispatcher.Start(_connection, _commandRegistrar);

        Console.WriteLine("Bot is running");
        await Task.Delay(-1);
    }
}