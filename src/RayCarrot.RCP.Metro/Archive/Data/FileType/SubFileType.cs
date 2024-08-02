namespace RayCarrot.RCP.Metro.Archive;

/// <summary>
/// The subtype of a file. For example a .png file for an image file type.
/// </summary>
public class SubFileType
{
    public SubFileType()
    {
        DisplayName = null;
        ImportFormats = Array.Empty<FileExtension>();
        ExportFormats = Array.Empty<FileExtension>();
    }

    public SubFileType(string displayName)
    {
        DisplayName = displayName;
        ImportFormats = Array.Empty<FileExtension>();
        ExportFormats = Array.Empty<FileExtension>();
    }

    public SubFileType(FileExtension[] importFormats, FileExtension[] exportFormats)
    {
        DisplayName = null;
        ImportFormats = importFormats;
        ExportFormats = exportFormats;
    }

    public SubFileType(string displayName, FileExtension[] importFormats, FileExtension[] exportFormats)
    {
        DisplayName = displayName;
        ImportFormats = importFormats;
        ExportFormats = exportFormats;
    }

    public string? DisplayName { get; }

    public FileExtension[] ImportFormats { get; }
    public FileExtension[] ExportFormats { get; }
}