using System;
using System.Collections.Generic;
using CClean.Core.Services;

namespace CClean.Core.Models;

public class CacheSource
{
    public string Name { get; set; } = string.Empty;

    // May contain %ENV_VAR% placeholders and a single "*" wildcard segment
    public string PathTemplate { get; set; } = string.Empty;

    public string Category { get; set; } = string.Empty;

    // Expands env vars, then resolves any wildcard segment into all matching real paths
    public List<string> ResolvePaths()
    {
        var expanded = Environment.ExpandEnvironmentVariables(PathTemplate);
        return PathWildcardResolver.Resolve(expanded);
    }
}