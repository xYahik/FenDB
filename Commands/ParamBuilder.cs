

public class ParamBuilder{
    private readonly List<CommandParam> _params = new();


    public ParamBuilder CreateIntParam(string Name, string Description, bool required = false){
        _params.Add(new CommandParam(Name,Description, SlashParamType.INTEGER,required));
        return this;
    }
    public ParamBuilder CreateStringParam(string Name, string Description, bool required = false){
        _params.Add(new CommandParam(Name,Description, SlashParamType.STRING,required));
        return this;
    }
    
    public List<CommandParam> Build() => _params;
}