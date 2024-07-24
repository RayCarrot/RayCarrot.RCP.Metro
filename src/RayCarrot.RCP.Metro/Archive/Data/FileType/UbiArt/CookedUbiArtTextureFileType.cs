using System.IO;
using System.Windows.Media.Imaging;
using BinarySerializer;
using BinarySerializer.UbiArt;
using MahApps.Metro.IconPacks;
using RayCarrot.RCP.Metro.Imaging;

namespace RayCarrot.RCP.Metro.Archive.UbiArt;

/// <summary>
/// A cooked UbiArt texture file type
/// </summary>
public sealed class CookedUbiArtTextureFileType : FileType
{
    #region Constructor

    public CookedUbiArtTextureFileType()
    {
        SupportedFormats = new ImageFormat[]
        {
            new PngImageFormat(),
            new JpgImageFormat(),
            new BmpImageFormat(),
            new DdsImageFormat(),
        };

        ImportFormats = SupportedFormats.Where(x => x.CanEncode).SelectMany(x => x.FileExtensions).ToArray();
        ExportFormats = SupportedFormats.Where(x => x.CanDecode).SelectMany(x => x.FileExtensions).ToArray();
    }

    #endregion

    #region Public Properties

    public override string TypeDisplayName => "Cooked UBIArt Texture"; // TODO-UPDATE: Localize?
    public override PackIconMaterialKind Icon => PackIconMaterialKind.ImageOutline;

    public override FileExtension[] ImportFormats { get; }
    public override FileExtension[] ExportFormats { get; }

    public ImageFormat[] SupportedFormats { get; }

    #endregion

    #region Private Methods

    private void RemapChannels(RawImageData rawImageData, TextureCooked header)
    {
        byte[] imgData = rawImageData.RawData;
        byte[] colors = new byte[6];
        colors[0] = 0xFF;
        colors[4] = 0x00;
        colors[5] = 0xFF;

        switch (rawImageData.PixelFormat)
        {
            case RawImageDataPixelFormat.Bgr24:
                for (int i = 0; i < imgData.Length; i += 3)
                {
                    colors[1] = imgData[i + 2]; // Red
                    colors[2] = imgData[i + 1]; // Green
                    colors[3] = imgData[i + 0]; // Blue

                    imgData[i + 2] = colors[header.Remap_R];
                    imgData[i + 1] = colors[header.Remap_G];
                    imgData[i + 0] = colors[header.Remap_B];
                }
                break;
            
            case RawImageDataPixelFormat.Bgra32:
                for (int i = 0; i < imgData.Length; i += 4)
                {
                    colors[0] = imgData[i + 3]; // Alpha
                    colors[1] = imgData[i + 2]; // Red
                    colors[2] = imgData[i + 1]; // Green
                    colors[3] = imgData[i + 0]; // Blue

                    imgData[i + 3] = colors[header.Remap_A];
                    imgData[i + 2] = colors[header.Remap_R];
                    imgData[i + 1] = colors[header.Remap_G];
                    imgData[i + 0] = colors[header.Remap_B];
                }
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(rawImageData.PixelFormat), rawImageData.PixelFormat, null);
        }
    }

    private TextureCooked? ReadCookedHeader(ArchiveFileStream inputStream, UbiArtSettings settings, IArchiveDataManager manager)
    {
        if (!HasCookedHeader(settings))
            return null;

        // Serialize the header
        return manager.Context!.ReadStreamData<TextureCooked>(
            stream: inputStream.Stream,
            name: inputStream.Name,
            mode: VirtualFileMode.DoNotClose,
            onPreSerialize: x => x.Pre_SerializeRawData = false);
    }

    private void WriteCookedHeader(TextureCooked header, ArchiveFileStream outputStream, UbiArtSettings settings, IArchiveDataManager manager)
    {
        if (!HasCookedHeader(settings))
            return;

        // Serialize the header
        header.Pre_SerializeRawData = false;
        manager.Context!.WriteStreamData<TextureCooked>(
            stream: outputStream.Stream,
            obj: header,
            name: outputStream.Name,
            mode: VirtualFileMode.DoNotClose);
    }

    private bool HasCookedHeader(UbiArtSettings settings)
    {
        return settings.Game != BinarySerializer.UbiArt.Game.RaymanOrigins;
    }

    private bool IsSupported(UbiArtSettings settings)
    {
        return settings.Platform is Platform.PC or Platform.Xbox360;
    }

    // TODO-UPDATE: Check other platforms which use DDS, such as Android. Previously we would dynamically determine it, but now we do it based on settings.
    private ImageFormat GetImageFormat(UbiArtSettings settings, TextureCooked? header)
    {
        return settings.Platform switch
        {
            Platform.PC => new DdsImageFormat(),
            Platform.Xbox360 => new Xbox360D3DTexture(),
            _ => throw new InvalidOperationException("Textures are not supported for the current platform")
        };
    }

    private ImageFormat GetImageFormat(FileExtension fileExtension) =>
        SupportedFormats.First(x => x.FileExtensions.Contains(fileExtension));

    #endregion

    #region Public Methods

    public override bool IsSupported(IArchiveDataManager manager) => manager.Context?.HasSettings<UbiArtSettings>() is true;

    public override bool IsOfType(FileExtension fileExtension, IArchiveDataManager manager) =>
        IsSupported(manager.Context!.GetRequiredSettings<UbiArtSettings>()) &&
        (fileExtension == new FileExtension(".tga.ckd", multiple: true) || 
         fileExtension == new FileExtension(".png.ckd", multiple: true));

    public override FileThumbnailData LoadThumbnail(
        ArchiveFileStream inputStream, 
        FileExtension fileExtension, 
        IArchiveDataManager manager)
    {
        // Get the settings
        UbiArtSettings settings = manager.Context!.GetRequiredSettings<UbiArtSettings>();

        // Read the header
        TextureCooked? header = ReadCookedHeader(inputStream, settings, manager);

        // Decode the image
        ImageFormat imageFormat = GetImageFormat(settings, header);
        RawImageData imgData = imageFormat.Decode(inputStream.Stream);

        // Remap if needed
        if (header is { IsRemapped: true })
            RemapChannels(imgData, header);

        // Create an image source
        BitmapSource thumb = imgData.ToBitmapSource();

        return new FileThumbnailData(thumb, new[]
        {
            new DuoGridItemViewModel(
                header: new ResourceLocString(nameof(Resources.Archive_FileInfo_Img_Size)),
                text: $"{imgData.Metadata.Width}x{imgData.Metadata.Height}"),
            new DuoGridItemViewModel(
                header: new ResourceLocString(nameof(Resources.Archive_FileInfo_Format)),
                text: new GeneratedLocString(() => imageFormat.Name)),
            // TODO-UPDATE: Get additional, optional, info from metadata, such as compression
            //new DuoGridItemViewModel(
            //    header: new ResourceLocString(nameof(Resources.Archive_FileInfo_Img_Compression)),
            //    text: img.Compression == CompressionMethod.NoCompression 
            //        ? new ResourceLocString(nameof(Resources.Archive_FileInfo_Img_Compression_None)) 
            //        : $"{img.Compression}"),
        });
    }

    public override void ConvertTo(
        FileExtension inputFormat, 
        FileExtension outputFormat, 
        ArchiveFileStream inputStream, 
        Stream outputStream,
        IArchiveDataManager manager)
    {
        // Get the settings
        UbiArtSettings settings = manager.Context!.GetRequiredSettings<UbiArtSettings>();

        // Read the header
        TextureCooked? header = ReadCookedHeader(inputStream, settings, manager);

        // Get the formats
        ImageFormat inputImageFormat = GetImageFormat(settings, header);
        ImageFormat outputImageFormat = GetImageFormat(outputFormat);

        // Convert manually if remapped
        if (header is { IsRemapped: true })
        {
            RawImageData decodedData = inputImageFormat.Decode(inputStream.Stream);
            RemapChannels(decodedData, header);
            outputImageFormat.Encode(decodedData, outputStream);
        }
        // Don't convert if it's the same format. For example if exporting as .dds on PC.
        else if (inputImageFormat == outputImageFormat)
        {
            // Copy the image data
            inputStream.Stream.CopyToEx(outputStream);
        }
        // Convert
        else
        {
            // Convert the image data
            inputImageFormat.Convert(inputStream.Stream, outputStream, outputImageFormat);
        }
    }

    public override void ConvertFrom(
        FileExtension inputFormat, 
        FileExtension outputFormat, 
        ArchiveFileStream currentFileStream,
        ArchiveFileStream inputStream, 
        ArchiveFileStream outputStream,
        IArchiveDataManager manager)
    {
        // Get the settings
        UbiArtSettings settings = manager.Context!.GetRequiredSettings<UbiArtSettings>();

        // Read the header
        TextureCooked? header = ReadCookedHeader(currentFileStream, settings, manager);

        // Get the formats
        ImageFormat inputImageFormat = GetImageFormat(inputFormat);
        ImageFormat outputImageFormat = GetImageFormat(settings, header);

        // Skip the header since we right that last
        int dataOffset = (int)(header?.RawDataStartOffset ?? 0);
        outputStream.Stream.Position = dataOffset;

        ImageMetadata? metadata = null;

        // Don't convert if it's the same format
        if (inputImageFormat == outputImageFormat)
        {
            // Copy the image data
            inputStream.Stream.CopyToEx(outputStream.Stream);
        }
        else
        {
            // Convert the image data
            metadata = inputImageFormat.Convert(inputStream.Stream, outputStream.Stream, outputImageFormat);
        }

        // Write the header if there is one
        if (header != null)
        {
            // Get the metadata if it's null
            outputStream.Stream.Position = dataOffset;
            metadata ??= outputImageFormat.GetMetadata(outputStream.Stream);

            uint dataSize = (uint)(outputStream.Stream.Length - dataOffset);

            header.Width = (ushort)metadata.Width;
            header.Height = (ushort)metadata.Height;
            header.RawDataSize = dataSize;
            header.MemorySize = dataSize;
            header.Remap = 0x00010203;

            outputStream.Stream.Position = 0;
            WriteCookedHeader(header, outputStream, settings, manager);
        }
    }

    #endregion
}