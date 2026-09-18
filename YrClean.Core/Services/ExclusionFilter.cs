namespace YrClean.Core.Services;

public static class ExclusionFilter
{
    public static bool IsExcluded(string fullPath, IEnumerable<string> excludedPaths)
    {
        var normalized = fullPath.TrimEnd(Path.DirectorySeparatorChar);

        foreach (var excluded in excludedPaths)
        {
            var ex = excluded.TrimEnd(Path.DirectorySeparatorChar);
            if (normalized.Equals(ex, StringComparison.OrdinalIgnoreCase) ||
                normalized.StartsWith(ex + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
                return true;
        }

        return false;
    }
}