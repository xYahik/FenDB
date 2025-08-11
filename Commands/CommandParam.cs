public class CommandParam
{
    public string Name { get; }
    public string Description {get;}
    public SlashParamType Type { get; }
    public bool Required {get;}

    public CommandParam(string _name,string _description, SlashParamType _type, bool _required){
        Name = _name;
        Description = _description;
        Type = _type;
        Required = _required;
    }
}