using System.Collections.Generic;
using System.Linq;
using YrClean.Core.Models;

namespace YrClean.UI;

public class CacheGroupNode : SelectableNodeBase
{
    public string Name { get; }
    public string SizeDisplay { get; }
    public string CountDisplay { get; }
    public string FullFolderPath { get; }
    public long TotalSizeBytes { get; }
    public List<CacheFileNode> Children { get; }

    public CacheGroupNode(CacheGroup group)
    {
        Name = group.FolderPath;
        SizeDisplay = group.SizeDisplay;
        CountDisplay = $"{group.FileCount} files";
        FullFolderPath = group.FullFolderPath;
        TotalSizeBytes = group.TotalSizeBytes;
        Children = group.Files
            .OrderByDescending(f => f.SizeBytes)
            .Select(f => new CacheFileNode(f))
            .ToList();
    }

    protected override IEnumerable<SelectableNodeBase> GetChildren() => Children;
}