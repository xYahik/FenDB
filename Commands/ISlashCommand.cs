public interface ISlashCommand{
    string Name{get;}
    string Description{get;}
    int Type{get;}
    public List<CommandParam>? Parameters { get; }

    Task ExecuteAsync(string id,string token, string data);
}