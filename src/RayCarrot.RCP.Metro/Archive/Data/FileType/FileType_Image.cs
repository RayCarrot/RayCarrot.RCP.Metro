using System;
using ImageMagick;
using MahApps.Metro.IconPacks;
using System.IO;
using System.Linq;
using System.Windows.Media.Imaging;

namespace RayCarrot.RCP.Metro.Archive;

/// <summary>
/// An image file type
/// </summary>
public class FileType_Image : IFileType
{
    #region Interface Implementations

    /// <summary>
    /// The display name for the file type
    /// </summary>
    public virtual string TypeDisplayName => Resources.Archive_Format_Img;

    /// <summary>
    /// The default icon kind for the type
    /// </summary>
    public PackIconMaterialKind Icon => PackIconMaterialKind.ImageOutline;

    /// <summary>
    /// Indicates if the specified manager supports files of this type
    /// </summary>
    /// <param name="manager">The manager to check</param>
    /// <returns>True if supported, otherwise false</returns>
    public virtual bool IsSupported(IArchiveDataManager manager) => true;

    /// <summary>
    /// Indicates if a file with the specifies file extension is of this type
    /// </summary>
    /// <param name="fileExtension">The file extension to check</param>
    /// <returns>True if it is of this type, otherwise false</returns>
    public virtual bool IsOfType(FileExtension fileExtension) => SupportedFormats.Any(x => GetFormat(x) == fileExtension);

    /// <summary>
    /// Indicates if a file with the specifies file extension and data is of this type
    /// </summary>
    /// <param name="fileExtension">The file extension to check</param>
    /// <param name="inputStream">The file data to check</param>
    /// <param name="manager">The manager</param>
    /// <returns>True if it is of this type, otherwise false</returns>
    public virtual bool IsOfType(FileExtension fileExtension, ArchiveFileStream inputStream, IArchiveDataManager manager) => false;

    /// <summary>
    /// The supported formats to import from
    /// </summary>
    public virtual FileExtension[] ImportFormats => SupportedFormats.Select(GetFormat).ToArray();

    /// <summary>
    /// The supported formats to export to
    /// </summary>
    public virtual FileExtension[] ExportFormats => SupportedFormats.Select(GetFormat).ToArray();

    /// <summary>
    /// Loads the thumbnail and display info for the file
    /// </summary>
    /// <param name="inputStream">The file data stream</param>
    /// <param name="fileExtension">The file extension</param>
    /// <param name="width">The thumbnail width</param>
    /// <param name="manager">The manager</param>
    /// <returns>The thumbnail data</returns>
    public virtual FileThumbnailData LoadThumbnail(ArchiveFileStream inputStream, FileExtension fileExtension, int width, IArchiveDataManager manager)
    {
        // Get the image
        using MagickImage img = GetImage(inputStream, fileExtension, manager);

        // Resize to a thumbnail
        img.Thumbnail(width, (int)(img.Height / ((double)img.Width / width)));

        BitmapSource thumb = img.ToBitmapSource();

        return new FileThumbnailData(thumb, new DuoGridItemViewModel[]
        {
            new DuoGridItemViewModel(
                header: new ResourceLocString(nameof(Resources.Archive_FileInfo_Img_Size)), 
                text: $"{img.Width}x{img.Height}"),
            new DuoGridItemViewModel(
                header: new ResourceLocString(nameof(Resources.Archive_FileInfo_Format)), 
                text: new GeneratedLocString(() => $"{GetFormat(fileExtension)}")),
        });
    }

    /// <summary>
    /// Converts the file data to the specified format
    /// </summary>
    /// <param name="inputFormat">The format to convert from</param>
    /// <param name="outputFormat">The format to convert to</param>
    /// <param name="inputStream">The input file data stream</param>
    /// <param name="outputStream">The output stream for the converted data</param>
    /// <param name="manager">The manager</param>
    public virtual void ConvertTo(FileExtension inputFormat, FileExtension outputFormat, ArchiveFileStream inputStream, Stream outputStream, IArchiveDataManager manager)
    {
        // Get the image
        using MagickImage img = GetImage(inputStream, inputFormat, manager);

        // Write to stream as new format
        img.Write(outputStream, GetMagickFormat(outputFormat.FileExtensions));
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
    public virtual void ConvertFrom(FileExtension inputFormat, FileExtension outputFormat, ArchiveFileStream currentFileStream, ArchiveFileStream inputStream, ArchiveFileStream outputStream, IArchiveDataManager manager)
    {
        ConvertFrom(inputFormat, GetMagickFormat(outputFormat), inputStream, outputStream);
    }

    #endregion

    #region Image Data

    /// <summary>
    /// Gets an image from the file data
    /// </summary>
    /// <param name="inputStream">The file data stream</param>
    /// <param name="format">The file format</param>
    /// <param name="manager">The manager to check</param>
    /// <returns>The image</returns>
    protected virtual MagickImage GetImage(ArchiveFileStream inputStream, FileExtension format, IArchiveDataManager manager) => new MagickImage(inputStream.Stream, GetMagickFormat(format));

    /// <summary>
    /// Converts the file data from the specified format
    /// </summary>
    /// <param name="inputFormat">The format to convert from</param>
    /// <param name="outputFormat">The format to convert to</param>
    /// <param name="inputStream">The input file data stream to convert from</param>
    /// <param name="outputStream">The output stream for the converted data</param>
    public void ConvertFrom(FileExtension inputFormat, MagickFormat outputFormat, ArchiveFileStream inputStream, ArchiveFileStream outputStream)
    {
        // Get the image in specified format
        using MagickImage img = new(inputStream.Stream, GetMagickFormat(inputFormat.FileExtensions));

        // Write to stream as native format
        img.Write(outputStream.Stream, outputFormat);
    }

    /// <summary>
    /// Gets the file extension for the image format
    /// </summary>
    /// <param name="format">The magick image format</param>
    /// <returns>The file extension</returns>
    protected FileExtension GetFormat(MagickFormat format) => new FileExtension($".{format}");

    protected MagickFormat GetMagickFormat(FileExtension ext) => GetMagickFormat(ext.PrimaryFileExtension);

    protected MagickFormat GetMagickFormat(string ext)
    {
        IMagickFormatInfo? formatInfo = MagickFormatInfo.Create(ext);

        if (formatInfo == null)
            throw new Exception($"Invalid file extension {ext}");

        return formatInfo.Format;
    }

    /// <summary>
    /// The supported image formats
    /// </summary>
    protected MagickFormat[] SupportedFormats => new MagickFormat[]
    {
        MagickFormat.Png,
        MagickFormat.Jpg,
        MagickFormat.Jpeg,
        MagickFormat.Bmp,
        MagickFormat.Dds,
        MagickFormat.Pcx,
    };

    /// <summary>
    /// Gets the format to display the image as
    /// </summary>
    /// <param name="ext">The file extension</param>
    /// <returns>The file format display name</returns>
    public virtual string GetFormat(FileExtension ext) => ext.DisplayName;

    #endregion
}