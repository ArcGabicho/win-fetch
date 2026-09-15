namespace WFetch;

internal sealed record LogoSegment(string Text, string Color);

internal static class Logo
{
    // Arte ASCII clásico de Windows (estilo neofetch), hecho con caracteres
    // de texto en vez de bloques sólidos.
    private static readonly string[] AsciiLines =
    [
        "        ,.=:!!t3Z3z.,",
        "       :tt:::tt333EE3",
        "       Et:::ztt33EEEL @Ee.,      ..,",
        "      ;tt:::tt333EE7 ;EEEEEEttttt33#",
        "     :Et:::zt333EEQ. $EEEEEttttt33QL",
        "     it::::tt333EEF @EEEEEEttttt33F",
        "    ;3=*^```\"*4EEV :EEEEEEttttt33@.",
        "    ,.=::::!t=., ` @EEEEEEtttz33QF",
        "   ;::::::::zt33)   \"4EEEtttji3P*",
        "  :t::::::::tt33.:Z3z..  ``` ,..g.",
        "  i::::::::zt33F AEEEtttt::::ztF",
        " ;:::::::::t33V ;EEEttttt::::t3",
        " E::::::::zt33L @EEEttttt::::z3F",
        "{3=*^```\"*4E3) ;EEEttttt:::::tZ`",
        "             ` :EEEEtttt::::z7",
        "                 \"VEzjt:;;z>*`",
    ];

    internal static List<List<LogoSegment>> Build(string name, string accentColor)
    {
        return name.ToLowerInvariant() switch
        {
            "windows10" or "classic" => BuildQuadrantAscii("red", "green", "blue", "yellow"),
            "none" => [],
            _ => BuildMonoAscii(accentColor),
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

    private static List<List<LogoSegment>> BuildMonoAscii(string color)
    {
        var result = new List<List<LogoSegment>>(AsciiLines.Length);
        foreach (var line in AsciiLines)
            result.Add([new LogoSegment(line, color)]);
        return result;
    }

    private static List<List<LogoSegment>> BuildQuadrantAscii(string tl, string tr, string bl, string br)
    {
        var result = new List<List<LogoSegment>>(AsciiLines.Length);
        int half = AsciiLines.Length / 2;

        for (int i = 0; i < AsciiLines.Length; i++)
        {
            string line = AsciiLines[i];
            int mid = Math.Max(1, line.Length / 2);
            string left = line[..mid];
            string right = line[mid..];
            string colorLeft = i < half ? tl : bl;
            string colorRight = i < half ? tr : br;
            result.Add([new LogoSegment(left, colorLeft), new LogoSegment(right, colorRight)]);
        }

        return result;
    }
}
