using System.IO;
using System.Text;
using BinarySerializer;
using BinarySerializer.UbiArt;
using MahApps.Metro.IconPacks;

namespace RayCarrot.RCP.Metro.Archive.UbiArt;

/// <summary>
/// A cooked sound .wav file type
/// </summary>
public sealed class CookedUbiArtSoundFileType : FileType
{
    #region Constructor

    public CookedUbiArtSoundFileType()
    {
        SubFileType = new SubFileType(
            importFormats: new FileExtension[]
            {
                new(".wav")
            },
            exportFormats: new FileExtension[]
            {
                new(".wav")
            });
    }

    #endregion

    #region Private Properties

    private SubFileType SubFileType { get; }

    #endregion

    #region Public Properties

    public override string TypeDisplayName => Resources.Archive_Format_Snd;
    public override PackIconMaterialKind Icon => PackIconMaterialKind.FileMusicOutline;

    #endregion

    #region Interface Implementations

    public override bool IsSupported(IArchiveDataManager manager) => manager.Context?.HasSettings<UbiArtSettings>() is true;

    public override bool IsOfType(FileExtension fileExtension, IArchiveDataManager manager) => false;
    public override bool IsOfType(FileExtension fileExtension, ArchiveFileStream inputStream, IArchiveDataManager manager)
    {
        if (fileExtension != new FileExtension(".wav.ckd", multiple: true))
            return false;

        inputStream.SeekToBeginning();
        using Reader reader = new(inputStream.Stream, true, true);
        string identifier = reader.ReadString(4, Encoding.ASCII);
        reader.ReadUInt32(); // Skip size
        string type = reader.ReadString(4, Encoding.ASCII);

        return identifier is "RIFF" && type is "WAVE";
    }

    public override SubFileType GetSubType(FileExtension fileExtension, ArchiveFileStream inputStream, IArchiveDataManager manager) => SubFileType;

    public override void ConvertTo(
        FileExtension inputFormat, 
        FileExtension outputFormat, 
        ArchiveFileStream inputStream, 
        Stream outputStream, 
        IArchiveDataManager manager)
    {
        // Same format, but different file extensions. Just copy the data.
        inputStream.Stream.CopyToEx(outputStream);
    }

    public override void ConvertFrom(
        FileExtension inputFormat, 
        FileExtension outputFormat, 
        ArchiveFileStream currentFileStream, 
        ArchiveFileStream inputStream, 
        ArchiveFileStream outputStream, 
        IArchiveDataManager manager)
    {
        // Same format, but different file extensions. Just copy the data.
        inputStream.Stream.CopyToEx(outputStream.Stream);
    }

    #endregion
}