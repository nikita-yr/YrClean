using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CClean.Core.Services;

public class DeleteResult
{
    public int DeletedCount { get; set; }
    public long FreedBytes { get; set; }

    public int SkippedTooRecentCount { get; set; }
    public int SkippedProtectedCount { get; set; }
    public int SkippedOutsideRootCount { get; set; }
    public int SkippedMissingCount { get; set; }
    public int SkippedReparsePointCount { get; set; }
    public int SkippedAccessDeniedCount { get; set; }
    public int SkippedInUseCount { get; set; }
    public int SkippedOtherErrorCount { get; set; }

    public int SkippedCount =>
        SkippedTooRecentCount + SkippedProtectedCount + SkippedOutsideRootCount +
        SkippedMissingCount + SkippedReparsePointCount + SkippedAccessDeniedCount +
        SkippedInUseCount + SkippedOtherErrorCount;

    public List<string> Errors { get; set; } = new();
}

public static class SafeDeleteService
{
    public class ClassificationResult
    {
        public List<string> DeletableNow { get; set; } = new();
        public long DeletableNowBytes { get; set; }
        public List<string> NeedsAdmin { get; set; } = new();
        public long NeedsAdminBytes { get; set; }

        public int SkippedTooRecentCount { get; set; }
        public int SkippedProtectedCount { get; set; }
        public int SkippedOutsideRootCount { get; set; }
        public int SkippedMissingCount { get; set; }
        public int SkippedReparsePointCount { get; set; }
        public int SkippedInUseCount { get; set; }
    }

    // File types that must never be deleted, even if they somehow end up selected
    // inside a cache folder — protects against breaking the system or an app.
    private static readonly HashSet<string> ProtectedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".exe", ".dll", ".sys", ".msi", ".ocx", ".drv", ".lnk", ".ini", ".bat", ".cmd", ".ps1", ".vbs"
    };

    public static ClassificationResult Classify(
        IEnumerable<string> filePaths,
        IEnumerable<string> allowedRoots,
        int minAgeDays)
    {
        var result = new ClassificationResult();
        var roots = allowedRoots
            .Select(r => Path.GetFullPath(r).TrimEnd(Path.DirectorySeparatorChar))
            .ToList();
        var cutoff = DateTime.Now.AddDays(-minAgeDays);

        foreach (var path in filePaths)
        {
            try
            {
                var fullPath = Path.GetFullPath(path);
                bool insideAllowedRoot = roots.Any(root =>
                    fullPath.StartsWith(root + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase));

                if (!insideAllowedRoot)
                {
                    result.SkippedOutsideRootCount++;
                    continue;
                }

                if (ProtectedExtensions.Contains(Path.GetExtension(fullPath)))
                {
                    result.SkippedProtectedCount++;
                    continue;
                }

                var info = new FileInfo(fullPath);
                if (!info.Exists)
                {
                    result.SkippedMissingCount++;
                    continue;
                }

                if (info.Attributes.HasFlag(FileAttributes.ReparsePoint))
                {
                    result.SkippedReparsePointCount++;
                    continue;
                }

                if (info.LastAccessTime > cutoff)
                {
                    result.SkippedTooRecentCount++;
                    continue;
                }

                if (CanLikelyDelete(info, out var reason))
                {
                    result.DeletableNow.Add(fullPath);
                    result.DeletableNowBytes += info.Length;
                }
                else if (reason == DenyReason.AccessDenied)
                {
                    result.NeedsAdmin.Add(fullPath);
                    result.NeedsAdminBytes += info.Length;
                }
                else
                {
                    result.SkippedInUseCount++;
                }
            }
            catch (Exception)
            {
                result.SkippedInUseCount++;
            }
        }

        return result;
    }

    public static DeleteResult DeleteFiles(
        IEnumerable<string> filePaths,
        IEnumerable<string> allowedRoots,
        int minAgeDays,
        bool dryRun = false)
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
                    result.SkippedOutsideRootCount++;
                    continue;
                }

                // Safety check 2: never touch protected file types
                if (ProtectedExtensions.Contains(Path.GetExtension(fullPath)))
                {
                    result.Errors.Add($"Blocked (protected extension): {fullPath}");
                    result.SkippedProtectedCount++;
                    continue;
                }

                var info = new FileInfo(fullPath);
                if (!info.Exists)
                {
                    result.SkippedMissingCount++;
                    continue;
                }

                if (info.Attributes.HasFlag(FileAttributes.ReparsePoint))
                {
                    result.Errors.Add($"Blocked (reparse point): {fullPath}");
                    result.SkippedReparsePointCount++;
                    continue;
                }

                // Safety check 3: skip anything accessed more recently than the age threshold
                if (info.LastAccessTime > cutoff)
                {
                    result.SkippedTooRecentCount++;
                    continue;
                }

                if (dryRun)
                {
                    if (!CanLikelyDelete(info, out var reason))
                    {
                        if (reason == DenyReason.AccessDenied)
                            result.SkippedAccessDeniedCount++;
                        else
                            result.SkippedInUseCount++;

                        continue;
                    }

                    result.DeletedCount++;
                    result.FreedBytes += info.Length;
                    continue;
                }

                long size = info.Length;
                info.Delete();
                result.DeletedCount++;
                result.FreedBytes += size;
            }
            catch (UnauthorizedAccessException ex)
            {
                result.Errors.Add($"Access denied (needs administrator): {path} - {ex.Message}");
                result.SkippedAccessDeniedCount++;
            }
            catch (IOException ex)
            {
                result.Errors.Add($"In use by another process: {path} - {ex.Message}");
                result.SkippedInUseCount++;
            }
            catch (Exception ex)
            {
                result.Errors.Add($"{path}: {ex.Message}");
                result.SkippedOtherErrorCount++;
            }
        }

        return result;
    }

    private enum DenyReason
    {
        AccessDenied,
        InUse
    }

    private static bool CanLikelyDelete(FileInfo info, out DenyReason reason)
    {
        reason = DenyReason.AccessDenied;

        try
        {
            using var stream = info.Open(FileMode.Open, FileAccess.Write, FileShare.None);
            return true;
        }
        catch (UnauthorizedAccessException)
        {
            reason = DenyReason.AccessDenied;
            return false;
        }
        catch (IOException)
        {
            reason = DenyReason.InUse;
            return false;
        }
    }
}