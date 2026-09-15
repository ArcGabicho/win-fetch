namespace WFetch;

internal sealed record LogoSegment(string Text, string Color);

internal static class Logo
{
    // El logo real de Windows 11 es una cuadrícula 2x2 de paneles sólidos en
    // azul. Se dibuja con caracteres ASCII (#) en vez de bloques Unicode, pero
    // rellenos por completo (no huecos).
    private const int TileWidth = 15;
    private const int TileHeight = 7;
    private const string Gap = "   ";
    private static readonly string FilledRow = new('#', TileWidth);

    internal static List<List<LogoSegment>> Build(string name, string accentColor)
    {
        return name.ToLowerInvariant() switch
        {
            "windows10" or "classic" => BuildQuadrants("red", "green", "blue", "yellow"),
            "none" => [],
            _ => BuildQuadrants("blue", "blue", "blue", "blue"),
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
        var result = new List<List<LogoSegment>>();

        for (int row = 0; row < TileHeight; row++)
        {
            result.Add([
                new LogoSegment(FilledRow, tl),
                new LogoSegment(Gap, "black"),
                new LogoSegment(FilledRow, tr),
            ]);
        }

        result.Add([new LogoSegment(new string(' ', TileWidth * 2 + Gap.Length), "black")]);

        for (int row = 0; row < TileHeight; row++)
        {
            result.Add([
                new LogoSegment(FilledRow, bl),
                new LogoSegment(Gap, "black"),
                new LogoSegment(FilledRow, br),
            ]);
        }

        return result;
    }
}
