using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using BinarySerializer;
using BinarySerializer.OpenSpace;
using ImageMagick;
using MahApps.Metro.IconPacks;
using PixelFormat = System.Drawing.Imaging.PixelFormat;

namespace RayCarrot.RCP.Metro.Archive.CPA;

// TODO-UPDATE: Rewrite this to use the new ImageFormat system

/// <summary>
/// An image file type
/// </summary>
public sealed class CpaTextureFileType : FileType
{
    #region Constructor

    public CpaTextureFileType()
    {
        ExportFormats = new FileExtension[]
        {
            new(".png"),
            new(".jpg"),
            new(".bmp"),
            new(".tga"), // TODO: Allow importing TGA as well, but the GF conversion code should be rewritten then first
        };
        ImportFormats = new FileExtension[]
        {
            new(".png"),
            new(".jpg"),
            new(".jpeg"),
            new(".bmp"),
        };
    }

    #endregion

    #region Logger

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    #endregion

    #region Public Properties

    public override string TypeDisplayName => Resources.Archive_Format_GF;
    public override PackIconMaterialKind Icon => PackIconMaterialKind.ImageOutline;

    public override FileExtension[] ImportFormats { get; }
    public override FileExtension[] ExportFormats { get; }

    #endregion

    #region Private Methods

    /// <summary>
    /// Gets the contents of the file
    /// </summary>
    /// <param name="fileStream">The file stream</param>
    /// <param name="manager">The data manager</param>
    /// <returns>The deserialized file</returns>
    private GF ReadFile(ArchiveFileStream fileStream, IArchiveDataManager manager)
    {
        return manager.Context!.ReadStreamData<GF>(fileStream.Stream, name: fileStream.Name, mode: VirtualFileMode.DoNotClose);
    }

    #endregion

    #region Public Methods

    public override bool IsSupported(IArchiveDataManager manager) => manager.Context?.HasSettings<OpenSpaceSettings>() is true;

    public override bool IsOfType(FileExtension fileExtension, IArchiveDataManager manager) => fileExtension.PrimaryFileExtension == ".gf";

    public override FileThumbnailData LoadThumbnail(
        ArchiveFileStream inputStream, 
        FileExtension fileExtension, 
        IArchiveDataManager manager)
    {
        // Load the file
        GF file = ReadFile(inputStream, manager);

        // Load the raw bitmap data
        RawBitmapData rawBmp = file.GetRawBitmapData();

        var format = rawBmp.PixelFormat == PixelFormat.Format32bppArgb ? PixelFormats.Bgra32 : PixelFormats.Bgr24;

        // Get a thumbnail source
        BitmapSource thumbnailSource = BitmapSource.Create(rawBmp.Width, rawBmp.Height, 96, 96, format, null, rawBmp.PixelData, (rawBmp.Width * format.BitsPerPixel + 7) / 8);

        // Get the thumbnail with the specified size
        return new FileThumbnailData(thumbnailSource, new DuoGridItemViewModel[]
        {
            new DuoGridItemViewModel(
                header: new ResourceLocString(nameof(Resources.Archive_FileInfo_Img_Size)),
                text: $"{file.Width}x{file.Height}"),
            new DuoGridItemViewModel(
                header: new ResourceLocString(nameof(Resources.Archive_FileInfo_Img_HasAlpha)),
                text: file.PixelFormat.SupportsTransparency().ToLocalizedString()),
            new DuoGridItemViewModel(
                header: new ResourceLocString(nameof(Resources.Archive_FileInfo_Img_Mipmaps)),
                text: $"{file.ExclusiveMipmapCount}"),
            new DuoGridItemViewModel(
                header: new ResourceLocString(nameof(Resources.Archive_FileInfo_Format)),
                text: $"{file.PixelFormat.ToString().Replace("Format_", "")}",
                minUserLevel: UserLevel.Technical),
            new DuoGridItemViewModel(
                header: new ResourceLocString(nameof(Resources.Archive_FileInfo_Img_Compression)),
                text: "RLE"),
        });
    }

    public override void ConvertTo(
        FileExtension inputFormat, 
        FileExtension outputFormat, 
        ArchiveFileStream inputStream, 
        Stream outputStream, 
        IArchiveDataManager manager)
    {
        // Hacky workaround for TGA exporting
        if (outputFormat.PrimaryFileExtension == ".tga")
        {
            RawBitmapData rawBmp = ReadFile(inputStream, manager).GetRawBitmapData(flipY: false);
            MagickFormat format = rawBmp.PixelFormat switch
            {
                PixelFormat.Format24bppRgb => MagickFormat.Bgr,
                PixelFormat.Format32bppArgb => MagickFormat.Bgra,
                _ => throw new Exception($"Invalid pixel format {rawBmp.PixelFormat}"),
            };
            using MagickImage img = new(rawBmp.PixelData, new MagickReadSettings()
            {
                Format = format,
                Width = rawBmp.Width,
                Height = rawBmp.Height,
            });
            img.Orientation = OrientationType.BottomLeft;
            img.Write(outputStream, MagickFormat.Tga);
            return;
        }

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
        using Bitmap bmp = ReadFile(inputStream, manager).GetRawBitmapData().GetBitmap();

        // Save the bitmap to the output stream
        bmp.Save(outputStream, imgFormat);
    }

    public override void ConvertFrom(
        FileExtension inputFormat, 
        FileExtension outputFormat, 
        ArchiveFileStream currentFileStream, 
        ArchiveFileStream inputStream,
        ArchiveFileStream outputStream, 
        IArchiveDataManager manager)
    {
        // Load the bitmap
        using Bitmap bmp = new(inputStream.Stream);

        // Load the current file
        GF gf = ReadFile(currentFileStream, manager);

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
            if (Services.Data.Archive_GF_UpdateTransparency != GFTransparencyMode.PreserveFormat)
            {
                // NOTE: Only 24 and 32 bpp bitmaps are supported
                // Check if the imported file is transparent
                bool? isTransparent = bmp.PixelFormat switch
                {
                    PixelFormat.Format32bppArgb => (Services.Data.Archive_GF_UpdateTransparency == GFTransparencyMode.UpdateBasedOnPixelFormat ||
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

        OpenSpaceSettings settings = manager.Context!.GetRequiredSettings<OpenSpaceSettings>();

        // Import the bitmap
        gf.ImportFromBitmap(settings, rawBitmapData, Services.Data.Archive_GF_GenerateMipmaps);

        Logger.Debug("The repeat byte has been updated for a .gf file from {0} to {1}", oldRepeatByte, gf.RepeatByte);

        // Serialize the data to get the bytes
        manager.Context.WriteStreamData(outputStream.Stream, gf, name: outputStream.Name, mode: VirtualFileMode.DoNotClose);
    }

    #endregion
}