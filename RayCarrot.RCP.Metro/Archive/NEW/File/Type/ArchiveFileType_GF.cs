using MahApps.Metro.IconPacks;
using RayCarrot.Binary;
using RayCarrot.IO;
using RayCarrot.Rayman;
using RayCarrot.Rayman.OpenSpace;
using RayCarrot.WPF;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using PixelFormat = System.Drawing.Imaging.PixelFormat;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// An image file type
    /// </summary>
    public class ArchiveFileType_GF : IArchiveFileType
    {
        #region Interface Implementations

        /// <summary>
        /// The display name for the file type
        /// </summary>
        public string TypeDisplayName => "GF";

        /// <summary>
        /// The default icon kind for the type
        /// </summary>
        public PackIconMaterialKind Icon => PackIconMaterialKind.ImageOutline;

        /// <summary>
        /// Indicates if the specified manager supports files of this type
        /// </summary>
        /// <param name="manager">The manager to check</param>
        /// <returns>True if supported, otherwise false</returns>
        public bool IsSupported(IArchiveDataManager manager) => manager.SerializerSettings is OpenSpaceSettings;

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
        /// The native file format
        /// </summary>
        public FileExtension NativeFormat => new FileExtension(".gf");

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
        /// Initializes the file
        /// </summary>
        /// <param name="inputStream">The file data stream</param>
        /// <param name="width">The thumbnail width</param>
        /// <param name="manager">The manager</param>
        /// <returns>The init data</returns>
        public ArchiveFileInitData InitFile(ArchiveFileStream inputStream, int width, IArchiveDataManager manager)
        {
            // Load the file
            var file = GetFileContent(inputStream.Stream, manager);

            // Get the thumbnail with the specified size
            return new ArchiveFileInitData(file.GetRawBitmapData(width, (int)(file.Height / ((double)file.Width / width))).GetBitmap().ToImageSource(), new DuoGridItemViewModel[]
            {
                // TODO-UPDATE: Localize
                new DuoGridItemViewModel("Size:", $"{file.Width}x{file.Height}"), 
                new DuoGridItemViewModel("Transparent:", $"{file.GFPixelFormat.SupportsTransparency()}"), 
                new DuoGridItemViewModel("Mipmaps:", $"{file.RealMipmapCount}"),
                new DuoGridItemViewModel("Format:", $"{file.Format}", UserLevel.Technical), 
            });
        }

        /// <summary>
        /// Converts the file data to the specified format
        /// </summary>
        /// <param name="format">The format to convert to</param>
        /// <param name="inputStream">The input file data stream</param>
        /// <param name="outputStream">The output stream for the converted data</param>
        /// <param name="manager">The manager</param>
        public void ConvertTo(FileExtension format, Stream inputStream, Stream outputStream, IArchiveDataManager manager)
        {
            // Get the image format
            var imgFormat = format.PrimaryFileExtension switch
            {
                ".png" => ImageFormat.Png,
                ".jpeg" => ImageFormat.Jpeg,
                ".jpg" => ImageFormat.Jpeg,
                ".bmp" => ImageFormat.Bmp,
                _ => throw new Exception($"The specified file format {format.PrimaryFileExtension} is not supported")
            };

            // Get the bitmap
            using var bmp = GetFileContent(inputStream, manager).GetRawBitmapData().GetBitmap();

            // Save the bitmap to the output stream
            bmp.Save(outputStream, imgFormat);
        }

        /// <summary>
        /// Converts the file data from the specified format
        /// </summary>
        /// <param name="format">The format to convert from</param>
        /// <param name="currentFileStream">The current file stream</param>
        /// <param name="inputStream">The input file data stream to convert from</param>
        /// <param name="outputStream">The output stream for the converted data</param>
        /// <param name="manager">The manager</param>
        public void ConvertFrom(FileExtension format, ArchiveFileStream currentFileStream, Stream inputStream, Stream outputStream, IArchiveDataManager manager)
        {
            // Load the bitmap
            using var bmp = new Bitmap(inputStream);

            // Load the current file
            OpenSpaceGFFile gf = GetFileContent(currentFileStream.Stream, manager);

            // IDEA: If bmp is not in supported format, then convert it?

            RawBitmapData rawBitmapData;

            // Get the bitmap lock
            using (var bmpLock = new BitmapLock(bmp))
            {
                // Get the raw bitmap data
                rawBitmapData = new RawBitmapData(bmp.Width, bmp.Height, bmpLock.Pixels, bmp.PixelFormat);

                // Force the new pixel format to be 888 or 8888 if set to do so
                if (RCPServices.Data.Archive_GF_ForceGF8888Import)
                    gf.GFPixelFormat = gf.GFPixelFormat.SupportsTransparency() ? OpenSpaceGFFormat.Format_32bpp_BGRA_8888 : OpenSpaceGFFormat.Format_24bpp_BGR_888;

                // Check if the format should be updated for transparency
                if (RCPServices.Data.Archive_GF_UpdateTransparency != Archive_GF_TransparencyMode.PreserveFormat)
                {
                    // NOTE: Only 24 and 32 bpp bitmaps are supported
                    // Check if the imported file is transparent
                    var isTransparent = bmp.PixelFormat switch
                    {
                        PixelFormat.Format32bppArgb => (RCPServices.Data.Archive_GF_UpdateTransparency == Archive_GF_TransparencyMode.UpdateBasedOnPixelFormat ||
                                                        bmpLock.UtilizesAlpha()),
                        PixelFormat.Format24bppRgb => false,
                        _ => (bool?)null
                    };

                    // NOTE: Currently only supported for formats with 3 or 4 channels
                    // Check if the format should be updated for transparency
                    if (gf.Channels >= 3 && isTransparent != null)
                        // Update the format
                        gf.GFPixelFormat = isTransparent.Value ? OpenSpaceGFFormat.Format_32bpp_BGRA_8888 : OpenSpaceGFFormat.Format_24bpp_BGR_888;
                }
            }

            // Import the bitmap
            gf.ImportFromBitmap((OpenSpaceSettings)manager.SerializerSettings, rawBitmapData, RCPServices.Data.Archive_GF_GenerateMipmaps);

            // Serialize the data to get the bytes
            BinarySerializableHelpers.WriteToStream(gf, outputStream, manager.SerializerSettings, RCPServices.App.GetBinarySerializerLogger());
        }

        #endregion

        #region GF Methods

        /// <summary>
        /// Gets the contents of the file
        /// </summary>
        /// <param name="fileStream">The file stream</param>
        /// <param name="manager">The data manager</param>
        /// <returns>The deserialized file</returns>
        public OpenSpaceGFFile GetFileContent(Stream fileStream, IArchiveDataManager manager)
        {
            // Serialize the data
            return BinarySerializableHelpers.ReadFromStream<OpenSpaceGFFile>(fileStream, manager.SerializerSettings, RCPServices.App.GetBinarySerializerLogger());
        }

        #endregion
    }
}