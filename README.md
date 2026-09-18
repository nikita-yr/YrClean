<p align="center">
  <img src="assets/logo.png" width="96" alt="YrClean logo">
</p>

<h1 align="center">🧹 YrClean</h1>

<p align="center">
  <img src="https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet&logoColor=white" alt=".NET 8">
  <img src="https://img.shields.io/badge/UI-WPF-0078D6?logo=windows&logoColor=white" alt="WPF">
  <img src="https://img.shields.io/badge/platform-Windows%2010%20%7C%2011-0078D6?logo=windows&logoColor=white" alt="Windows 10 | 11">
  <img src="https://img.shields.io/badge/license-MIT-green" alt="MIT License">
</p>

<p align="center">
  <b>YrClean shows you exactly what it's about to delete before it deletes it, and never touches system files.</b><br>
  Run it by hand or let it run silently on a schedule.
</p>


---

## 🖼️ Interface

<!--
  TODO: drop a real screenshot here. Easiest way:
  1. Run YrClean.UI.exe, do a scan so the tree has results in it.
  2. Screenshot the window, save as assets/screenshot.png (PNG, ~800px wide is plenty).
  3. Uncomment the line below.
-->
<p align="center">
  <!-- <img src="assets/screenshot.png" width="700" alt="YrClean main window"> -->
  <i>Screenshot coming soon — see the HTML comment above for the two-minute way to add one.</i>
</p>

The main window is a single tree: sources → cache groups → individual files, each with its own checkbox, size, and file count. Select what you want, hit **Clean**, confirm the total, done. Right-click anything to open its folder, look it up online, or exclude it from future scans.

---

## 📉 Example run

> Illustrative example, not a benchmarked claim — your own numbers will depend on what's actually built up on your machine.

```
Before: 1.2 GB across Chrome, Steam, Discord, and NVIDIA shader caches
After:  340 MB left — 860 MB freed, 0 files skipped for being protected or in use
```

Run `Measure`-style before/after yourself simply by scanning, noting the total, cleaning, and scanning again — there's no separate benchmarking script bundled, just the same scan the app already shows you.

---

## 🆚 How it compares

| | Shows what it'll delete first | Won't touch protected/system files | Scheduling | Finds new cache folders on its own |
|---|:---:|:---:|:---:|:---:|
| CCleaner | ❌ | ⚠️ configurable, opt-out | ✔️ | ❌ |
| BleachBit | ✔️ | ⚠️ configurable, opt-out | ❌ | ❌ |
| Windows Storage Sense | ❌ | ✔️ | ✔️ | ❌ |
| **YrClean** | ✔️ | ✔️ enforced, not a toggle | ✔️ | ✔️ |

This is a rough comparison of default behavior, not a full feature audit — the honest version of most of these tools' safety story is "safe if you configure it that way." YrClean's safety checks (allowed roots, protected extensions, min file age, reparse points) aren't settings you can turn off; they run on every deletion, scheduled or manual.

---

## ⚡ Features

- **Interactive scan & review** — results grouped by source, sorted by size, full tree selectable, folders excludable per-scan or permanently.
- **One-click cleanup** — see the total size before confirming; admin-only files get a single UAC prompt instead of blocking the whole run.
- **Scheduled unattended runs** — hourly/daily/weekly Task Scheduler job, launches invisibly, applies your saved settings with zero UI.
- **Bounded auto-discovery** — optionally sweep in newly-found cache folders during scheduled runs too, not just the built-in list.
- **Optional notifications** — a Windows toast after each scheduled run; silent by default.
- **Run log** — every unattended run appends a line to `%AppData%\YrClean\autoclean.log`.

<details>
<summary><b>🛡️ Safety model (click to expand)</b></summary>
<br>

Every file goes through the same checks whether you click **Clean** or a scheduled task fires unattended:

| Check | What it prevents |
|---|---|
| Allowed-root check | The file must sit inside one of the scanned cache roots — nothing outside that boundary can ever be deleted |
| Protected extensions | `.exe`, `.dll`, `.sys`, `.msi`, `.ocx`, `.drv`, `.lnk`, `.ini`, `.bat`, `.cmd`, `.ps1`, `.vbs` are never deleted, no matter where found |
| Reparse points | Symlinked/junctioned folders are skipped |
| Minimum file age | Files accessed more recently than the threshold (14 days by default) are left alone |
| Exclusions | Any folder you exclude takes priority over every other rule, including a cache-name match |

Files that need administrator rights are never silently escalated — they're classified separately and you decide whether to grant one UAC prompt to finish them off.

**What gets scanned:**

| Category | Examples |
|---|---|
| System | User & system `Temp`, `Prefetch` |
| GPU shader caches | NVIDIA `GLCache` / `DXCache`, AMD `DxCache` / `GLCache` / `VkCache` |
| Messengers | Discord `Cache` and `Code Cache` |
| Browsers | Chrome, Edge, and Firefox disk cache |
| Games | Steam's embedded-browser (`htmlcache`) cache |
| Auto-discovered | Any `*cache*`, `tmp`, `temp`, or `logs`-style folder under `%LocalAppData%`, `%AppData%`, and `%ProgramData%` |

Auto-discovery walks up to **4 folders deep** and stops after **20,000 folders visited**. Anything with `config`, `settings`, `save`, `profile`, `credentials`, `keys`, `wallet`, or similar in its path is skipped automatically, even if the folder name also looks like a cache.

</details>

---

## 📦 Install

No installer yet — grab the source, build it, and run the `.exe`:

```powershell
git clone <this repo>
cd YrClean
dotnet build YrClean.sln
YrClean.UI\bin\Debug\net8.0-windows\YrClean.UI.exe
```

*(If/when you cut a Release build and publish binaries, swap this for: "Download the latest release from the Releases page and run `YrClean.UI.exe`.")*

## 🔧 Build

```powershell
dotnet build YrClean.sln
```

Executable lands at `YrClean.UI\bin\Debug\net8.0-windows\YrClean.UI.exe`.

## ▶️ Run

```powershell
YrClean.UI\bin\Debug\net8.0-windows\YrClean.UI.exe
```

Open **Schedule...** (the gear icon) to configure run frequency, notifications, whether auto-discovered folders are included in scheduled runs, and exclusions.

For an unattended run — this is what the scheduled task calls, but you can trigger it yourself too:

```powershell
YrClean.UI\bin\Debug\net8.0-windows\YrClean.UI.exe --auto-clean
```

## 🖥️ Requirements

- Windows 10 or 11
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) (to build)
- WPF and Windows Forms desktop workloads (installed alongside the .NET 8 SDK)

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

Every deletion is checked against the allowed cache roots, protected extensions, reparse-point rules, and the minimum-age threshold — see [Safety model](#️-safety-model-click-to-expand). Files outside a known cache location are never even considered.
</details>

<details>
<summary><b>Does it need administrator rights?</b></summary>
<br>

Not for most files. If some selected files need elevated rights, YrClean classifies them separately and asks for a single UAC prompt to finish them in the background — the rest happens without elevation.
</details>

<details>
<summary><b>What does the scheduled task actually run?</b></summary>
<br>

`schtasks.exe` registers a task that launches `YrClean.UI.exe --auto-clean` through a tiny invisible `.vbs` wrapper, so no console or window appears. It applies your saved settings and exits.
</details>

<details>
<summary><b>Can I exclude specific folders?</b></summary>
<br>

Yes — per scan or saved for scheduled runs, and exclusions always beat a cache-name match.
</details>

---

## 🗺️ Roadmap

- [ ] Dark theme
- [ ] JSON-formatted log (alongside the plain-text one)
- [ ] UI redesign pass
- [ ] Portable / single-file build
- [ ] Signed release binaries

Have an idea or a bug? Open an issue — see below.

## 🤝 Contributing

Issues and PRs are welcome. Before sending a PR:

1. Keep changes focused — one feature or fix per PR.
2. If you're touching `SafeDeleteService` or anything in the delete path, explain the reasoning in the PR description; that code is deliberately conservative.
3. `dotnet build YrClean.sln` should succeed with no warnings before you push.

## 📄 License

Licensed under the [MIT License](LICENSE).
