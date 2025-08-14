
using System.Text.RegularExpressions;
public class DiscordIdExtractor
{
    public static string? GetChannelID(string input)
    {
        string pattern = @"\d{19}";
        Match match = Regex.Match(input, pattern);
        if (match.Success)
        {
            return match.Value;
        }
        else
        {
            return null;
        }
    }
}