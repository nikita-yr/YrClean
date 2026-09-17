# YrClean

YrClean is a Windows cache and temporary-file cleaner built with C#/.NET 8 and WPF.
It supports interactive cleanup, scheduled unattended runs, bounded auto-discovery of cache folders, and optional Windows notifications.

## Features

- Scans known cache locations and optionally discovers cache/log folders under `%LocalAppData%`, `%AppData%`, and `%ProgramData%`.
- Shows scan results grouped by source and sorted by size.
- Supports selecting the complete tree and excluding specific folders from scans and scheduled cleanup.
- Enforces a minimum file age before deletion.
- Blocks protected extensions (`.exe`, `.dll`, `.sys`, `.msi`, `.ocx`, `.drv`, `.lnk`, `.ini`, `.bat`, `.cmd`, `.ps1`, `.vbs`).
- Refuses paths outside the allowed cache roots and skips reparse points.
- Registers an hourly, daily, or weekly Task Scheduler job through `wscript.exe`.
- Writes unattended-run results to `%AppData%\CClean\autoclean.log`.

## Requirements

- Windows
- .NET 8 SDK
- WPF and Windows Forms workloads included with the .NET SDK

## Build

```powershell
dotnet build CClean.sln
```

The executable is produced at `CClean.UI\bin\Debug\net8.0-windows\CClean.UI.exe`.

## Run

```powershell
CClean.UI\bin\Debug\net8.0-windows\CClean.UI.exe
```

Use **Schedule...** to configure the frequency, notifications, auto-discovered scheduled sources, and exclusions.

For an unattended run:

```powershell
CClean.UI\bin\Debug\net8.0-windows\CClean.UI.exe --auto-clean
```

The scheduled auto-discovery option is enabled by default for new settings, but existing settings remain governed by their saved value.

## Safety model

Every deletion is checked against the configured cache roots, the minimum access-age threshold, protected extensions, and reparse-point rules. Discovery is bounded by maximum depth and folder count, and exclusion keywords take precedence over cache-name matches.

## 📄 License

Licensed under the [MIT License](LICENSE).
