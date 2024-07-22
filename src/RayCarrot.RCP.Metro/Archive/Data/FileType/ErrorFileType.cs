using System.IO;
using MahApps.Metro.IconPacks;

namespace RayCarrot.RCP.Metro.Archive;

/// <summary>
/// The error file type. This is used for files which had a file type which could not be used.
/// </summary>
public sealed class ErrorFileType : FileType
{
    #region Public Properties

    public override string TypeDisplayName => Resources.Archive_Format_Default;
    public override PackIconMaterialKind Icon => PackIconMaterialKind.FileAlertOutline;

    #endregion

    #region Public Methods

    public override bool IsOfType(FileExtension fileExtension, IArchiveDataManager manager) => true;
    public override bool IsOfType(FileExtension fileExtension, ArchiveFileStream inputStream, IArchiveDataManager manager) => true;

    public override void ConvertTo(
        FileExtension inputFormat,
        FileExtension outputFormat,
        ArchiveFileStream inputStream,
        Stream outputStream,
        IArchiveDataManager manager)
        => throw new NotSupportedException("A default file types can't be converted");

    public override void ConvertFrom(
        FileExtension inputFormat,
        FileExtension outputFormat,
        ArchiveFileStream currentFileStream,
        ArchiveFileStream inputStream,
        ArchiveFileStream outputStream,
        IArchiveDataManager manager)
        => throw new NotSupportedException("A default file types can't be converted");

    #endregion
}