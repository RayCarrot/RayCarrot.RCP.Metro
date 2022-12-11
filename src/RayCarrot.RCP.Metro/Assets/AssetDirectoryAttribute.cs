namespace RayCarrot.RCP.Metro;

[AttributeUsage(AttributeTargets.Enum)]
public sealed class AssetDirectoryAttribute : Attribute
{
    public AssetDirectoryAttribute(string directoryPath, string? defaultFileName = null, string? defaultFileExtension = null)
    {
        DirectoryPath = directoryPath;
        DefaultFileName = defaultFileName;
        DefaultFileExtension = defaultFileExtension;
    }

    public string DirectoryPath { get; }
    public string? DefaultFileName { get; }
    public string? DefaultFileExtension { get; }
}