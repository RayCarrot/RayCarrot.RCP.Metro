using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using BinarySerializer.OpenSpace;
using MahApps.Metro.IconPacks;
using NLog;
using PixelFormat = System.Drawing.Imaging.PixelFormat;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// An image file type
/// </summary>
public class ArchiveFileType_GF : IArchiveFileType
{
    #region Logger

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    #endregion

    #region Interface Implementations

    /// <summary>
    /// The display name for the file type
    /// </summary>
    public string TypeDisplayName => Resources.Archive_Format_GF;

    /// <summary>
    /// The default icon kind for the type
    /// </summary>
    public PackIconMaterialKind Icon => PackIconMaterialKind.ImageOutline;

    /// <summary>
    /// Indicates if the specified manager supports files of this type
    /// </summary>
    /// <param name="manager">The manager to check</param>
    /// <returns>True if supported, otherwise false</returns>
    public bool IsSupported(IArchiveDataManager manager) => manager.Context?.HasSettings<OpenSpaceSettings>() is true;

    /// <summary>
    /// Indicates if a file with the specifies file extension is of this type
    /// </summary>
    /// <param name="fileExtension">The file extension to check</param>
    /// <returns>True if it is of this type, otherwise false</returns>
    public virtual bool IsOfType(FileExtension fileExtension) => fileExtension.PrimaryFileExtension == ".gf";

    /// <summary>
    /// Indicates if a file with the specifies file extension and data is of this type
    /// </summary>
    /// <param name="fileExtension">The file extension to check</param>
    /// <param name="inputStream">The file data to check</param>
    /// <param name="manager">The manager</param>
    /// <returns>True if it is of this type, otherwise false</returns>
    public virtual bool IsOfType(FileExtension fileExtension, Stream inputStream, IArchiveDataManager manager) => false;

    /// <summary>
    /// The supported formats to import from
    /// </summary>
    public FileExtension[] ImportFormats => new FileExtension[]
    {
        new FileExtension(".png"),
        new FileExtension(".jpg"),
        new FileExtension(".jpeg"),
        new FileExtension(".bmp"),
    };

    /// <summary>
    /// The supported formats to export to
    /// </summary>
    public FileExtension[] ExportFormats => new FileExtension[]
    {
        new FileExtension(".png"),
        new FileExtension(".jpg"),
        new FileExtension(".bmp"),
    };

    /// <summary>
    /// Loads the thumbnail and display info for the file
    /// </summary>
    /// <param name="inputStream">The file data stream</param>
    /// <param name="fileExtension">The file extension</param>
    /// <param name="width">The thumbnail width</param>
    /// <param name="manager">The manager</param>
    /// <returns>The thumbnail data</returns>
    public ArchiveFileThumbnailData LoadThumbnail(ArchiveFileStream inputStream, FileExtension fileExtension, int width, IArchiveDataManager manager)
    {
        // Load the file
        GF file = GetFileContent(inputStream.Stream, manager);

        // Load the raw bitmap data
        RawBitmapData rawBmp = file.GetRawBitmapData(width, (int)(file.Height / ((double)file.Width / width)));

        var format = rawBmp.PixelFormat == PixelFormat.Format32bppArgb ? PixelFormats.Bgra32 : PixelFormats.Bgr24;

        // Get a thumbnail source
        BitmapSource thumbnailSource = BitmapSource.Create(rawBmp.Width, rawBmp.Height, 96, 96, format, null, rawBmp.PixelData, (rawBmp.Width * format.BitsPerPixel + 7) / 8);

        // Get the thumbnail with the specified size
        return new ArchiveFileThumbnailData(thumbnailSource, new DuoGridItemViewModel[]
        {
            new DuoGridItemViewModel(
                header: new ResourceLocString(nameof(Resources.Archive_FileInfo_Img_Size)), 
                text: $"{file.Width}x{file.Height}"),
            new DuoGridItemViewModel(
                header: new ResourceLocString(nameof(Resources.Archive_FileInfo_Img_HasAlpha)), 
                text: new GeneratedLocString(() => $"{file.PixelFormat.SupportsTransparency()}")),
            new DuoGridItemViewModel(
                header: new ResourceLocString(nameof(Resources.Archive_FileInfo_Img_Mipmaps)), 
                text: $"{file.ExclusiveMipmapCount}"),
            new DuoGridItemViewModel(
                header: new ResourceLocString(nameof(Resources.Archive_FileInfo_Format)), 
                text: $"{file.PixelFormat.ToString().Replace("Format_", "")}", 
                minUserLevel: UserLevel.Technical),
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
    public void ConvertTo(FileExtension inputFormat, FileExtension outputFormat, Stream inputStream, Stream outputStream, IArchiveDataManager manager)
    {
        // Get the image format
        ImageFormat imgFormat = outputFormat.PrimaryFileExtension switch
        {
            ".png" => ImageFormat.Png,
            ".jpeg" => ImageFormat.Jpeg,
            ".jpg" => ImageFormat.Jpeg,
            ".bmp" => ImageFormat.Bmp,
            _ => throw new Exception($"The specified file format {outputFormat.PrimaryFileExtension} is not supported")
        };

        // Get the bitmap
        using Bitmap bmp = GetFileContent(inputStream, manager).GetRawBitmapData().GetBitmap();

        // Save the bitmap to the output stream
        bmp.Save(outputStream, imgFormat);
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
    public void ConvertFrom(FileExtension inputFormat, FileExtension outputFormat, ArchiveFileStream currentFileStream, Stream inputStream, Stream outputStream, IArchiveDataManager manager)
    {
        // Load the bitmap
        using Bitmap bmp = new Bitmap(inputStream);

        // Load the current file
        GF gf = GetFileContent(currentFileStream.Stream, manager);

        // IDEA: If bmp is not in supported format, then convert it?

        RawBitmapData rawBitmapData;

        // Get the bitmap lock
        using (BitmapLock bmpLock = new(bmp))
        {
            // Get the raw bitmap data
            rawBitmapData = new RawBitmapData(bmp.Width, bmp.Height, bmpLock.Pixels, bmp.PixelFormat);

            // Force the new pixel format to be 888 or 8888 if set to do so
            if (Services.Data.Archive_GF_ForceGF8888Import)
                gf.PixelFormat = gf.PixelFormat.SupportsTransparency() ? GF_Format.Format_32bpp_BGRA_8888 : GF_Format.Format_24bpp_BGR_888;

            // Check if the format should be updated for transparency
            if (Services.Data.Archive_GF_UpdateTransparency != UserData_Archive_GF_TransparencyMode.PreserveFormat)
            {
                // NOTE: Only 24 and 32 bpp bitmaps are supported
                // Check if the imported file is transparent
                bool? isTransparent = bmp.PixelFormat switch
                {
                    PixelFormat.Format32bppArgb => (Services.Data.Archive_GF_UpdateTransparency == UserData_Archive_GF_TransparencyMode.UpdateBasedOnPixelFormat ||
                                                    bmpLock.UtilizesAlpha()),
                    PixelFormat.Format24bppRgb => false,
                    _ => null
                };

                // NOTE: Currently only supported for formats with 3 or 4 channels
                // Check if the format should be updated for transparency
                if (gf.Channels >= 3 && isTransparent != null)
                    // Update the format
                    gf.PixelFormat = isTransparent.Value ? GF_Format.Format_32bpp_BGRA_8888 : GF_Format.Format_24bpp_BGR_888;
            }
        }

        byte oldRepeatByte = gf.RepeatByte;

        OpenSpaceSettings settings = manager.Context!.GetSettings<OpenSpaceSettings>();

        // Import the bitmap
        gf.ImportFromBitmap(settings, rawBitmapData, Services.Data.Archive_GF_GenerateMipmaps);

        Logger.Debug("The repeat byte has been updated for a .gf file from {0} to {1}", oldRepeatByte, gf.RepeatByte);

        // Serialize the data to get the bytes
        manager.Context.WriteStreamData(outputStream, gf, leaveOpen: true);
    }

    #endregion

    #region GF Methods

    /// <summary>
    /// Gets the contents of the file
    /// </summary>
    /// <param name="fileStream">The file stream</param>
    /// <param name="manager">The data manager</param>
    /// <returns>The deserialized file</returns>
    public GF GetFileContent(Stream fileStream, IArchiveDataManager manager)
    {
        return manager.Context!.ReadStreamData<GF>(fileStream, leaveOpen: true);
    }

    #endregion
}