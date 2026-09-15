using WFetch;

Console.OutputEncoding = System.Text.Encoding.UTF8;

string? configPath = null;
string? logoOverride = null;
string? styleOverride = null;
bool noLogo = false;

for (int i = 0; i < args.Length; i++)
{
    switch (args[i])
    {
        case "--config" or "-c" when i + 1 < args.Length:
            configPath = args[++i];
            break;
        case "--logo" or "-l" when i + 1 < args.Length:
            logoOverride = args[++i];
            break;
        case "--style" or "-s" when i + 1 < args.Length:
            styleOverride = args[++i];
            break;
        case "--no-logo":
            noLogo = true;
            break;
        case "--init-config":
            {
                string path = WFetchConfig.DefaultConfigPath();
                WFetchConfig.WriteExample(path);
                Console.WriteLine($"Config de ejemplo creada en: {path}");
                return;
            }
        case "--help" or "-h":
            PrintHelp();
            return;
    }
}

EnableAnsi();

var config = WFetchConfig.Load(configPath);
if (noLogo) config.Logo = "none";
if (logoOverride != null) config.Logo = logoOverride;
if (styleOverride != null) config.Style = styleOverride;

Console.Out.Write(Renderer.Render(config));

static void EnableAnsi()
{
    var handle = NativeMethods.GetStdHandle(NativeMethods.STD_OUTPUT_HANDLE);
    if (NativeMethods.GetConsoleMode(handle, out uint mode))
        NativeMethods.SetConsoleMode(handle, mode | NativeMethods.ENABLE_VIRTUAL_TERMINAL_PROCESSING);
}

static void PrintHelp()
{
    Console.WriteLine("""
    wfetch - fetch de sistema para Windows 11, ligero y rápido

    Uso:
      wfetch [opciones]

    Opciones:
      -c, --config <ruta>   Usa un archivo de configuración específico
      -l, --logo <nombre>   windows11 | windows10 | none | ruta a un .txt personalizado
      -s, --style <nombre>  boxed | classic
      --no-logo             No muestra ningún logo
      --init-config         Crea una config de ejemplo en %USERPROFILE%\.config\wfetch\config.json
      -h, --help            Muestra esta ayuda
    """);
}
