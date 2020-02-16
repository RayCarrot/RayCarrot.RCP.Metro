using RayCarrot.Rayman;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using RayCarrot.CarrotFramework.Abstractions;
using RayCarrot.IO;
using RayCarrot.Extensions;
using Ionic.Zlib;
using RayCarrot.Rayman.UbiArt;
using SevenZip.Compression.LZMA;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// Archive data manager for a UbiArt .ipk file
    /// </summary>
    public class UbiArtIPKArchiveDataManager : IArchiveDataManager
    {
        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="settings">The settings when serializing the data</param>
        public UbiArtIPKArchiveDataManager(UbiArtSettings settings)
        {
            Settings = settings;
        }

        #endregion

        #region Protected Properties

        /// <summary>
        /// The settings when serializing the data
        /// </summary>
        protected UbiArtSettings Settings { get; }

        #endregion

        #region Public Properties

        /// <summary>
        /// The path separator character to use. This is usually \ or /.
        /// </summary>
        public char PathSeparatorCharacter => '/';

        #endregion

        #region Public Methods

        /// <summary>
        /// Gets the available directories from the archive along with their contents
        /// </summary>
        /// <param name="archiveFileStream">The file stream for the archive</param>
        /// <returns>The directories</returns>
        public IEnumerable<ArchiveDirectory> GetDirectories(Stream archiveFileStream)
        {
            RCFCore.Logger?.LogInformationSource("The directories are being retrieved for an IPK archive");

            // Set the stream position to 0
            archiveFileStream.Position = 0;

            // Read the file data
            UbiArtIpkData data = UbiArtIpkData.GetSerializer(Settings).Deserialize(archiveFileStream);

            RCFCore.Logger?.LogInformationSource($"Read IPK file ({data.Version}) with {data.FilesCount} files");

            // Helper method for getting the archive file data
            IArchiveFileData GetFileData(UbiArtIPKFileEntry file)
            {
                if (file.Path.GetFileExtensions().Any(x => x == ".png" || x == ".tga"))
                    return new UbiArtIPKArchiveImageFileData(file, Settings, data.BaseOffset);
                else
                    return new UbiArtIPKArchiveFileData(file, Settings, data.BaseOffset);
            }

            // Add the directories to the collection
            foreach (var file in data.Files.GroupBy(x => x.Path.DirectoryPath))
            {
                // Return each directory with the available files, including the root directory
                yield return new ArchiveDirectory(file.Key, file.Select(GetFileData).ToArray());
            }
        }

        /// <summary>
        /// Updates the archive with the modified files
        /// </summary>
        /// <param name="archiveFileStream">The file stream for the archive</param>
        /// <param name="outputFileStream">The file stream for the updated archive</param>
        /// <param name="files">The files of the archive. Modified files have the <see cref="IArchiveFileData.PendingImportTempPath"/> property set to an existing path.</param>
        public void UpdateArchive(Stream archiveFileStream, Stream outputFileStream, IEnumerable<IArchiveFileData> files)
        {
            RCFCore.Logger?.LogInformationSource($"An IPK archive is being repacked...");

            // Set the stream position to 0
            archiveFileStream.Position = 0;

            // Load the current file
            var data = UbiArtIpkData.GetSerializer(Settings).Deserialize(archiveFileStream);

            // Order files by path
            var allFiles = files.Cast<UbiArtIPKArchiveFileData>().ToDictionary(x => x.FileEntry.Path.FullPath);

            // Create a temporary directory to load the modified files into
            using var tempDir = new TempDirectory(true);

            // Create the file generator
            var fileGenerator = new ArchiveFileGenerator();

            // The current pointer position
            ulong currentOffset = 0;

            // Handle each file
            foreach (var file in data.Files)
            {
                // NOTE: The order which the files get placed is different from how the IPK was originally packed. This is easily fixed looping the files ordered by their offset value. This is however not needed as the game reads them fine still.

                // Get the file
                var existingFile = allFiles.TryGetValue(file.Path.FullPath);

                // Check if the file is one of the modified files
                if (existingFile.PendingImportTempPath.FileExists)
                {
                    RCFCore.Logger?.LogTraceSource($"{existingFile.FileName} as been modified");

                    // Get the temporary file path without disposing it as it gets removed from the directory
                    var tempFilePath = (tempDir.TempPath + file.Path.FileName).GetNonExistingFileName();

                    // Remove the file from the dictionary
                    allFiles.Remove(file.Path.FullPath);

                    // Move the file
                    RCFRCP.File.MoveFile(existingFile.PendingImportTempPath, tempFilePath, true);

                    // Compress the file if it was previously compressed
                    if (file.IsCompressed)
                    {
                        // Get the file bytes
                        var bytes = File.ReadAllBytes(tempFilePath);

                        byte[] compressedBytes;

                        // Use LZMA
                        if (Settings.IPKVersion >= 8)
                        {
                            // Compress the bytes
                            compressedBytes = SevenZipHelper.Compress(bytes);
                        }
                        // Use ZLib
                        else
                        {
                            // Compress the bytes
                            compressedBytes = ZlibStream.CompressBuffer(bytes);
                        }

                        RCFCore.Logger?.LogTraceSource($"The file {existingFile.FileName} has been compressed");

                        // Delete the file
                        RCFRCP.File.DeleteFile(tempFilePath);

                        // Write the compressed bytes to the file
                        File.WriteAllBytes(tempFilePath, compressedBytes);

                        // Set the file size
                        file.Size = (uint)bytes.Length;

                        // Set the compressed file size
                        file.CompressedSize = (uint)compressedBytes.Length;
                    }
                    else
                    {
                        // Set the file size
                        file.Size = (uint)tempFilePath.GetSize().Bytes;
                    }

                    // Add to the generator
                    fileGenerator.Add(file.Path.FullPath, () => File.ReadAllBytes(tempFilePath));
                }
                // Use the original file without decompressing it
                else
                {
                    // Add to the generator
                    fileGenerator.Add(file.Path.FullPath, () => existingFile.FileEntry.GetFileBytes(archiveFileStream, data.BaseOffset, Settings, false));
                }

                // Reset the offset array to always contain 1 item
                file.Offsets = new ulong[]
                {
                    // Set the offset
                    currentOffset
                };

                // Set the count
                file.OffsetCount = 1;

                // Increase by the file size
                currentOffset += file.ArchiveSize;
            }

            // Set the base offset
            data.BaseOffset = data.GetHeaderSize(Settings);

            // Serialize the data
            UbiArtIpkData.GetSerializer(Settings).Serialize(outputFileStream, data);

            // Write the files
            data.WriteArchiveContent(outputFileStream, fileGenerator);

            RCFCore.Logger?.LogInformationSource($"The IPK archive has been repacked");
        }

        #endregion
    }
}