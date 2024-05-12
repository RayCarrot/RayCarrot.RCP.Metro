using System.IO;
using BinarySerializer;
using BinarySerializer.UbiArt;
using ImageMagick;

namespace RayCarrot.RCP.Metro.Archive.UbiArt;

/// <summary>
/// A base UbiArt texture file type
/// </summary>
public abstract class FileType_BaseUbiArtTex : FileType_Image
{
    /// <summary>
    /// The display name for the file type
    /// </summary>
    public override string TypeDisplayName => $"TEX ({Format})";

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
        if (fileExtension != new FileExtension(".tga.ckd", multiple: true) && fileExtension != new FileExtension(".png.ckd", multiple: true))
            return false;

        inputStream.SeekToBeginning();

        // Set the Stream position
        TextureCooked? tex = ReadTEXHeader(inputStream, manager);

        // Check for type match
        if (IsOfType(inputStream, manager, tex))
            return true;

        // If the format has a magic header we check for it
        if (FormatMagic != null)
        {
            // Use a reader
            using Reader reader = new(inputStream.Stream, manager.Context!.GetRequiredSettings<UbiArtSettings>().Endian == BinarySerializer.Endian.Little, true);

            // Get the magic header
            uint magic = reader.ReadUInt32();

            // Check if it matches the magic
            return magic == FormatMagic;
        }

        return false;
    }

    public virtual bool IsOfType(ArchiveFileStream inputStream, IArchiveDataManager manager, TextureCooked? tex) => false;

    /// <summary>
    /// The supported formats to import from
    /// </summary>
    public override FileExtension[] ImportFormats => IsFormatSupported ? new FileExtension[]
    {
        Format
    }.Concat(base.ImportFormats.Where(x => x != Format)).ToArray() : Array.Empty<FileExtension>();

    /// <summary>
    /// The supported formats to export to
    /// </summary>
    public override FileExtension[] ExportFormats => IsFormatSupported ? new FileExtension[]
    {
        Format
    }.Concat(base.ExportFormats.Where(x => x != Format)).ToArray() : new FileExtension[]
    {
        Format
    };

    /// <summary>
    /// Loads the thumbnail and display info for the file
    /// </summary>
    /// <param name="inputStream">The file data stream</param>
    /// <param name="fileExtension">The file extension</param>
    /// <param name="width">The thumbnail width</param>
    /// <param name="manager">The manager</param>
    /// <returns>The thumbnail data</returns>
    public override FileThumbnailData LoadThumbnail(ArchiveFileStream inputStream, FileExtension fileExtension, int width, IArchiveDataManager manager)
    {
        // Only load thumbnails for supported formats
        if (!IsFormatSupported)
            return new FileThumbnailData(null, Array.Empty<DuoGridItemViewModel>());
        else
            return base.LoadThumbnail(inputStream, fileExtension, width, manager);
    }

    /// <summary>
    /// Converts the file data to the specified format
    /// </summary>
    /// <param name="inputFormat">The format to convert from</param>
    /// <param name="outputFormat">The format to convert to</param>
    /// <param name="inputStream">The input file data stream</param>
    /// <param name="outputStream">The output stream for the converted data</param>
    /// <param name="manager">The manager</param>
    public override void ConvertTo(FileExtension inputFormat, FileExtension outputFormat, ArchiveFileStream inputStream, Stream outputStream,
        IArchiveDataManager manager)
    {
        // Check if it's the native format
        if (outputFormat == Format)
        {
            // Set the start position
            ReadTEXHeader(inputStream, manager);

            // Copy the image data
            inputStream.Stream.CopyToEx(outputStream);
        }
        else
        {
            // Convert the image normally
            base.ConvertTo(inputFormat, outputFormat, inputStream, outputStream, manager);
        }
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
    public override void ConvertFrom(FileExtension inputFormat, FileExtension outputFormat, ArchiveFileStream currentFileStream,
        ArchiveFileStream inputStream, ArchiveFileStream outputStream, IArchiveDataManager manager)
    {
        // Get the current TEX data
        TextureCooked? tex = ReadTEXHeader(currentFileStream, manager);

        // If there's no TEX header we handle the image data directly
        if (tex == null)
        {
            if (outputFormat == Format)
                inputStream.Stream.CopyToEx(outputStream.Stream);
            else
                ConvertFrom(inputFormat, MagickFormat, inputStream, outputStream);
        }
        else
        {
            // Get the image in specified format
            using MagickImage img = new(inputStream.Stream, GetMagickFormat(inputFormat.FileExtensions));

            tex.Height = (ushort)img.Height;
            tex.Width = (ushort)img.Width;

            byte[] bytes;
            if (MagickFormat == MagickFormat.Dds)
            {
                // Create mipmaps without alpha blend
                using MagickImageCollection ddsCollection = img.CreateDdsWithMipmaps(compress: true, filterType: FilterType.Box);
                
                // Get the image bytes
                bytes = ddsCollection.ToByteArray();
            }
            else
            {
                // Change the type to the output format
                img.Format = MagickFormat;

                // Get the image bytes
                bytes = img.ToByteArray();
            }

            // Update the TEX header
            // TODO: Figure out what the values are on Wii U where they don't match the actual size
            tex.RawDataSize = (uint)bytes.Length;
            tex.MemorySize = (uint)bytes.Length;
            tex.RawData = bytes;

            tex.Pre_SerializeRawData = true;

            // Write the TEX file
            manager.Context!.WriteStreamData(outputStream.Stream, tex, name: outputStream.Name, mode: VirtualFileMode.DoNotClose);
        }
    }

    protected override void WriteImage(MagickImage img, MagickFormat outputFormat, ArchiveFileStream outputStream)
    {
        if (outputFormat == MagickFormat.Dds)
        {
            using MagickImageCollection ddsCollection = img.CreateDdsWithMipmaps(compress: true, filterType: FilterType.Box);
            ddsCollection.Write(outputStream.Stream, outputFormat);
        }
        else
        {
            base.WriteImage(img, outputFormat, outputStream);
        }
    }

    /// <summary>
    /// Gets an image from the file data
    /// </summary>
    /// <param name="inputStream">The file data stream</param>
    /// <param name="format">The file format</param>
    /// <param name="manager">The manager to check</param>
    /// <returns>The image</returns>
    protected override MagickImage GetImage(ArchiveFileStream inputStream, FileExtension format, IArchiveDataManager manager)
    {
        // Set the Stream position
        ReadTEXHeader(inputStream, manager);

        // Return the image
        return new MagickImage(inputStream.Stream);
    }

    /// <summary>
    /// Gets the format to display the image as
    /// </summary>
    /// <param name="ext">The file extension</param>
    /// <returns>The file format display name</returns>
    public override string GetFormat(FileExtension ext) => Format.DisplayName;

    /// <summary>
    /// Reads the TEX header if there is one
    /// </summary>
    /// <param name="inputStream">The input stream</param>
    /// <param name="manager">The manager</param>
    /// <returns>The TEX header, if available</returns>
    protected TextureCooked? ReadTEXHeader(ArchiveFileStream inputStream, IArchiveDataManager manager)
    {
        // Use a reader
        using Reader reader = new(inputStream.Stream, manager.Context!.GetRequiredSettings<UbiArtSettings>().Endian == BinarySerializer.Endian.Little, true);

        // Check if it's in a TEX wrapper
        inputStream.Stream.Position = 4;
        bool usesTexWrapper = reader.ReadUInt32() == TEXHeader;

        // Reset the position
        inputStream.Stream.Position = 0;

        // If it uses a TEX wrapper we need to serialize the header
        if (usesTexWrapper)
        {
            // Serialize the header
            return manager.Context.ReadStreamData<TextureCooked>(inputStream.Stream, name: inputStream.Name, mode: VirtualFileMode.DoNotClose, onPreSerialize: x => x.Pre_SerializeRawData = false);
        }

        return null;
    }

    /// <summary>
    /// The header for a TEX wrapper
    /// </summary>
    protected const uint TEXHeader = 0x54455800;

    /// <summary>
    /// The format
    /// </summary>
    protected abstract FileExtension Format { get; }

    /// <summary>
    /// The magick format
    /// </summary>
    protected virtual MagickFormat MagickFormat => MagickFormat.Unknown;

    /// <summary>
    /// The magic header for the format
    /// </summary>
    protected virtual uint? FormatMagic => null;

    /// <summary>
    /// Indicates if the format is fully supported and can be read as an image
    /// </summary>
    protected abstract bool IsFormatSupported { get; }
}