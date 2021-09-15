using MahApps.Metro.IconPacks;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// The error file type. This is used for files which had a file type which could not be used.
    /// </summary>
    public class ArchiveFileType_Error : ArchiveFileType_Default
    {
        public override PackIconMaterialKind Icon => PackIconMaterialKind.FileAlertOutline;
    }
}