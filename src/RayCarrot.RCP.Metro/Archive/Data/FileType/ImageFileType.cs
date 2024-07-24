﻿using System.IO;
using System.Windows.Media.Imaging;
using MahApps.Metro.IconPacks;
using RayCarrot.RCP.Metro.Imaging;

namespace RayCarrot.RCP.Metro.Archive;

/// <summary>
/// A common image file type
/// </summary>
public sealed class ImageFileType : FileType
{
    #region Constructor

    public ImageFileType()
    {
        SupportedFormats = new ImageFormat[]
        {
            new PngImageFormat(),
            new JpgImageFormat(),
            new BmpImageFormat(),
            new TgaImageFormat(),
            new DdsImageFormat(),
            new PcxImageFormat(),
        };

        ImportFormats = SupportedFormats.Where(x => x.CanEncode).SelectMany(x => x.FileExtensions).ToArray();
        ExportFormats = SupportedFormats.Where(x => x.CanDecode).SelectMany(x => x.FileExtensions).ToArray();
    }

    #endregion

    #region Private Properties

    private ImageFormat[] SupportedFormats { get; }

    #endregion

    #region Public Properties

    public override string TypeDisplayName => Resources.Archive_Format_Img;
    public override PackIconMaterialKind Icon => PackIconMaterialKind.ImageOutline;

    public override FileExtension[] ImportFormats { get; }
    public override FileExtension[] ExportFormats { get; }

    #endregion

    #region Private Methods

    private ImageFormat GetImageFormat(FileExtension fileExtension) =>
        SupportedFormats.First(x => x.FileExtensions.Contains(fileExtension));

    #endregion

    #region Public Methods

    public override bool IsOfType(FileExtension fileExtension, IArchiveDataManager manager) => 
        SupportedFormats.Any(x => x.FileExtensions.Contains(fileExtension));

    public override FileThumbnailData LoadThumbnail(
        ArchiveFileStream inputStream, 
        FileExtension fileExtension, 
        IArchiveDataManager manager)
    {
        ImageFormat imageFormat = GetImageFormat(fileExtension);
        RawImageData imgData = imageFormat.Decode(inputStream.Stream);
        BitmapSource thumb = imgData.ToBitmapSource();

        return new FileThumbnailData(thumb, imgData.Metadata.GetInfoItems(imageFormat).ToArray());
    }

    public override void ConvertTo(
        FileExtension inputFormat,
        FileExtension outputFormat, 
        ArchiveFileStream inputStream, 
        Stream outputStream, 
        IArchiveDataManager manager)
    {
        ImageFormat inputImageFormat = GetImageFormat(inputFormat);
        ImageFormat outputImageFormat = GetImageFormat(outputFormat);

        // Don't convert if it's the same format
        if (inputImageFormat == outputImageFormat)
        {
            // Copy the image data
            inputStream.Stream.CopyToEx(outputStream);
        }
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
        ImageFormat inputImageFormat = GetImageFormat(inputFormat);
        ImageFormat outputImageFormat = GetImageFormat(outputFormat);

        // Don't convert if it's the same format
        if (inputImageFormat == outputImageFormat)
        {
            // Copy the image data
            inputStream.Stream.CopyToEx(outputStream.Stream);
        }
        else
        {
            // Convert the image data
            inputImageFormat.Convert(inputStream.Stream, outputStream.Stream, outputImageFormat);
        }
    }

    #endregion
}