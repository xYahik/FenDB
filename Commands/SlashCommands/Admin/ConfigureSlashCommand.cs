using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace FenDB.Bot;

[AdminOnly]
public class ConfigureSlashCommand : ISlashCommand
{
    public string Name => "Configure";
    public string Description => "Configure specified server setting value";
    public int Type => 1;
    public List<CommandParam> Parameters => new ParamBuilder()
        .CreateParam(SlashParamType.STRING, "Name", "Setting Param name", true)
        .CreateParam(SlashParamType.STRING, "Value", "Setting Param value", true)
        .Build();

    public async Task ExecuteAsync(string id, string token, string data)
    {
        var payload = JsonNode.Parse(data);
        var _data = payload["d"];
        var _channelId = _data["channel_id"].GetValue<String>();
        var _guildId = _data["data"]["guild_id"]?.GetValue<String>();

        string _settingName = "";
        string _settingValue = "";

        foreach (var param in _data["data"]["options"].AsArray())
        {
            if (param["name"].GetValue<String>() == "name")
            {
                _settingName = param["value"].GetValue<String>();
            }
            else if (param["name"].GetValue<String>() == "value")
            {
                _settingValue = param["value"].GetValue<String>();
            }
        }

        ServerSettingsController.SetOption(_guildId, _settingName, _settingValue);

        using (var client = new HttpClient())
        {
            var payloadSend = JsonSerializer.Serialize(new
            {
                type = 4,
                data = new
                {
                    content = $"Successfully set {_settingName} = {_settingValue}",
                    flags = 64
                }
            });
            await client.PostAsync($"https://discord.com/api/v10/interactions/{id}/{token}/callback",
                new StringContent(payloadSend, Encoding.UTF8, "application/json"));
        }
    }
}