using System.Text.RegularExpressions;

namespace DocFusion.Application.Helper;

public class AiParser
{
    public static string ParseAiResponse(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            throw new ArgumentException("Empty response");

        // 1. Убираем управляющие символы (\n, \r и т.п.)
        var clean = raw.Replace("\\n", "\n")
            .Replace("\\t", "\t")
            .Replace("\\\"", "\"");

        // 2. Убираем комментарии // ...
        clean = Regex.Replace(clean, @"//.*", string.Empty);

        // 3. Убираем возможный мусор до первого {
        var jsonStart = clean.IndexOf('{');
        if (jsonStart > 0)
            clean = clean.Substring(jsonStart);

        // 4. Убираем всё после последней закрывающей скобки
        var jsonEnd = clean.IndexOf('}');
        if (jsonEnd > 0 && jsonEnd < clean.Length - 1)
            clean = clean.Substring(0, jsonEnd + 1);
        
        return clean;
    }
}