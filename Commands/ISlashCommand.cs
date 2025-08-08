public interface ISlashCommand{
    string Name{get;}
    string Description{get;}
    int Type{get;}
    Task ExecuteAsync(string id,string token);
}