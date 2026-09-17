@echo off
:: Enable UTF-8 console support so log text remains readable
chcp 65001 > nul

:: =========================================================================
:: 1. WINDOWS SYSTEM TEMPORARY FILES
:: =========================================================================
:: Clean the current user's Temp folder (installer caches and extractors)
del /s /f /q "%temp%\*.*" 2>nul
for /d %%x in ("%temp%\*") do rmdir /s /q "%%x" 2>nul

:: Clean the shared system Temp folder (service and update leftovers)
del /s /f /q "%systemroot%\Temp\*.*" 2>nul
for /d %%x in ("%systemroot%\Temp\*") do rmdir /s /q "%%x" 2>nul

:: Remove Prefetch files (application startup optimization data)
del /s /f /q "%systemroot%\Prefetch\*.*" 2>nul


:: =========================================================================
:: 2. GPU SHADER CACHE (NVIDIA)
:: =========================================================================
:: Remove GLCache and DXCache. They can grow over time and cause
:: stuttering in games. The GPU rebuilds them after deletion.
del /s /f /q "%localappdata%\NVIDIA\GLCache\*.*" 2>nul
del /s /f /q "%localappdata%\NVIDIA\DXCache\*.*" 2>nul


:: =========================================================================
:: 3. WINDOWS EVENT LOGS
:: =========================================================================
:: Find all Windows LTSC event logs and clear them.
:: This removes large error reports accumulated over time.
for /f "tokens=*" %%a in ('wevtutil.exe el') do wevtutil.exe cl "%%a" 2>nul


:: =========================================================================
:: 4. MESSAGING APPS (DISCORD AND TELEGRAM)
:: =========================================================================
:: Discord uses Chromium. To avoid disrupting a running instance,
:: remove only media and code cache older than 14 days with 'forfiles'.
forfiles /p "%appdata%\discord\Cache" /s /d -14 /c "cmd /c del /f /q @path" 2>nul
forfiles /p "%appdata%\discord\Code Cache" /s /d -14 /c "cmd /c del /f /q @path" 2>nul

:: Remove the local Telegram downloads folder completely.
:: Viewed images and videos are removed; the account remains intact because data is in the cloud.
del /s /f /q "C:\Users\nikita_yr\Documents\Telegram\*.*" 2>nul
for /d %%x in ("C:\Users\nikita_yr\Documents\Telegram\*") do rmdir /s /q "%%x" 2>nul


:: =========================================================================
:: 5. BROWSERS (CHROME AND EDGE) - SAFE CLEANUP
:: =========================================================================
:: IMPORTANT: Use '-14' days. The script removes only old images and ad data.
:: Fresh cache and the 'Code Cache' folder remain untouched to avoid breaking site rendering
:: or slowing down frequently used tabs. Passwords and sessions remain intact.
forfiles /p "%localappdata%\Google\Chrome\User Data\Default\Cache\Cache_Data" /s /d -14 /c "cmd /c del /f /q @path" 2>nul
forfiles /p "%localappdata%\Microsoft\Edge\User Data\Default\Cache\Cache_Data" /s /d -14 /c "cmd /c del /f /q @path" 2>nul


:: =========================================================================
:: 6. STEAM GAME CLIENT
:: =========================================================================
:: Remove old Steam embedded-browser cache (store and overlay data).
:: The 14-day limit helps avoid store and overlay lag during gameplay.
forfiles /p "%localappdata%\Steam\htmlcache\Cache" /s /d -14 /c "cmd /c del /f /q @path" 2>nul


:: =========================================================================
:: 7. NETWORK OPTIMIZATION
:: =========================================================================
:: Fully reset the local DNS cache. This can resolve sites failing to open
:: after a provider changes IP addresses.
ipconfig /flushdns > nul