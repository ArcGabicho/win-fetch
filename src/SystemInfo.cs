using Microsoft.Win32;
using System.Runtime.InteropServices;

namespace WFetch;

internal static class SystemInfo
{
    internal static string GetOsLine()
    {
        using var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion");
        string productName = key?.GetValue("ProductName") as string ?? "Windows";
        string displayVersion = key?.GetValue("DisplayVersion") as string
            ?? key?.GetValue("ReleaseId") as string ?? "";

        // Windows no actualiza esta clave de registro en Windows 11: sigue diciendo
        // "Windows 10" aunque el build sea >= 22000. Lo corregimos con el build real.
        var info = new NativeMethods.OSVERSIONINFOEX { dwOSVersionInfoSize = Marshal.SizeOf<NativeMethods.OSVERSIONINFOEX>() };
        NativeMethods.RtlGetVersion(ref info);
        if (info.dwBuildNumber >= 22000)
            productName = productName.Replace("Windows 10", "Windows 11");
        string arch = RuntimeInformation.OSArchitecture switch
        {
            Architecture.X64 => "x86_64",
            Architecture.Arm64 => "ARM64",
            Architecture.X86 => "x86",
            var a => a.ToString()
        };
        return string.IsNullOrEmpty(displayVersion)
            ? $"{productName} {arch}"
            : $"{productName} {displayVersion} {arch}";
    }

    internal static string GetKernelLine()
    {
        var info = new NativeMethods.OSVERSIONINFOEX { dwOSVersionInfoSize = Marshal.SizeOf<NativeMethods.OSVERSIONINFOEX>() };
        NativeMethods.RtlGetVersion(ref info);

        using var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion");
        string ubr = key?.GetValue("UBR")?.ToString() ?? "0";
        return $"{info.dwMajorVersion}.{info.dwMinorVersion}.{info.dwBuildNumber}.{ubr}";
    }

    internal static string GetHostLine()
    {
        using var key = Registry.LocalMachine.OpenSubKey(@"HARDWARE\DESCRIPTION\System\BIOS");
        string manufacturer = (key?.GetValue("SystemManufacturer") as string)?.Trim() ?? "";
        string product = (key?.GetValue("SystemProductName") as string)?.Trim() ?? "";
        string combined = $"{manufacturer} {product}".Trim();
        return string.IsNullOrWhiteSpace(combined) ? Environment.MachineName : combined;
    }

    internal static string GetUptimeLine()
    {
        var uptime = TimeSpan.FromMilliseconds(NativeMethods.GetTickCount64());
        var parts = new List<string>();
        if (uptime.Days > 0) parts.Add($"{uptime.Days} d");
        if (uptime.Hours > 0) parts.Add($"{uptime.Hours} h");
        if (uptime.Minutes > 0 || parts.Count == 0) parts.Add($"{uptime.Minutes} min");
        return string.Join(", ", parts);
    }

    internal static string GetShellLine()
    {
        string name = FindParentShellExeName() ?? "desconocida";
        return Path.GetFileNameWithoutExtension(name);
    }

    internal static string GetTerminalLine()
    {
        if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("WT_SESSION")))
            return "Windows Terminal";
        string? termProgram = Environment.GetEnvironmentVariable("TERM_PROGRAM");
        if (!string.IsNullOrEmpty(termProgram))
            return termProgram;
        if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("VSCODE_PID")))
            return "VS Code Integrated Terminal";
        if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("ConEmuPID")))
            return "ConEmu";
        return "Console Host";
    }

    internal static string GetCpuLine()
    {
        using var key = Registry.LocalMachine.OpenSubKey(@"HARDWARE\DESCRIPTION\System\CentralProcessor\0");
        string name = (key?.GetValue("ProcessorNameString") as string)?.Trim() ?? "CPU desconocida";
        while (name.Contains("  ")) name = name.Replace("  ", " ");
        return $"{name} ({Environment.ProcessorCount})";
    }

    internal static string GetGpuLine()
    {
        try
        {
            using var videoKey = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\Video");
            if (videoKey == null) return "Desconocida";
            foreach (var guid in videoKey.GetSubKeyNames())
            {
                using var adapterKey = videoKey.OpenSubKey($@"{guid}\0000");
                string? desc = adapterKey?.GetValue("DriverDesc") as string;
                if (string.IsNullOrWhiteSpace(desc)) continue;
                if (desc.Contains("RDPUDD", StringComparison.OrdinalIgnoreCase)) continue;
                if (desc.Contains("Mirror", StringComparison.OrdinalIgnoreCase)) continue;
                return desc;
            }
        }
        catch { /* registry layout can vary; fall through */ }
        return "Desconocida";
    }

    internal static (string used, string total, int percent) GetMemory()
    {
        var status = new NativeMethods.MEMORYSTATUSEX { dwLength = (uint)Marshal.SizeOf<NativeMethods.MEMORYSTATUSEX>() };
        NativeMethods.GlobalMemoryStatusEx(ref status);
        double totalGb = status.ullTotalPhys / 1024.0 / 1024.0 / 1024.0;
        double usedGb = (status.ullTotalPhys - status.ullAvailPhys) / 1024.0 / 1024.0 / 1024.0;
        int percent = status.dwMemoryLoad <= 100 ? (int)status.dwMemoryLoad : 0;
        return ($"{usedGb:0.0} GiB", $"{totalGb:0.0} GiB", percent);
    }

    internal static (string used, string total, int percent) GetDisk()
    {
        string drive = Path.GetPathRoot(Environment.SystemDirectory) ?? "C:\\";
        if (!NativeMethods.GetDiskFreeSpaceEx(drive, out _, out ulong totalBytes, out ulong freeBytes))
            return ("?", "?", 0);
        double totalGb = totalBytes / 1024.0 / 1024.0 / 1024.0;
        double usedGb = (totalBytes - freeBytes) / 1024.0 / 1024.0 / 1024.0;
        int percent = totalBytes == 0 ? 0 : (int)Math.Round((totalBytes - freeBytes) * 100.0 / totalBytes);
        return ($"{usedGb:0.#} GiB", $"{totalGb:0.#} GiB ({drive.TrimEnd('\\')})", percent);
    }

    internal static string GetResolutionLine()
    {
        int w = NativeMethods.GetSystemMetrics(NativeMethods.SM_CXSCREEN);
        int h = NativeMethods.GetSystemMetrics(NativeMethods.SM_CYSCREEN);
        int monitors = NativeMethods.GetSystemMetrics(NativeMethods.SM_CMONITORS);
        return monitors > 1 ? $"{w}x{h} (+{monitors - 1} más)" : $"{w}x{h}";
    }

    internal static string? GetBatteryLine()
    {
        if (!NativeMethods.GetSystemPowerStatus(out var status)) return null;
        if (status.BatteryFlag == 128) return null; // 128 = no battery present
        string charging = status.ACLineStatus == 1 ? " (cargando)" : "";
        return $"{status.BatteryLifePercent}%{charging}";
    }

    internal static string GetLocaleLine()
    {
        Span<char> buffer = stackalloc char[85];
        int len = NativeMethods.GetUserDefaultLocaleName(buffer, buffer.Length);
        return len > 1 ? new string(buffer[..(len - 1)]) : "es-ES";
    }

    internal static string GetUserHostLine() => $"{Environment.UserName}@{Environment.MachineName}";

    internal static string GetUserName() => Environment.UserName;

    internal static (string used, string total, int percent) GetSwap()
    {
        var info = new NativeMethods.PERFORMANCE_INFORMATION { cb = (uint)Marshal.SizeOf<NativeMethods.PERFORMANCE_INFORMATION>() };
        if (!NativeMethods.GetPerformanceInfo(ref info, info.cb))
            return ("0 MiB", "0 MiB", 0);

        double pageSize = info.PageSize;
        double physTotalBytes = (double)info.PhysicalTotal * pageSize;
        double commitTotalBytes = (double)info.CommitTotal * pageSize;
        double commitLimitBytes = (double)info.CommitLimit * pageSize;

        double swapTotalBytes = Math.Max(0, commitLimitBytes - physTotalBytes);
        double swapUsedBytes = Math.Clamp(commitTotalBytes - physTotalBytes, 0, swapTotalBytes);
        int percent = swapTotalBytes <= 0 ? 0 : (int)Math.Round(swapUsedBytes * 100.0 / swapTotalBytes);

        return (FormatBytes(swapUsedBytes), FormatBytes(swapTotalBytes), percent);
    }

    internal static List<(string letter, string used, string total, int percent)> GetDrives()
    {
        var result = new List<(string, string, string, int)>();
        foreach (var drive in DriveInfo.GetDrives())
        {
            if (drive.DriveType != DriveType.Fixed || !drive.IsReady) continue;
            double totalGb = drive.TotalSize / 1024.0 / 1024.0 / 1024.0;
            double freeGb = drive.TotalFreeSpace / 1024.0 / 1024.0 / 1024.0;
            double usedGb = totalGb - freeGb;
            int percent = drive.TotalSize == 0 ? 0 : (int)Math.Round((drive.TotalSize - drive.TotalFreeSpace) * 100.0 / drive.TotalSize);
            result.Add((drive.Name.TrimEnd('\\'), $"{usedGb:0.0} GiB", $"{totalGb:0.0} GiB", percent));
        }
        return result;
    }

    internal static string GetLoginLine()
    {
        try
        {
            using var explorer = System.Diagnostics.Process.GetProcessesByName("explorer").FirstOrDefault();
            var start = explorer?.StartTime ?? DateTime.Now - TimeSpan.FromMilliseconds(NativeMethods.GetTickCount64());
            return start.ToString("yyyy-MM-dd HH:mm:ss");
        }
        catch
        {
            return DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        }
    }

    private static string FormatBytes(double bytes)
    {
        double gib = bytes / 1024.0 / 1024.0 / 1024.0;
        if (gib < 1.0)
            return $"{bytes / 1024.0 / 1024.0:0.0} MiB";
        return $"{gib:0.0} GiB";
    }

    private static string? FindParentShellExeName()
    {
        try
        {
            uint pid = NativeMethods.GetCurrentProcessId();
            var snapshot = NativeMethods.CreateToolhelp32Snapshot(NativeMethods.TH32CS_SNAPPROCESS, 0);
            if (snapshot == nint.Zero || snapshot == new nint(-1)) return null;
            try
            {
                var map = new Dictionary<uint, (uint parent, string exe)>();
                var entry = new NativeMethods.PROCESSENTRY32 { dwSize = (uint)Marshal.SizeOf<NativeMethods.PROCESSENTRY32>() };
                if (NativeMethods.Process32FirstW(snapshot, ref entry))
                {
                    do
                    {
                        map[entry.th32ProcessID] = (entry.th32ParentProcessID, entry.szExeFile);
                    } while (NativeMethods.Process32NextW(snapshot, ref entry));
                }

                uint current = pid;
                for (int i = 0; i < 6 && map.TryGetValue(current, out var procInfo); i++)
                {
                    string exeLower = procInfo.exe.ToLowerInvariant();
                    bool isShell = exeLower is "pwsh.exe" or "powershell.exe" or "cmd.exe" or "nu.exe" or "bash.exe" or "sh.exe";
                    bool isSelf = exeLower is "wfetch.exe" or "conhost.exe" or "windowsterminal.exe";
                    if (isShell) return procInfo.exe;
                    if (isSelf) { current = procInfo.parent; continue; }
                    current = procInfo.parent;
                }
            }
            finally
            {
                NativeMethods.CloseHandle(snapshot);
            }
        }
        catch { /* best-effort */ }
        return null;
    }
}
