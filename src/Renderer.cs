using System.Text;

namespace WFetch;

internal static class Renderer
{
    internal static string Render(WFetchConfig config)
    {
        var logoLines = File.Exists(config.Logo)
            ? Logo.BuildFromFile(config.Logo, config.AccentColor)
            : Logo.Build(config.Logo, config.AccentColor);

        var infoLines = config.Style.Equals("classic", StringComparison.OrdinalIgnoreCase)
            ? BuildClassicInfoLines(config)
            : BuildBoxedInfoLines(config);

        int logoWidth = Logo.VisualWidth(logoLines);
        int rowCount = Math.Max(logoLines.Count, infoLines.Count);

        var sb = new StringBuilder();
        sb.Append('\n');
        if (config.Style.Equals("boxed", StringComparison.OrdinalIgnoreCase))
        {
            string userHost = SystemInfo.GetUserHostLine();
            sb.Append(Ansi.Colorize(Ansi.Bold + $"┌──({userHost})-[wfetch]", config.TitleColor)).Append('\n');
            sb.Append(Ansi.Colorize(Ansi.Bold + "└─$", config.TitleColor))
              .Append(' ')
              .Append(Ansi.Colorize("system_info --scan", config.LabelColor))
              .Append('\n').Append('\n');
        }
        for (int i = 0; i < rowCount; i++)
        {
            if (logoWidth > 0)
            {
                if (i < logoLines.Count)
                {
                    var segments = logoLines[i];
                    int written = 0;
                    foreach (var seg in segments)
                    {
                        sb.Append(Ansi.Fg(seg.Color)).Append(seg.Text).Append(Ansi.Reset);
                        written += seg.Text.Length;
                    }
                    sb.Append(' ', Math.Max(0, logoWidth - written + 4));
                }
                else
                {
                    sb.Append(' ', logoWidth + 4);
                }
            }

            if (i < infoLines.Count)
                sb.Append(infoLines[i]);

            sb.Append('\n');
        }
        sb.Append('\n');
        return sb.ToString();
    }

    private static List<string> BuildBoxedInfoLines(WFetchConfig config)
    {
        const int labelWidth = 10;

        var lines = new List<string>();

        AddGroup(lines, config, "HARDWARE", BuildHardwareRows(config), labelWidth);
        lines.Add("");
        AddGroup(lines, config, "SESSION", BuildSessionRows(config), labelWidth);
        lines.Add("");
        AddGroup(lines, config, "UPTIME / DATE", BuildUptimeRows(config), labelWidth);

        if (config.ShowColorBlocks)
        {
            lines.Add("");
            lines.Add(string.Concat(Ansi.BasicColorNames.Where(c => c != "black").Select(c => Ansi.Colorize("● ", c))));
        }

        return lines;
    }

    private static void AddGroup(List<string> lines, WFetchConfig config, string title, List<(string Label, string Value)> rows, int labelWidth)
    {
        lines.Add(Ansi.Colorize("┌─ [+] ", config.AccentColor) + Ansi.Colorize(Ansi.Bold + title, config.AccentColor));
        for (int i = 0; i < rows.Count; i++)
        {
            string connector = i == rows.Count - 1 ? "└─ " : "├─ ";
            lines.Add(Ansi.Colorize(connector, config.AccentColor)
                + Ansi.Colorize(rows[i].Label.PadRight(labelWidth), config.LabelColor)
                + rows[i].Value);
        }
    }

    private static List<(string Label, string Value)> BuildHardwareRows(WFetchConfig config)
    {
        var rows = new List<(string, string)>
        {
            ("CPU", SystemInfo.GetCpuLine()),
            ("GPU", SystemInfo.GetGpuLine()),
        };

        var (ramUsed, ramTotal, ramPct) = SystemInfo.GetMemory();
        rows.Add(("RAM", $"{ramUsed} / {ramTotal} {Bar(ramPct, config.AccentColor)}"));

        var (swapUsed, swapTotal, swapPct) = SystemInfo.GetSwap();
        rows.Add(("SWAP", $"{swapUsed} / {swapTotal} {Bar(swapPct, config.AccentColor)}"));

        foreach (var (letter, used, total, pct) in SystemInfo.GetDrives())
            rows.Add(($"DRIVE {letter}", $"{used} / {total} {Bar(pct, config.AccentColor)}"));

        return rows;
    }

    private static List<(string Label, string Value)> BuildSessionRows(WFetchConfig config) =>
    [
        ("LOGIN", SystemInfo.GetLoginLine()),
    ];

    private static List<(string Label, string Value)> BuildUptimeRows(WFetchConfig config) =>
    [
        ("UPTIME", SystemInfo.GetUptimeLine()),
        ("DATE", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")),
    ];

    private static List<string> BuildClassicInfoLines(WFetchConfig config)
    {
        var lines = new List<string>();
        string userHost = SystemInfo.GetUserHostLine();

        foreach (var module in config.Modules)
        {
            switch (module.ToLowerInvariant())
            {
                case "titulo":
                    lines.Add(Ansi.Colorize(Ansi.Bold + userHost, config.TitleColor));
                    break;
                case "separador":
                    lines.Add(Ansi.Colorize(new string('-', userHost.Length), config.LabelColor));
                    break;
                case "os":
                    lines.Add(Label("OS", SystemInfo.GetOsLine(), config));
                    break;
                case "host":
                    lines.Add(Label("Host", SystemInfo.GetHostLine(), config));
                    break;
                case "kernel":
                    lines.Add(Label("Kernel", SystemInfo.GetKernelLine(), config));
                    break;
                case "uptime":
                    lines.Add(Label("Uptime", SystemInfo.GetUptimeLine(), config));
                    break;
                case "shell":
                    lines.Add(Label("Shell", SystemInfo.GetShellLine(), config));
                    break;
                case "terminal":
                    lines.Add(Label("Terminal", SystemInfo.GetTerminalLine(), config));
                    break;
                case "resolucion":
                case "resolution":
                    lines.Add(Label("Resolución", SystemInfo.GetResolutionLine(), config));
                    break;
                case "cpu":
                    lines.Add(Label("CPU", SystemInfo.GetCpuLine(), config));
                    break;
                case "gpu":
                    lines.Add(Label("GPU", SystemInfo.GetGpuLine(), config));
                    break;
                case "memoria":
                case "memory":
                    {
                        var (used, total, pct) = SystemInfo.GetMemory();
                        lines.Add(Label("Memoria", $"{used} / {total} {Bar(pct, config.AccentColor)}", config));
                        break;
                    }
                case "disco":
                case "disk":
                    {
                        var (used, total, pct) = SystemInfo.GetDisk();
                        lines.Add(Label("Disco", $"{used} / {total} {Bar(pct, config.AccentColor)}", config));
                        break;
                    }
                case "bateria":
                case "battery":
                    {
                        var battery = SystemInfo.GetBatteryLine();
                        if (battery != null) lines.Add(Label("Batería", battery, config));
                        break;
                    }
                case "locale":
                    lines.Add(Label("Locale", SystemInfo.GetLocaleLine(), config));
                    break;
                case "espacio":
                case "blank":
                    lines.Add("");
                    break;
                case "colores":
                case "colors":
                    if (config.ShowColorBlocks)
                        lines.Add(string.Concat(Ansi.BasicColorNames.Select(Ansi.BgBlock)));
                    break;
            }
        }

        return lines;
    }

    private static string Label(string name, string value, WFetchConfig config) =>
        $"{Ansi.Colorize(name, config.TitleColor)} {Ansi.Colorize(config.Separator, config.LabelColor)} {value}";

    private static string Bar(int percent, string accent, int width = 12)
    {
        percent = Math.Clamp(percent, 0, 100);
        int filled = (int)Math.Round(width * percent / 100.0);
        string bar = new string('█', filled) + new string('░', width - filled);
        return $"{Ansi.Fg(accent)}{bar}{Ansi.Reset} {percent}%";
    }
}
