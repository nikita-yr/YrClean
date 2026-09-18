using System.Collections.Generic;
using System.Linq;
using YrClean.Core.Models;
using YrClean.Core.Services;

namespace YrClean.UI;

public class CacheSourceNode : SelectableNodeBase
{
    public string Name { get; }
    public string SizeDisplay { get; }
    public string CountDisplay { get; }
    public string FullFolderPath { get; }
    public long TotalSizeBytes { get; }
    public List<CacheGroupNode> Children { get; }

    public CacheSourceNode(CacheSource source, List<CacheGroup> groups, string primaryPath)
    {
        Name = $"{source.Name}  [{source.Category}]";
        FullFolderPath = primaryPath;

        long totalBytes = groups.Sum(g => g.TotalSizeBytes);
        int totalFiles = groups.Sum(g => g.FileCount);
        TotalSizeBytes = totalBytes;

        SizeDisplay = SizeFormatter.Format(totalBytes);
        CountDisplay = $"{totalFiles} files";

        Children = groups.Select(g => new CacheGroupNode(g)).ToList();
    }

    protected override IEnumerable<SelectableNodeBase> GetChildren() => Children;
}