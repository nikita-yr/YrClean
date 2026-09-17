using System.Collections.Generic;

namespace CClean.Core.Models;

public class CacheGroup
{
    public string FolderPath { get; set; } = string.Empty;
    public string FullFolderPath { get; set; } = string.Empty;
    public long TotalSizeBytes { get; set; }
    public int FileCount { get; set; }
    public List<CacheEntry> Files { get; set; } = new();

    public string SizeDisplay => SizeFormatter.Format(TotalSizeBytes);
}