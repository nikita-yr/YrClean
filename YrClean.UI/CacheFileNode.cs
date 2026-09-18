using System.Collections.Generic;
using System.IO;
using YrClean.Core.Models;
using YrClean.Core.Services;

namespace YrClean.UI;

public class CacheFileNode : SelectableNodeBase
{
    public string Name { get; }
    public string SizeDisplay { get; }
    public string FullPath { get; }

    public CacheFileNode(CacheEntry entry)
    {
        FullPath = entry.FullPath;
        Name = Path.GetFileName(entry.FullPath);
        SizeDisplay = SizeFormatter.Format(entry.SizeBytes);
    }

    protected override IEnumerable<SelectableNodeBase> GetChildren() => new List<SelectableNodeBase>();
}