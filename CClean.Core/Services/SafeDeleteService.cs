using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CClean.Core.Services;

public class DeleteResult
{
    public int DeletedCount { get; set; }
    public int SkippedCount { get; set; }
    public long FreedBytes { get; set; }
    public List<string> Errors { get; set; } = new();
}

public static class SafeDeleteService
{
    // File types that must never be deleted, even if they somehow end up selected
    // inside a cache folder — protects against breaking the system or an app.
    private static readonly HashSet<string> ProtectedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".exe", ".dll", ".sys", ".msi", ".ocx", ".drv", ".lnk", ".ini", ".bat", ".cmd", ".ps1", ".vbs"
    };

    public static DeleteResult DeleteFiles(IEnumerable<string> filePaths, IEnumerable<string> allowedRoots, int minAgeDays)
    {
        var result = new DeleteResult();
        var roots = allowedRoots
            .Select(r => Path.GetFullPath(r).TrimEnd(Path.DirectorySeparatorChar))
            .ToList();
        var cutoff = DateTime.Now.AddDays(-minAgeDays);

        foreach (var path in filePaths)
        {
            try
            {
                var fullPath = Path.GetFullPath(path);

                // Safety check 1: the file must live inside one of the known/allowed roots.
                // This makes it physically impossible to delete anything outside cache folders,
                // even if a bug elsewhere passes in a bad path.
                bool insideAllowedRoot = roots.Any(root =>
                    fullPath.StartsWith(root + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase));

                if (!insideAllowedRoot)
                {
                    result.Errors.Add($"Blocked (outside allowed roots): {fullPath}");
                    result.SkippedCount++;
                    continue;
                }

                // Safety check 2: never touch protected file types
                if (ProtectedExtensions.Contains(Path.GetExtension(fullPath)))
                {
                    result.Errors.Add($"Blocked (protected extension): {fullPath}");
                    result.SkippedCount++;
                    continue;
                }

                var info = new FileInfo(fullPath);
                if (!info.Exists)
                {
                    result.SkippedCount++;
                    continue;
                }

                if (info.Attributes.HasFlag(FileAttributes.ReparsePoint))
                {
                    result.Errors.Add($"Blocked (reparse point): {fullPath}");
                    result.SkippedCount++;
                    continue;
                }

                // Safety check 3: skip anything accessed more recently than the age threshold
                if (info.LastAccessTime > cutoff)
                {
                    result.SkippedCount++;
                    continue;
                }

                long size = info.Length;
                info.Delete();
                result.DeletedCount++;
                result.FreedBytes += size;
            }
            catch (Exception ex)
            {
                result.Errors.Add($"{path}: {ex.Message}");
                result.SkippedCount++;
            }
        }

        return result;
    }
}