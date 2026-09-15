using System.Text.Json;
using System.Text.Json.Serialization;

namespace WFetch;

internal sealed class WFetchConfig
{
    public string Logo { get; set; } = "windows11";

    // "classic" (lista plana estilo neofetch/fastfetch: user@host, guiones,
    // Label : Valor) o "boxed" (paneles agrupados Hardware / Session / Uptime).
    public string Style { get; set; } = "classic";
    public string AccentColor { get; set; } = "cyan";
    public string LabelColor { get; set; } = "cyandim";
    public string TitleColor { get; set; } = "cyan";
    public bool ShowColorBlocks { get; set; } = true;
    public string Separator { get; set; } = ":";
    public List<string> Modules { get; set; } =
    [
        "titulo",
        "separador",
        "os",
        "host",
        "kernel",
        "uptime",
        "shell",
        "terminal",
        "resolucion",
        "cpu",
        "gpu",
        "memoria",
        "disco",
        "bateria",
        "locale",
        "espacio",
        "colores"
    ];

    internal static WFetchConfig LoadDefault() => new();

    internal static string DefaultConfigPath()
    {
        string configDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            ".config", "wfetch");
        return Path.Combine(configDir, "config.json");
    }

    internal static WFetchConfig Load(string? explicitPath)
    {
        string path = explicitPath ?? DefaultConfigPath();
        if (!File.Exists(path))
            return LoadDefault();

        try
        {
            string json = File.ReadAllText(path);
            var loaded = JsonSerializer.Deserialize(json, WFetchJsonContext.Default.WFetchConfig);
            return loaded ?? LoadDefault();
        }
        catch
        {
            // Config inválida: seguimos con los valores por defecto en vez de fallar.
            return LoadDefault();
        }
    }

    internal static void WriteExample(string path)
    {
        string? dir = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);
        var example = LoadDefault();
        string json = JsonSerializer.Serialize(example, WFetchJsonContext.Default.WFetchConfig);
        File.WriteAllText(path, json);
    }
}

[JsonSerializable(typeof(WFetchConfig))]
[JsonSourceGenerationOptions(
    WriteIndented = true,
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    ReadCommentHandling = JsonCommentHandling.Skip,
    AllowTrailingCommas = true)]
internal partial class WFetchJsonContext : JsonSerializerContext
{
}
