namespace WFetch;

internal sealed record LogoSegment(string Text, string Color);

internal static class Logo
{
    private const int QuadW = 15;
    private const int QuadH = 6;
    private const string Gap = "   ";

    internal static List<List<LogoSegment>> Build(string name, string accentColor)
    {
        return name.ToLowerInvariant() switch
        {
            "windows10" or "classic" => BuildQuadrants("red", "green", "blue", "yellow"),
            "none" => [],
            _ => BuildQuadrants(accentColor, accentColor, accentColor, accentColor),
        };
    }

    internal static List<List<LogoSegment>> BuildFromFile(string path, string accentColor)
    {
        var lines = File.ReadAllLines(path);
        var result = new List<List<LogoSegment>>(lines.Length);
        foreach (var line in lines)
        {
            string text = line
                .Replace("{accent}", "", StringComparison.OrdinalIgnoreCase)
                .Replace("{r}", "", StringComparison.OrdinalIgnoreCase);
            result.Add([new LogoSegment(text, accentColor)]);
        }
        return result;
    }

    internal static int VisualWidth(List<List<LogoSegment>> lines) =>
        lines.Count == 0 ? 0 : lines.Max(segments => segments.Sum(s => s.Text.Length));

    private static List<List<LogoSegment>> BuildQuadrants(string tl, string tr, string bl, string br)
    {
        string full = new('█', QuadW);
        var result = new List<List<LogoSegment>>();

        for (int i = 0; i < QuadH; i++)
            result.Add([new LogoSegment(full, tl), new LogoSegment(Gap, "black"), new LogoSegment(full, tr)]);

        result.Add([new LogoSegment(new string(' ', QuadW * 2 + Gap.Length), "black")]);

        for (int i = 0; i < QuadH; i++)
            result.Add([new LogoSegment(full, bl), new LogoSegment(Gap, "black"), new LogoSegment(full, br)]);

        return result;
    }
}
