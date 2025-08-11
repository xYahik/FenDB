
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Reflection;

namespace FenDB.Bot;

/*
For testing purpose better register commands locally, i.e. on specific server 
EndPoint: /applications/{APP_ID}/guilds/{GUILD_ID}/commands
it add commands instantly and so easy to test.


As a final product, if bot should be hosted for multiple servers, it should be added globaly
EndPoint: /applications/{APP_ID}/commands
command can be added after some time, ~1h, maybe longer. but it's added for all servers etc.



ToRemove specific command DELETE EndPoint

should add struct and param to decide if command is testable or global :thinking: and just depends on it, add it to register locally for "testServer"(need add extra param for that) or globally
*/

//TODO: if command have disabled attribute, add functionality to delete command from bot.
//      discord don't remove it itself at restart bot, it will be on server until we delete it manually.
public class CommandRegistrar{
    private readonly HttpClient _http = new();

    public List<ISlashCommand> _slashCommands;

    public async Task CommandLoader()
    {
        _slashCommands = LoadSlashCommands();
    }

    private List<ISlashCommand> LoadSlashCommands()
    {
        var assembly = Assembly.GetExecutingAssembly();

        var commandTypes = assembly.GetTypes()
            .Where(t =>
                typeof(ISlashCommand).IsAssignableFrom(t) &&
                t.IsClass &&
                !t.IsAbstract &&
                !t.GetCustomAttributes(typeof(DisabledAttribute), inherit: true).Any()
            );

        var commands = commandTypes
            .Select(Activator.CreateInstance)
            .Cast<ISlashCommand>()
            .ToList();

        return commands;
    }

    public async Task RegisterCommandAsync(){

        await CommandLoader();

        _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bot", CONFIG.Token);

        foreach(var command in _slashCommands){

            var hasAdminOnly = command.GetType().GetCustomAttribute<AdminOnlyAttribute>() != null;

            //default_member_permission - we can assume that default permission to use command everyone is DiscordPermissionFlags.SEND_MESSAGES, if we can't write we shouldn't be able to use command anyway
            var commandBody = new {
                name = command.Name.ToLowerInvariant(), 
                description = command.Description, 
                type = command.Type, 
                default_member_permissions = hasAdminOnly ? DiscordPermissionFlags.ADMINISTRATOR : DiscordPermissionFlags.SEND_MESSAGES,
                options = command.Parameters
                };

            var json = JsonSerializer.Serialize(commandBody,new JsonSerializerOptions{PropertyNamingPolicy = JsonNamingPolicy.CamelCase});
            //Console.WriteLine(json);
            var content = new StringContent(json,Encoding.UTF8, "application/json");
            var url = $"https://discord.com/api/v10/applications/{CONFIG.AppId}/guilds/{CONFIG.GuildId}/commands";

            var response = await _http.PostAsync(url,content);
            if(response.IsSuccessStatusCode){
                Console.WriteLine($"Registered /{command.Name}");
            }else{
                var error = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Failed to register /{command.Name}: {error}");
            }
        }
    }

}