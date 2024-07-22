using MahApps.Metro.IconPacks;
using System.IO;

namespace RayCarrot.RCP.Metro.Archive;

/// <summary>
/// A sound .wav file type
/// </summary>
public sealed class WaveSoundFileType : FileType
{
    #region Public Properties

    public override string TypeDisplayName => Resources.Archive_Format_Snd;
    public override PackIconMaterialKind Icon => PackIconMaterialKind.FileMusicOutline;

    #endregion

    #region Public Methods

    public override bool IsOfType(FileExtension fileExtension, IArchiveDataManager manager) => fileExtension == new FileExtension(".wav");

    public override void ConvertTo(
        FileExtension inputFormat, 
        FileExtension outputFormat, 
        ArchiveFileStream inputStream, 
        Stream outputStream, 
        IArchiveDataManager manager) 
        => throw new NotSupportedException("Converting .wav files is not supported");

    public override void ConvertFrom(
        FileExtension inputFormat, 
        FileExtension outputFormat, 
        ArchiveFileStream currentFileStream, 
        ArchiveFileStream inputStream, 
        ArchiveFileStream outputStream, 
        IArchiveDataManager manager) 
        => throw new NotSupportedException("Converting .wav files is not supported");

    #endregion
}