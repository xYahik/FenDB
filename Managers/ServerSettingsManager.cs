
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Reflection;
using FenDB.Bot;

public class ServerSettingsManager
{
    private static Dictionary<string, ServerSettings> _serverConfigs { get; } = new();

    public async static Task LoadOptionsFromDB()
    {
        var result = SQLManager.ExecuteQuery($@"
        SELECT json_agg(row_to_json(t))
        FROM (
            SELECT * FROM serverSettings
        ) t;")?.ToString();

        var json = JsonNode.Parse(result);
        foreach (var server in json.AsArray())
        {
            string server_id = server["server_id"].GetValue<String>();
            if (!_serverConfigs.ContainsKey(server_id))
            {
                _serverConfigs[server_id] = new ServerSettings();
            }

            var type = _serverConfigs[server_id].GetType();
            foreach (var prop in type.GetProperties())
            {
                var attr = prop.GetCustomAttribute<DBVariableNameAttribute>();
                if (attr != null)
                {
                    if (prop.PropertyType == typeof(string))
                    {
                        string columnName = attr.VariableName.ToLower();
                        prop.SetValue(_serverConfigs[server_id], server[columnName]?.GetValue<String>());

                    }
                    else if (prop.PropertyType == typeof(int?))
                    {
                        string columnName = attr.VariableName.ToLower();
                        prop.SetValue(_serverConfigs[server_id], server[columnName]?.GetValue<int>());
                    }
                }
            }
        }
    }


    /*TODO: Move updating sql data from SetOption to Task which will be executed every X minutes to prevent too many Queries*/
    public static void SetOption(string guildId, string optionName, string optionValue)
    {
        Console.WriteLine(optionName);
        Console.WriteLine(optionValue);
        if (!_serverConfigs.ContainsKey(guildId))
        {
            _serverConfigs[guildId] = new ServerSettings();
        }

        ServerSettings serverConfig = _serverConfigs[guildId];
        switch (optionName)
        {
            case "ChannelLogId":
                {
                    serverConfig.ChannelLogId = DiscordIdExtractor.GetChannelID(optionValue);
                }
                break;
            case "WarnCountBeforeBan":
                {
                    serverConfig.WarnCountBeforeBan = int.TryParse(optionValue, out var temp) ? temp : (int?)null;
                }
                break;
            case "BanRoleId":
                {
                    serverConfig.BanRoleId = DiscordIdExtractor.GetRoleID(optionValue);
                }
                break;
            default: break;
        }

        var type = serverConfig.GetType();
        foreach (var prop in type.GetProperties())
        {
            Console.WriteLine($"{prop.Name} kekw");
            if (prop.Name.ToLower() == optionName.ToLower())
            {
                var attribute = (DBVariableNameAttribute?)Attribute.GetCustomAttribute(prop, typeof(DBVariableNameAttribute));
                if (attribute != null)
                {
                    var value = prop.GetValue(serverConfig);
                    string sqlValue = (value == null || string.IsNullOrWhiteSpace(value.ToString())) ? "NULL" : $"'{value}'";
                    SQLManager.ExecuteUpdate(
                        @$"
                    INSERT INTO serverSettings (server_id, {attribute.VariableName})
                    VALUES ('{guildId}', {sqlValue})
                    ON CONFLICT (server_id) DO UPDATE
                    SET {attribute.VariableName} = EXCLUDED.{attribute.VariableName};
                    "
                    );
                }
            }
        }

    }
    public static string? GetOption(string guildId, string optionName)
    {
        if (_serverConfigs.ContainsKey(guildId))
        {
            switch (optionName)
            {
                case "ChannelLogId":
                    {
                        return _serverConfigs[guildId].ChannelLogId;
                    }
                case "WarnCountBeforeBan":
                    {
                        return _serverConfigs[guildId].WarnCountBeforeBan.ToString();
                    }
                case "BanRoleId":
                    {
                        return _serverConfigs[guildId].BanRoleId;
                    }
                default: break;
            }

        }

        return null;

    }

    public static dynamic GenerateSettingsEmbed(string guildId)
    {
        if (!_serverConfigs.ContainsKey(guildId))
        {
            _serverConfigs[guildId] = new ServerSettings();
        }
        string? banRoleId = GetOption(guildId, "BanRoleId");
        string? warnCountBeforeBan = GetOption(guildId, "WarnCountBeforeBan");
        string? channelLogId = $"<#{GetOption(guildId, "ChannelLogId")}>";
        //Add Embed variable
        var embed = new
        {
            title = "ServerSetting",
            description = @$"
            **BanRoleId:** {banRoleId}
            **WarnCountBeforeBan:** {warnCountBeforeBan}
            **ChannelLogId:** {channelLogId}
        ",
            color = 0xFF0000
        };
        return embed;
    }
}

public class ServerSettings
{
    [DBVariableName("banrole_id")]
    public string? BanRoleId { get; set; }

    [DBVariableName("warnCountBeforeBan")]
    public int? WarnCountBeforeBan { get; set; }

    [DBVariableName("channelLog_id")]
    public string? ChannelLogId { get; set; }
}