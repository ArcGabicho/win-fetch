<div align="center">

# wfetch

**A lightweight, blazing-fast system info fetch tool for Windows 11.**

Compiles to a single native `.exe` (Native AOT) with no WMI calls, no runtime
to install, and a startup time measured in milliseconds.

[![License: MIT](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)
[![.NET 10](https://img.shields.io/badge/.NET-10-512BD4)](https://dotnet.microsoft.com/)
[![Platform: Windows 11](https://img.shields.io/badge/platform-Windows%2011-0078D4)](https://www.microsoft.com/windows/windows-11)
[![Native AOT](https://img.shields.io/badge/build-Native%20AOT-39FF14)](#-why-its-fast)

</div>

---

```
###############   ###############    gabic@TUF-F16
###############   ###############    -------------
###############   ###############    OS: Windows 11 Home 25H2 x86_64
###############   ###############    Host: ASUS TUF Gaming F16
###############   ###############    Kernel: 10.0.26200.9457
###############   ###############    CPU: Intel Core 5 210H (12)
###############   ###############    GPU: NVIDIA RTX 3050 6GB
###############   ###############    Memoria: 14.4 / 31.6 GiB ████░░░░ 45%
###############   ###############    Disco: 318 / 476 GiB ████████░ 67%
```

The logo is Windows 11's real 2x2 panel grid, drawn solid in its brand blue
using plain ASCII (`#`) instead of Unicode blocks. Everything else defaults
to cyan, also part of the Windows 11 Fluent palette.

## Table of contents

- [Features](#features)
- [Why it's fast](#why-its-fast)
- [Installation](#installation)
- [Usage](#usage)
- [Configuration](#configuration)
  - [Options reference](#options-reference)
  - [Available colors](#available-colors)
  - [Custom ASCII logo](#custom-ascii-logo)
- [Project layout](#project-layout)
- [Contributing](#contributing)
- [License](#license)

## Features

- **Native AOT binary** — one `.exe`, ~2.8 MB, no .NET runtime to install,
  starts in tens of milliseconds.
- **No WMI** — every value is read from the Windows registry or via direct
  Win32 calls (P/Invoke), which is what makes it fast in the first place.
- **Real Windows 11 branding** — the ASCII logo is the actual 4-panel grid
  from the Windows 11 icon, not a generic "Windows flag".
- **Two layouts** — a classic neofetch/fastfetch-style flat list, or a
  boxed layout with grouped panels (Hardware / Session / Uptime).
- **Fully configurable** — colors, logo, layout, separator and module order
  via a JSON config file or CLI flags.
- **Bring your own ASCII art** — point `--logo` at any `.txt` file.

## Why it's fast

| Typical fetch tool | wfetch |
|---|---|
| Queries WMI (`Win32_Processor`, `Win32_VideoController`, ...), each call can take 50-300ms | Reads the registry directly and calls Win32 APIs (`GlobalMemoryStatusEx`, `GetDiskFreeSpaceEx`, `GetTickCount64`, `RtlGetVersion`, ...) |
| Runs on a JIT-compiled runtime | Compiles ahead-of-time to native machine code (Native AOT) — no JIT, no runtime startup |
| Prints line by line | Builds the entire frame in memory and writes it to the console once |

The result is a cold start in the tens of milliseconds, comparable to or
faster than `fastfetch`.

## Installation

### Requirements

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- For the Native AOT build only: Visual Studio with the **"Desktop
  development with C++"** workload, and the **Windows 10/11 SDK** component.

### Build

```powershell
git clone https://github.com/ArcGabicho/win-fetch.git
cd win-fetch

# Quick build for development (JIT, no C++ tools required)
dotnet build -c Release

# Native AOT build (final, single native .exe). This needs the MSVC linker,
# so use the helper script — it locates vcvars64.bat and sets everything up:
.\publish.ps1
```

The final binary lands at:

```
bin\Release\net10.0-windows\win-x64\publish\wfetch.exe
```

Copy it to a folder on your `PATH` (e.g. `%USERPROFILE%\bin`) to run
`wfetch` from anywhere.

## Usage

```
wfetch                    # use saved config, or the built-in defaults
wfetch --no-logo          # skip the logo
wfetch --logo windows10   # classic 4-color logo
wfetch --logo path.txt    # your own ASCII art
wfetch --style boxed      # grouped panels instead of the classic flat list
wfetch --init-config      # generate an editable config file
wfetch --help
```

| Flag | Alias | Description |
|---|---|---|
| `--config <path>` | `-c` | Use a specific config file |
| `--logo <name>` | `-l` | `windows11` \| `windows10` \| `none` \| path to a custom `.txt` |
| `--style <name>` | `-s` | `classic` \| `boxed` |
| `--no-logo` | | Don't render a logo |
| `--init-config` | | Generate a starter config at `%USERPROFILE%\.config\wfetch\config.json` |
| `--help` | `-h` | Show usage |

## Configuration

Run `wfetch --init-config` to generate an editable config at:

```
%USERPROFILE%\.config\wfetch\config.json
```

Or copy [`config/config.default.jsonc`](config/config.default.jsonc) as a
starting point — it supports `//` comments.

### Options reference

| Field | Type | Default | Description |
|---|---|---|---|
| `logo` | string | `"windows11"` | `windows11`, `windows10`, `none`, or a path to a custom `.txt` |
| `style` | string | `"classic"` | `classic` (flat, configurable list) or `boxed` (grouped panels) |
| `accentColor` | string | `"cyan"` | Usage bars, and the logo color for `windows10`/custom logos (`windows11` is always brand blue) |
| `labelColor` | string | `"cyandim"` | Field labels |
| `titleColor` | string | `"cyan"` | Header / section titles |
| `showColorBlocks` | bool | `true` | Show the color swatch row at the bottom |
| `separator` | string | `":"` | Label/value separator (`classic` style only) |
| `modules` | string[] | see below | Order and selection of fields to show (`classic` style only) |

Default `modules`: `titulo`, `separador`, `os`, `host`, `kernel`, `uptime`,
`shell`, `terminal`, `resolucion`, `cpu`, `gpu`, `memoria`, `disco`,
`bateria`, `locale`, `espacio`, `colores`.

### Available colors

`black`, `red`, `green`, `yellow`, `blue`, `magenta`, `cyan`, `white`,
`gray`, `cyandim` (muted cyan), `matrix` (neon green), `matrixdim` (muted
neon green).

### Custom ASCII logo

Point `--logo` (or the `logo` config field) at any `.txt` file — it's
printed as-is, colored with `accentColor`:

```powershell
wfetch --logo my-logo.txt
```

## Project layout

```
wfetch.csproj           # Native AOT project (net10.0-windows)
publish.ps1             # publish helper: locates vcvars64.bat, runs dotnet publish
config/
  config.default.jsonc  # example / starter configuration
src/
  Program.cs            # CLI argument parsing, entry point
  Config.cs             # config model + JSON (de)serialization
  SystemInfo.cs         # all data collection: registry reads + P/Invoke
  NativeMethods.cs      # Win32 P/Invoke declarations
  Logo.cs               # ASCII logo generation
  Colors.cs             # ANSI/truecolor palette
  Renderer.cs           # composes the logo + info panel into the final frame
```

## Contributing

Issues and pull requests are welcome. A few ideas if you're looking for
where to start:

- Additional ASCII logo variants
- More info modules (network, motherboard, packages...)
- ARM64 publish profile

Please keep new data-gathering code free of WMI — the whole point of this
project is to stay fast by reading the registry / calling Win32 APIs
directly.

## License

[MIT](LICENSE) © ArcGabicho
