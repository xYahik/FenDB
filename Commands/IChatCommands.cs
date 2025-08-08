public interface IChatCommand{
    string CommandPrefix {get;}
    string Name{get;}
    Task ExecuteAsync(string channelId);
}