using RayCarrot.Common;
using RayCarrot.IO;
using RayCarrot.Rayman;
using RayCarrot.Rayman.UbiArt;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using RayCarrot.Binary;
using RayCarrot.Logging;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// Base archive data manager for a UbiArt .ipk file
    /// </summary>
    public abstract class BaseUbiArtIPKArchiveDataManager : IArchiveBaseDataManager
    {
        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="configViewModel">The configuration view model</param>
        protected BaseUbiArtIPKArchiveDataManager(UbiArtIPKArchiveConfigViewModel configViewModel)
        {
            Config = configViewModel;
        }

        #endregion

        #region Protected Properties

        /// <summary>
        /// The settings when serializing the data
        /// </summary>
        protected UbiArtSettings Settings => Config.Settings;

        /// <summary>
        /// The configuration view model
        /// </summary>
        protected UbiArtIPKArchiveConfigViewModel Config { get; }

        #endregion

        #region Public Properties

        /// <summary>
        /// The path separator character to use. This is usually \ or /.
        /// </summary>
        public char PathSeparatorCharacter => '/';

        /// <summary>
        /// The file filter to use for the archive files
        /// </summary>
        public string ArchiveFileFilter => new FileFilterItem("*.ipk", "IPK").ToString();

        #endregion

        #region Public Methods

        /// <summary>
        /// Encodes the file bytes
        /// </summary>
        /// <param name="fileData">The bytes to encode</param>
        /// <param name="fileEntry">The file entry for the file to encode</param>
        /// <returns>The encoded bytes</returns>
        public byte[] EncodeFile(byte[] fileData, object fileEntry)
        {
            // Get the file entry
            var file = fileEntry.CastTo<UbiArtIPKFileEntry>();

            // Set the file size
            file.Size = (uint)fileData.Length;

            // Return the data as is if the file should not be compressed
            if (!Config.ShouldCompress(file))
                return fileData;

            // Compress the bytes
            var compressedBytes = UbiArtIpkData.GetEncoder(file.IPKVersion, file.Size).Encode(fileData);

            RL.Logger?.LogTraceSource($"The file {file.Path.FileName} has been compressed");

            // Set the compressed file size
            file.CompressedSize = (uint)compressedBytes.Length;

            // Return the compressed bytes
            return compressedBytes;
        }

        /// <summary>
        /// Updates the archive with the modified files
        /// </summary>
        /// <param name="archive">The loaded archive data</param>
        /// <param name="outputFileStream">The file stream for the updated archive</param>
        /// <param name="files">The import data for the archive files</param>
        public void UpdateArchive(object archive, Stream outputFileStream, IEnumerable<IArchiveImportData> files)
        {
            RL.Logger?.LogInformationSource($"An IPK archive is being repacked...");

            // Get the archive data
            var data = archive.CastTo<UbiArtIpkData>();

            // Create the file generator
            using var fileGenerator = new ArchiveFileGenerator<UbiArtIPKFileEntry>();

            // Keep track of the current pointer position
            ulong currentOffset = 0;

            // Handle each file
            foreach (var importData in files)
            {
                // Get the file
                var file = importData.FileEntryData.CastTo<UbiArtIPKFileEntry>();

                // Reset the offset array to always contain 1 item
                file.Offsets = new ulong[]
                {
                    file.Offsets?.FirstOrDefault() ?? 0
                };

                // Set the count
                file.OffsetCount = (uint)file.Offsets.Length;

                // Add to the generator
                fileGenerator.Add(file, () =>
                {
                    // Get the file bytes to write to the archive
                    var bytes = importData.GetData(file);

                    // Set the offset
                    file.Offsets[0] = currentOffset;

                    // Increase by the file size
                    currentOffset += file.ArchiveSize;

                    return bytes;
                });
            }

            // Set the base offset
            data.BaseOffset = data.GetHeaderSize(Settings);

            // Write the files
            data.WriteArchiveContent(outputFileStream, fileGenerator, Config.ShouldCompress(data));

            outputFileStream.Position = 0;

            // Serialize the data
            BinarySerializableHelpers.WriteToStream(data, outputFileStream, Settings, RCPServices.App.GetBinarySerializerLogger());

            RL.Logger?.LogInformationSource($"The IPK archive has been repacked");
        }

        #endregion
    }
}