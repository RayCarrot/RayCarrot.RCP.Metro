namespace RayCarrot.RCP.Metro.Patcher;

/// <summary>
/// A serializable package file containing resources to be packed after a header
/// </summary>
public interface IPackageFile
{
    /// <summary>
    /// The resource entries in the package file
    /// </summary>
    IEnumerable<PackagedResourceEntry> Resources { get; }
}