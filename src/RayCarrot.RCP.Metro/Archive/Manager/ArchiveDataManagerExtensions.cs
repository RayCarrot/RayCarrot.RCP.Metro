using System.Collections.Generic;
using System.Linq;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// Extension methods for <see cref="IArchiveDataManager"/>
/// </summary>
public static class ArchiveDataManagerExtensions
{
    /// <summary>
    /// Combines all provided paths with the separator character used by the manager
    /// </summary>
    /// <param name="manager">The manager</param>
    /// <param name="paths">The paths to combine</param>
    /// <returns>The combined paths</returns>
    public static string CombinePaths(this IArchiveDataManager manager, params string[] paths) => manager.CombinePaths((IEnumerable<string>)paths);

    /// <summary>
    /// Combines all provided paths with the separator character used by the manager
    /// </summary>
    /// <param name="manager">The manager</param>
    /// <param name="paths">The paths to combine</param>
    /// <returns>The combined paths</returns>
    public static string CombinePaths(this IArchiveDataManager manager, IEnumerable<string> paths) => paths.
        Where(x => !x.IsNullOrWhiteSpace()).
        Select(x => x.TrimEnd(manager.PathSeparatorCharacter)).
        JoinItems(manager.PathSeparatorCharacter.ToString());
}