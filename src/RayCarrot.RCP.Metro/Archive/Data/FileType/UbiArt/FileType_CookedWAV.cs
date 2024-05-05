using System.IO;
using System.Text;
using BinarySerializer;
using BinarySerializer.UbiArt;

namespace RayCarrot.RCP.Metro.Archive;

// A bit of a hacky class, but it's because Rayman Origins has the sounds, .wav.ckd., just be normal .wav files

/// <summary>
/// A cooked sound .wav file type
/// </summary>
public class FileType_CookedWAV : FileType_WAV
{
    #region Interface Implementations

    /// <summary>
    /// Indicates if the specified manager supports files of this type
    /// </summary>
    /// <param name="manager">The manager to check</param>
    /// <returns>True if supported, otherwise false</returns>
    public override bool IsSupported(IArchiveDataManager manager) => manager.Context?.HasSettings<UbiArtSettings>() is true;

    /// <summary>
    /// Indicates if a file with the specifies file extension is of this type
    /// </summary>
    /// <param name="fileExtension">The file extension to check</param>
    /// <returns>True if it is of this type, otherwise false</returns>
    public override bool IsOfType(FileExtension fileExtension) => false;

    /// <summary>
    /// Indicates if a file with the specifies file extension and data is of this type
    /// </summary>
    /// <param name="fileExtension">The file extension to check</param>
    /// <param name="inputStream">The file data to check</param>
    /// <param name="manager">The manager</param>
    /// <returns>True if it is of this type, otherwise false</returns>
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

    /// <summary>
    /// The supported formats to import from
    /// </summary>
    public override FileExtension[] ImportFormats => base.ImportFormats.Concat(new FileExtension[]
    {
        new(".wav")
    }).ToArray();

    /// <summary>
    /// The supported formats to export to
    /// </summary>
    public override FileExtension[] ExportFormats => base.ExportFormats.Concat(new FileExtension[]
    {
        new(".wav")
    }).ToArray();

    /// <summary>
    /// Converts the file data to the specified format
    /// </summary>
    /// <param name="inputFormat">The format to convert from</param>
    /// <param name="outputFormat">The format to convert to</param>
    /// <param name="inputStream">The input file data stream</param>
    /// <param name="outputStream">The output stream for the converted data</param>
    /// <param name="manager">The manager</param>
    public override void ConvertTo(FileExtension inputFormat, FileExtension outputFormat, ArchiveFileStream inputStream, Stream outputStream, IArchiveDataManager manager)
    {
        if (outputFormat != new FileExtension(".wav"))
        {
            base.ConvertTo(inputFormat, outputFormat, inputStream, outputStream, manager);
            return;
        }

        // Same format, but different file extensions. Just copy the data.
        inputStream.Stream.CopyToEx(outputStream);
    }

    /// <summary>
    /// Converts the file data from the specified format
    /// </summary>
    /// <param name="inputFormat">The format to convert from</param>
    /// <param name="outputFormat">The format to convert to</param>
    /// <param name="currentFileStream">The current file stream</param>
    /// <param name="inputStream">The input file data stream to convert from</param>
    /// <param name="outputStream">The output stream for the converted data</param>
    /// <param name="manager">The manager</param>
    public override void ConvertFrom(FileExtension inputFormat, FileExtension outputFormat, ArchiveFileStream currentFileStream, ArchiveFileStream inputStream, ArchiveFileStream outputStream, IArchiveDataManager manager)
    {
        if (inputFormat != new FileExtension(".wav"))
        {
            base.ConvertFrom(inputFormat, outputFormat, currentFileStream, inputStream, outputStream, manager);
            return;
        }

        // Same format, but different file extensions. Just copy the data.
        inputStream.Stream.CopyToEx(outputStream.Stream);
    }

    #endregion
}