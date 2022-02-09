using MahApps.Metro.IconPacks;

namespace RayCarrot.RCP.Metro.Archive;

/// <summary>
/// The error file type. This is used for files which had a file type which could not be used.
/// </summary>
public class FileType_Error : FileType_Default
{
    public override PackIconMaterialKind Icon => PackIconMaterialKind.FileAlertOutline;
}