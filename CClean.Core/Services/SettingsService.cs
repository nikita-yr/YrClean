using System.Text.Json;
using CClean.Core.Models;

namespace CClean.Core.Services;

public static class SettingsService
{
    private static readonly string SettingsDir = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "CClean");

    private static readonly string SettingsPath = Path.Combine(SettingsDir, "settings.json");

    public static CleanSettings Load()
    {
        try
        {
            if (!File.Exists(SettingsPath))
                return new CleanSettings();

            var json = File.ReadAllText(SettingsPath);
            var settings = JsonSerializer.Deserialize<CleanSettings>(json) ?? new CleanSettings();

            using var document = JsonDocument.Parse(json);
            if (!document.RootElement.TryGetProperty(nameof(CleanSettings.NotifyOnComplete), out _) &&
                settings.Frequency == ScheduleFrequency.Hourly)
            {
                // Frequency=0 meant Daily before Hourly was introduced.
                settings.Frequency = ScheduleFrequency.Daily;
            }

            return settings;
        }
        catch (Exception)
        {
            // Corrupt or unreadable settings file — fall back to defaults rather than crashing
            return new CleanSettings();
        }
    }

    public static void Save(CleanSettings settings)
    {
        Directory.CreateDirectory(SettingsDir);
        var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(SettingsPath, json);
    }
}