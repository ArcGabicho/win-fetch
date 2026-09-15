namespace WFetch;

internal static class Ansi
{
    internal const string Reset = "[0m";
    internal const string Bold = "[1m";

    // Colores truecolor con acabado "Fluent" (parecidos a los de Windows 11)
    private static readonly Dictionary<string, string> Palette = new(StringComparer.OrdinalIgnoreCase)
    {
        ["black"] = "30",
        ["red"] = "38;2;196;43;28",
        ["green"] = "38;2;16;137;62",
        ["yellow"] = "38;2;255;185;0",
        ["blue"] = "38;2;0;120;212",
        ["magenta"] = "38;2;136;23;152",
        ["cyan"] = "38;2;0;188;227",
        ["white"] = "37",
        ["gray"] = "90",
        ["accent"] = "38;2;0;120;212",

        // Verde neón estilo "hacker terminal" / Matrix (disponible como opción).
        ["matrix"] = "38;2;57;255;20",
        ["matrixdim"] = "38;2;20;120;20",

        // Cyan apagado, para etiquetas junto al cyan de acento.
        ["cyandim"] = "38;2;0;120;140",
    };

    internal static string Fg(string name) =>
        Palette.TryGetValue(name, out var code) ? $"[{code}m" : $"[{name}m";

    internal static string Colorize(string text, string colorName) => $"{Fg(colorName)}{text}{Reset}";

    /// Bloque de color sólido de fondo, usado para la fila de swatches.
    internal static string BgBlock(string name)
    {
        string code = Palette.TryGetValue(name, out var c) ? c.Replace("38;", "48;") : "40";
        return $"[{code}m   {Reset}";
    }

    internal static readonly string[] BasicColorNames =
        ["black", "red", "green", "yellow", "blue", "magenta", "cyan", "white"];
}
