<h1 align="center">🧹 YrClean</h1>

<p align="center">
  <img src="https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet&logoColor=white" alt=".NET 8">
  <img src="https://img.shields.io/badge/UI-WPF-0078D6?logo=windows&logoColor=white" alt="WPF">
  <img src="https://img.shields.io/badge/platform-Windows%2010%20%7C%2011-0078D6?logo=windows&logoColor=white" alt="Windows 10 | 11">
  <img src="https://img.shields.io/badge/license-MIT-green" alt="MIT License">
</p>

<p align="center">
  A cache and temp-file cleaner for Windows with a real safety model behind it.<br>
  Scan, review, and delete on your own terms — or let it run silently on a schedule.
</p>

---

## ⚡ Why YrClean?

Most "PC cleaner" tools are either black boxes that delete whatever they feel like, or one-shot `.bat` scripts you have to remember to run yourself. YrClean sits in between: it's a small, auditable C#/.NET 8 WPF app that

- shows you exactly what it found and how much space it will free **before** anything is deleted,
- refuses to touch anything outside a known cache location, a protected file type, or a file that's still "fresh," and
- can register itself as a quiet Windows Task Scheduler job so cache buildup never comes back.

Nothing is obfuscated, nothing phones home, and nothing runs without your say-so unless you explicitly turn scheduling on.

---

## 🔍 What it cleans

YrClean ships with a list of known cache locations and can optionally go looking for more:

| Category | Examples |
|---|---|
| System | User & system `Temp`, `Prefetch` |
| GPU shader caches | NVIDIA `GLCache` / `DXCache`, AMD `DxCache` / `GLCache` / `VkCache` |
| Messengers | Discord `Cache` and `Code Cache` |
| Browsers | Chrome, Edge, and Firefox disk cache |
| Games | Steam's embedded-browser (`htmlcache`) cache |
| Auto-discovered | Any `*cache*`, `tmp`, `temp`, or `logs`-style folder found under `%LocalAppData%`, `%AppData%`, and `%ProgramData%` |

Auto-discovery walks up to 4 folders deep and stops after 20,000 folders visited, so it can't wander off into your whole disk. Anything with `config`, `settings`, `save`, `profile`, `credentials`, `keys`, `wallet`, or similar in its path is skipped automatically — even if the folder name also looks like a cache.

---

## 🛡️ Safety model

Every single file goes through the same checks before it's touched, whether you're clicking **Clean** or a scheduled task is running unattended at 3 AM:

| Check | What it prevents |
|---|---|
| Allowed-root check | The file's full, resolved path must sit inside one of the scanned cache roots — nothing outside that boundary can ever be deleted, even if a bug elsewhere passed in a bad path |
| Protected extensions | `.exe`, `.dll`, `.sys`, `.msi`, `.ocx`, `.drv`, `.lnk`, `.ini`, `.bat`, `.cmd`, `.ps1`, `.vbs` are never deleted, no matter where they're found |
| Reparse points | Symlinked/junctioned folders are skipped, so cleanup can't follow a link outside the intended cache folder |
| Minimum file age | Files accessed more recently than the configured threshold (14 days by default) are left alone — nothing currently in use gets swept up |
| Exclusions | Any folder you explicitly exclude takes priority over every other rule, including a cache-name match |

Files that need administrator rights to delete are never silently escalated — they're classified separately, and you decide whether to grant one UAC prompt to finish them off in the background.

---

## 🚀 Features

- **Interactive scan & review** — results are grouped by source and sorted by size, with the full tree selectable and individual folders excludable per scan or permanently.
- **One-click cleanup** — see the total size before confirming, with admin-only files handled via a single elevation prompt instead of blocking the whole run.
- **Scheduled unattended runs** — registers an hourly, daily, or weekly Task Scheduler job that launches invisibly (via a small `.vbs` wrapper) and applies your saved settings with zero UI.
- **Bounded auto-discovery** — optionally include newly-found cache folders in scheduled runs, not just the built-in list.
- **Optional notifications** — a Windows toast/balloon after each scheduled run reporting files deleted and space freed; fully silent by default.
- **Run log** — every unattended run appends a line to `%AppData%\YrClean\autoclean.log` (timestamp, deleted count, freed size, skipped count, error count).

---

## 🖥️ Requirements

- Windows 10 or 11
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) (to build)
- WPF and Windows Forms desktop workloads (installed alongside the .NET 8 SDK)

---

## 🔧 Build

```powershell
dotnet build YrClean.sln
```

The executable is produced at:

```
YrClean.UI\bin\Debug\net8.0-windows\YrClean.UI.exe
```

---

## ▶️ Run

```powershell
YrClean.UI\bin\Debug\net8.0-windows\YrClean.UI.exe
```

Open **Schedule...** from the main window to configure run frequency, notifications, whether auto-discovered folders are included in scheduled runs, and folder exclusions.

For an unattended run (used internally by the scheduled task, but you can trigger it yourself too):

```powershell
YrClean.UI\bin\Debug\net8.0-windows\YrClean.UI.exe --auto-clean
```

> Scheduled auto-discovery is enabled by default for new installs. Existing settings keep whatever value you last saved.

---

## 📁 Where things live

| Path | Contents |
|---|---|
| `%AppData%\YrClean\settings.json` | Your saved scan/schedule/exclusion settings |
| `%AppData%\YrClean\autoclean.log` | One line per unattended run: deleted / freed / skipped / errors |

---

## ❓ FAQ

<details>
<summary><b>Can YrClean delete something important by mistake?</b></summary>
<br>

Every deletion is checked against the allowed cache roots, protected extensions, reparse-point rules, and the minimum-age threshold — see [Safety model](#️-safety-model). Files outside a known cache location are never even considered.
</details>

<details>
<summary><b>Does it need administrator rights?</b></summary>
<br>

Not for most files. If some selected files can only be deleted with elevated rights, YrClean classifies them separately and asks for a single UAC prompt to finish them in the background — the rest of the cleanup happens without elevation.
</details>

<details>
<summary><b>What does the scheduled task actually run?</b></summary>
<br>

`schtasks.exe` registers a task that launches `YrClean.UI.exe --auto-clean` through a tiny invisible `.vbs` wrapper, so no console or window ever appears. It applies your saved settings and exits.
</details>

<details>
<summary><b>Will auto-discovery wander into folders it shouldn't?</b></summary>
<br>

No. It's capped at 4 folders deep and 20,000 folders visited per run, and any path containing keywords like `config`, `credentials`, `wallet`, or `profile` is skipped outright, even if the folder name also contains "cache."
</details>

<details>
<summary><b>Can I exclude specific folders?</b></summary>
<br>

Yes — exclusions can be set per scan or saved for scheduled runs, and they always take priority over a cache-name match.
</details>

<details>
<summary><b>Where are settings and logs stored?</b></summary>
<br>

`%AppData%\YrClean\settings.json` and `%AppData%\YrClean\autoclean.log`. See [Where things live](#-where-things-live).
</details>

---

## 📄 License

Licensed under the [MIT License](LICENSE).
