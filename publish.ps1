# Publica wfetch como binario nativo (Native AOT) para win-x64.
# Requiere: SDK de .NET 10, Visual Studio con "Desktop development with C++"
# y el componente "Windows 10/11 SDK".

$ErrorActionPreference = "Stop"
$repoRoot = $PSScriptRoot

$vsInstallPath = & "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe" `
    -latest -products * -requires Microsoft.VisualStudio.Component.VC.Tools.x86.x64 -property installationPath

if (-not $vsInstallPath) {
    throw "No se encontró una instalación de Visual Studio con las C++ Build Tools."
}

$vcvars = Join-Path $vsInstallPath "VC\Auxiliary\Build\vcvars64.bat"
$vswhereDir = "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer"

cmd.exe /c "`"$vcvars`" && set `"PATH=%PATH%;$vswhereDir`" && set `"Platform=`" && cd /d `"$repoRoot`" && dotnet publish -c Release -r win-x64"

$exePath = Join-Path $repoRoot "bin\Release\net10.0-windows\win-x64\publish\wfetch.exe"
if (Test-Path $exePath) {
    Write-Host "`nListo: $exePath" -ForegroundColor Cyan
} else {
    throw "El publish falló: no se generó $exePath"
}
