using CClean.Core.Models;

namespace CClean.Core.Services;

public static class AutoCleanRunner
{
    private static readonly string LogPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "CClean", "autoclean.log");

    // Scans every enabled source and deletes everything old enough, per the saved settings.
    // Used by the --auto-clean silent run, and later reusable by a "clean all" UI button.
    public static DeleteResult Run(CleanSettings settings)
    {
        var scanner = new FolderScanner();
        var sources = CacheSourceProvider.GetKnownSources();

        if (settings.IncludeAutoDiscoveredInScheduledRun)
            sources = sources.Concat(AutoDiscoveryScanner.Discover()).ToList();

        sources = sources.Where(s => !settings.DisabledSourceNames.Contains(s.Name)).ToList();

        var allFiles = new List<string>();
        var roots = new List<string>();

        foreach (var source in sources)
        {
            foreach (var path in source.ResolvePaths())
            {
                if (ExclusionFilter.IsExcluded(path, settings.ExcludedPaths))
                    continue;

                roots.Add(path);
                allFiles.AddRange(scanner.Scan(path).Select(f => f.FullPath));
            }
        }

        var result = SafeDeleteService.DeleteFiles(allFiles, roots, settings.MinAgeDays);
        WriteLog(result);
        return result;
    }

    private static void WriteLog(DeleteResult result)
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(LogPath)!);
            var line = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss}  deleted={result.DeletedCount}  " +
                       $"freed={SizeFormatter.Format(result.FreedBytes)}  skipped={result.SkippedCount}  " +
                       $"errors={result.Errors.Count}{Environment.NewLine}";
            File.AppendAllText(LogPath, line);
        }
        catch (Exception)
        {
            // Logging must never crash a scheduled run
        }
    }
}