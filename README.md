# wfetch

Un fetch de sistema para Windows 11, ligero y muy rápido: se compila a un único
`.exe` nativo (Native AOT, sin runtime de .NET que instalar) y arranca en
milisegundos porque evita WMI por completo — toda la información se lee vía
registro de Windows y llamadas nativas (P/Invoke) directas.

```
┌──(gabic@TUF-F16)-[wfetch]
└─$ system_info --scan

+-------------+   +-------------+    ┌─ [+] HARDWARE
:. . . . . . .:   :. . . . . . .:    ├─ CPU        Intel Core 5 210H (12)
:. . . . . . .:   :. . . . . . .:    ├─ GPU        NVIDIA RTX 3050 6GB
:. . . . . . .:   :. . . . . . .:    ├─ RAM        14.4 / 31.6 GiB ████░░░░ 45%
:. . . . . . .:   :. . . . . . .:    └─ DRIVE C:   318 / 476 GiB ████████░ 67%
+-------------+   +-------------+
                                     ┌─ [+] UPTIME / DATE
+-------------+   +-------------+   ├─ UPTIME     4 h, 57 min
:. . . . . . .:   :. . . . . . .:   └─ DATE       2026-09-14 22:11
:. . . . . . .:   :. . . . . . .:
+-------------+   +-------------+
```

El logo es la cuadrícula de 4 paneles de Windows 11, dibujada como mini
"terminal windows" en ASCII puro (no bloques Unicode), y todo por defecto en
verde neón estilo Matrix/hacker-terminal.

## Compilar

Requiere el SDK de .NET 10 y, para el build Native AOT, las herramientas de
compilación de C++ de Visual Studio ("Desktop development with C++") junto
con el componente **Windows 10/11 SDK**.

```powershell
# Build normal (rápido, para desarrollo)
dotnet build -c Release

# Publish Native AOT (binario final, un solo .exe nativo).
# dotnet publish necesita el linker de MSVC en el PATH, así que usa el script
# que ya localiza vcvars64.bat y lo prepara todo:
.\publish.ps1
```

El binario final queda en:
`bin\Release\net10.0-windows\win-x64\publish\wfetch.exe`

Cópialo a una carpeta que esté en tu `PATH` (por ejemplo
`%USERPROFILE%\bin`) para poder ejecutar `wfetch` desde cualquier lugar.

## Uso

```
wfetch                  # usa la config guardada, o los valores por defecto
wfetch --no-logo        # sin logo
wfetch --logo windows10 # logo clásico de 4 colores
wfetch --logo ruta.txt  # arte ASCII personalizado
wfetch --style classic  # lista plana en vez de paneles agrupados
wfetch --init-config    # crea una config editable de ejemplo
wfetch --help
```

## Configuración

`wfetch --init-config` crea el archivo en:

```
%USERPROFILE%\.config\wfetch\config.json
```

También puedes copiar `config\config.default.jsonc` como punto de partida
(admite comentarios `//`). Opciones disponibles:

| Campo             | Descripción                                                        |
|-------------------|---------------------------------------------------------------------|
| `logo`            | `windows11`, `windows10`, `none`, o ruta a un `.txt` propio         |
| `style`           | `boxed` (paneles agrupados) o `classic` (lista plana configurable)  |
| `accentColor`     | Color del logo y las barras de uso                                  |
| `labelColor`      | Color de las etiquetas                                              |
| `titleColor`      | Color de los títulos / cabecera                                     |
| `showColorBlocks` | Muestra la fila de colores al final                                 |
| `separator`       | Separador entre etiqueta y valor (solo estilo `classic`)            |
| `modules`         | Orden y selección de módulos a mostrar (solo estilo `classic`)      |

Colores válidos: `black`, `red`, `green`, `yellow`, `blue`, `magenta`,
`cyan`, `white`, `gray`, `matrix` (verde neón, color por defecto),
`matrixdim` (verde neón apagado, para etiquetas).

## Por qué es rápido

- **Native AOT**: el `.exe` es código máquina nativo, no hay JIT ni arranque
  de runtime.
- **Sin WMI**: WMI (`Win32_Processor`, `Win32_VideoController`, etc.) puede
  tardar cientos de milisegundos por consulta. Aquí todo se lee directo del
  registro de Windows o vía llamadas Win32 (`GlobalMemoryStatusEx`,
  `GetDiskFreeSpaceEx`, `GetTickCount64`, `RtlGetVersion`...).
- **Una sola escritura a consola**: toda la salida se arma en memoria y se
  imprime de una vez, evitando el overhead de múltiples `Console.WriteLine`.
