namespace CClean.Core.Models;

public class CacheEntry
{
    public string FullPath { get; set; } = string.Empty;
    public long SizeBytes { get; set; }
    public DateTime LastAccessTime { get; set; }
    public bool IsDirectory { get; set; }
    public double SizeKb => Math.Round(SizeBytes / 1024.0, 1);
}