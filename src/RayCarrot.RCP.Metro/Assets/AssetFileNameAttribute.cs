namespace RayCarrot.RCP.Metro;

/// <summary>
/// Defines the asset file name associated with the enum field
/// </summary>
[AttributeUsage(AttributeTargets.Field)]
public sealed class AssetFileNameAttribute : Attribute
{
    public AssetFileNameAttribute(string fileName)
    {
        FileName = fileName;
    }

    public string FileName { get; }
}