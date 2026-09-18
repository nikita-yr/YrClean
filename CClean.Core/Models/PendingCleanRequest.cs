using System.Collections.Generic;

namespace CClean.Core.Models;

public class PendingCleanRequest
{
    public List<string> FilePaths { get; set; } = new();
    public List<string> AllowedRoots { get; set; } = new();
    public int MinAgeDays { get; set; }
}
