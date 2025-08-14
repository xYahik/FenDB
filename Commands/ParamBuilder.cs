

public class ParamBuilder{
    private readonly List<CommandParam> _params = new();

    public ParamBuilder CreateParam(SlashParamType ParamType, string Name, string Description, bool Required = false){
        _params.Add(new CommandParam(Name.ToLower(),Description,ParamType,Required));
        return this;
    }

    public ParamBuilder CreateIntParam(string Name, string Description, bool required = false){
        return CreateParam(SlashParamType.INTEGER,Name,Description, required);
    }

    public ParamBuilder CreateStringParam(string Name, string Description, bool required = false){
        return CreateParam(SlashParamType.STRING,Name,Description, required);
    }
    
    public List<CommandParam> Build() => _params;
}