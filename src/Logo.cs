namespace WFetch;

internal sealed record LogoSegment(string Text, string Color);

internal static class Logo
{
    // El logo real de Windows 11 es una cuadrícula 2x2 de paneles. En vez de
    // rellenarlos sólidos, cada panel se dibuja como un mini "terminal window"
    // (marco + interior punteado tipo HUD) para un acabado más hacker/CRT.
    private const int TileWidth = 15;
    private const int TileHeight = 7;
    private const string Gap = "   ";

    private static readonly string[] TileRows = BuildTileRows();

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

    private static string[] BuildTileRows()
    {
        string border = "+" + new string('-', TileWidth - 2) + "+";
        string interior = string.Concat(Enumerable.Range(0, TileWidth - 2).Select(i => i % 2 == 0 ? "." : " "));
        string body = ":" + interior + ":";

        var rows = new string[TileHeight];
        rows[0] = border;
        rows[TileHeight - 1] = border;
        for (int i = 1; i < TileHeight - 1; i++) rows[i] = body;
        return rows;
    }

    private static List<List<LogoSegment>> BuildQuadrants(string tl, string tr, string bl, string br)
    {
        var result = new List<List<LogoSegment>>();

        for (int row = 0; row < TileHeight; row++)
        {
            result.Add([
                new LogoSegment(TileRows[row], tl),
                new LogoSegment(Gap, "black"),
                new LogoSegment(TileRows[row], tr),
            ]);
        }

        result.Add([new LogoSegment(new string(' ', TileWidth * 2 + Gap.Length), "black")]);

        for (int row = 0; row < TileHeight; row++)
        {
            result.Add([
                new LogoSegment(TileRows[row], bl),
                new LogoSegment(Gap, "black"),
                new LogoSegment(TileRows[row], br),
            ]);
        }

        return result;
    }
}
