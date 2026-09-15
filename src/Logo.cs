namespace WFetch;

internal sealed record LogoSegment(string Text, string Color);

internal static class Logo
{
    // El logo real de Windows 11 es una cuadrícula 2x2 de paneles (a diferencia
    // del "flag" ondulado de versiones viejas de Windows). Lo dibujamos con
    // caracteres ASCII (#) en vez de bloques Unicode, y con la esquina externa
    // de cada panel ligeramente redondeada para imitar el logo real.
    private const int TileWidth = 15;
    private const int TileHeight = 7;
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
        var result = new List<List<LogoSegment>>();

        for (int row = 0; row < TileHeight; row++)
        {
            result.Add([
                new LogoSegment(Tile(row, isTop: true, isLeft: true), tl),
                new LogoSegment(Gap, "black"),
                new LogoSegment(Tile(row, isTop: true, isLeft: false), tr),
            ]);
        }

        result.Add([new LogoSegment(new string(' ', TileWidth * 2 + Gap.Length), "black")]);

        for (int row = 0; row < TileHeight; row++)
        {
            result.Add([
                new LogoSegment(Tile(row, isTop: false, isLeft: true), bl),
                new LogoSegment(Gap, "black"),
                new LogoSegment(Tile(row, isTop: false, isLeft: false), br),
            ]);
        }

        return result;
    }

    private static string Tile(int row, bool isTop, bool isLeft)
    {
        bool isOuterCornerRow = isTop ? row == 0 : row == TileHeight - 1;
        if (!isOuterCornerRow)
            return new string('#', TileWidth);

        string body = new string('#', TileWidth - 1);
        return isLeft ? "." + body : body + ".";
    }
}
