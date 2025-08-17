

public class MessageManager
{
    private static Dictionary<string, CachedMessage> messageCache = new();

    public static void OnMessageCreated(string? msgId, string? content, string? timestamp, string? authorId)
    {
        messageCache.Add(msgId, new CachedMessage { Id = msgId, Content = content, Timestamp = DateTime.Parse(timestamp, null, System.Globalization.DateTimeStyles.RoundtripKind), AuthorId = authorId });
    }
    public static void OnMessageUpdate(string? msgId, string? content, string? timestamp)
    {
        if (messageCache.ContainsKey(msgId))
            messageCache[msgId] = new CachedMessage { Id = msgId, Content = content, Timestamp = DateTime.Parse(timestamp, null, System.Globalization.DateTimeStyles.RoundtripKind) };
    }
    public static void OnMessageDelete(string? msgId)
    {
        messageCache.Remove(msgId);
    }

    public static bool IsMessageCached(string msgId)
    {
        return messageCache.ContainsKey(msgId);
    }

    public static string? GetCachedMessage(string msgId)
    {
        return messageCache.ContainsKey(msgId) ? messageCache[msgId].Content : null;
    }
    public static string? GetCachedMessageAuthor(string msgId)
    {
        return messageCache.ContainsKey(msgId) ? messageCache[msgId].AuthorId : null;
    }
}


class CachedMessage
{
    public string Id { get; set; }
    public string Content { get; set; }
    public DateTime Timestamp { get; set; }

    public string AuthorId { get; set; }
}