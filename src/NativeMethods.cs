using System.Runtime.InteropServices;

namespace WFetch;

internal static partial class NativeMethods
{
    // --- Console / ANSI ---
    internal const int STD_OUTPUT_HANDLE = -11;
    internal const uint ENABLE_VIRTUAL_TERMINAL_PROCESSING = 0x0004;

    [LibraryImport("kernel32.dll", SetLastError = true)]
    internal static partial nint GetStdHandle(int nStdHandle);

    [LibraryImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static partial bool GetConsoleMode(nint hConsoleHandle, out uint lpMode);

    [LibraryImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static partial bool SetConsoleMode(nint hConsoleHandle, uint dwMode);

    // --- Uptime ---
    [LibraryImport("kernel32.dll")]
    internal static partial ulong GetTickCount64();

    // --- Memory ---
    [StructLayout(LayoutKind.Sequential)]
    internal struct MEMORYSTATUSEX
    {
        public uint dwLength;
        public uint dwMemoryLoad;
        public ulong ullTotalPhys;
        public ulong ullAvailPhys;
        public ulong ullTotalPageFile;
        public ulong ullAvailPageFile;
        public ulong ullTotalVirtual;
        public ulong ullAvailVirtual;
        public ulong ullAvailExtendedVirtual;
    }

    [LibraryImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static partial bool GlobalMemoryStatusEx(ref MEMORYSTATUSEX lpBuffer);

    // --- Disk ---
    [LibraryImport("kernel32.dll", EntryPoint = "GetDiskFreeSpaceExW", SetLastError = true, StringMarshalling = StringMarshalling.Utf16)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static partial bool GetDiskFreeSpaceEx(string lpDirectoryName, out ulong lpFreeBytesAvailable, out ulong lpTotalNumberOfBytes, out ulong lpTotalNumberOfFreeBytes);

    // --- Screen resolution ---
    [LibraryImport("user32.dll")]
    internal static partial int GetSystemMetrics(int nIndex);
    internal const int SM_CXSCREEN = 0;
    internal const int SM_CYSCREEN = 1;
    internal const int SM_CMONITORS = 80;

    // --- Battery ---
    [StructLayout(LayoutKind.Sequential)]
    internal struct SYSTEM_POWER_STATUS
    {
        public byte ACLineStatus;
        public byte BatteryFlag;
        public byte BatteryLifePercent;
        public byte SystemStatusFlag;
        public int BatteryLifeTime;
        public int BatteryFullLifeTime;
    }

    [LibraryImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static partial bool GetSystemPowerStatus(out SYSTEM_POWER_STATUS lpSystemPowerStatus);

    // --- Locale ---
    [LibraryImport("kernel32.dll", EntryPoint = "GetUserDefaultLocaleName", StringMarshalling = StringMarshalling.Utf16)]
    internal static partial int GetUserDefaultLocaleName(Span<char> lpLocaleName, int cchLocaleName);

    // --- True OS version (bypasses app-compat shims that Environment.OSVersion is subject to) ---
    [StructLayout(LayoutKind.Sequential)]
    internal struct OSVERSIONINFOEX
    {
        public int dwOSVersionInfoSize;
        public int dwMajorVersion;
        public int dwMinorVersion;
        public int dwBuildNumber;
        public int dwPlatformId;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string szCSDVersion;
        public ushort wServicePackMajor;
        public ushort wServicePackMinor;
        public ushort wSuiteMask;
        public byte wProductType;
        public byte wReserved;
    }

    // Struct con campo de longitud fija (ByValTStr): el generador de código fuente
    // de LibraryImport no puede marshalarlo, así que usamos DllImport clásico aquí.
    [DllImport("ntdll.dll")]
    internal static extern int RtlGetVersion(ref OSVERSIONINFOEX lpVersionInformation);

    // --- Process tree walk (to detect the parent shell, no WMI needed) ---
    internal const uint TH32CS_SNAPPROCESS = 0x00000002;

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    internal struct PROCESSENTRY32
    {
        public uint dwSize;
        public uint cntUsage;
        public uint th32ProcessID;
        public nint th32DefaultHeapID;
        public uint th32ModuleID;
        public uint cntThreads;
        public uint th32ParentProcessID;
        public int pcPriClassBase;
        public uint dwFlags;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
        public string szExeFile;
    }

    [LibraryImport("kernel32.dll", SetLastError = true)]
    internal static partial nint CreateToolhelp32Snapshot(uint dwFlags, uint th32ProcessID);

    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static extern bool Process32FirstW(nint hSnapshot, ref PROCESSENTRY32 lppe);

    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static extern bool Process32NextW(nint hSnapshot, ref PROCESSENTRY32 lppe);

    [LibraryImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static partial bool CloseHandle(nint hObject);

    [LibraryImport("kernel32.dll")]
    internal static partial uint GetCurrentProcessId();

    // --- Swap / commit charge (memoria virtual) ---
    [StructLayout(LayoutKind.Sequential)]
    internal struct PERFORMANCE_INFORMATION
    {
        public uint cb;
        public nuint CommitTotal;
        public nuint CommitLimit;
        public nuint CommitPeak;
        public nuint PhysicalTotal;
        public nuint PhysicalAvailable;
        public nuint SystemCache;
        public nuint KernelTotal;
        public nuint KernelPaged;
        public nuint KernelNonpaged;
        public nuint PageSize;
        public uint HandleCount;
        public uint ProcessCount;
        public uint ThreadCount;
    }

    [LibraryImport("psapi.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static partial bool GetPerformanceInfo(ref PERFORMANCE_INFORMATION pPerformanceInformation, uint cb);
}
