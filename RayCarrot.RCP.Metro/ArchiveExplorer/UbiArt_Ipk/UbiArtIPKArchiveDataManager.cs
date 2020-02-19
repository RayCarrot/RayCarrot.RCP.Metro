using RayCarrot.CarrotFramework.Abstractions;
using RayCarrot.Extensions;
using RayCarrot.Rayman;
using RayCarrot.Rayman.UbiArt;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using RayCarrot.IO;

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
        /// Loads the archive data
        /// </summary>
        /// <param name="archiveFileStream">The file stream for the archive</param>
        /// <returns>The archive data</returns>
        public ArchiveData LoadArchiveData(Stream archiveFileStream)
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

            // Helper method for getting the directories
            IEnumerable<ArchiveDirectory> GetDirectories()
            {
                // Add the directories to the collection
                foreach (var file in data.Files.GroupBy(x => x.Path.DirectoryPath))
                {
                    // Return each directory with the available files, including the root directory
                    yield return new ArchiveDirectory(file.Key, file.Select(GetFileData).ToArray());
                }
            }

            // Return the data
            return new ArchiveData(GetDirectories(), data.GetArchiveContent(archiveFileStream));
        }

        /// <summary>
        /// Loads the archive
        /// </summary>
        /// <param name="archiveFileStream">The file stream for the archive</param>
        /// <returns>The archive data</returns>
        public object LoadArchive(Stream archiveFileStream)
        {
            // Set the stream position to 0
            archiveFileStream.Position = 0;

            // Load the current file
            return UbiArtIpkData.GetSerializer(Settings).Deserialize(archiveFileStream);
        }

        /// <summary>
        /// Gets a new archive from a collection of modified files
        /// </summary>
        /// <param name="files"></param>
        /// <returns>The archive</returns>
        public object GetArchive(IEnumerable<IArchiveImportData> files)
        {
            // Create the data
            var data = new UbiArtIpkData();

            // TODO: Set default properties based on settings
            switch (Settings.Game)
            {
                case UbiArtGame.RaymanOrigins:

                    if (Settings.Platform != UbiArtPlatform.Nintendo3DS)
                    {
                        data.Version = 3;
                        data.Unknown1 = 0;
                    }
                    else
                    {
                        data.Version = 4;
                        data.Unknown1 = 5;
                    }

                    data.Unknown3 = false;
                    data.Unknown4 = true;
                    data.Unknown5 = true;
                    data.Unknown6 = 0;
                    data.Unknown7 = 0;
                    data.Unknown8 = 0;

                    break;

                case UbiArtGame.RaymanLegends:
                    break;

                case UbiArtGame.RaymanJungleRun:
                    break;

                case UbiArtGame.RaymanFiestaRun:
                    break;

                case UbiArtGame.RaymanAdventures:
                    break;

                case UbiArtGame.RaymanMini:
                    break;

                case UbiArtGame.JustDance2017:
                    break;

                case UbiArtGame.ValiantHearts:
                    break;

                case UbiArtGame.ChildOfLight:
                    break;

                case UbiArtGame.GravityFalls:
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }

            // Set the files
            data.Files = files.Select(x => x.FileEntryData.CastTo<UbiArtIPKFileEntry>()).ToArray();
            data.FilesCount = (uint)data.Files.Length;

            return data;
        }

        /// <summary>
        /// Gets a new file entry from the specified path
        /// </summary>
        /// <param name="relativePath">The relative path of the file</param>
        /// <returns>The file entry</returns>
        public object GetFileEntry(FileSystemPath relativePath)
        {
            return new UbiArtIPKFileEntry()
            {
                Path = new UbiArtPath(relativePath)
                // TODO: Possible set the file to use compression for certain types?
            };
        }

        /// <summary>
        /// Updates the archive with the modified files
        /// </summary>
        /// <param name="archive">The loaded archive data</param>
        /// <param name="outputFileStream">The file stream for the updated archive</param>
        /// <param name="files">The import data for the archive files</param>
        /// <param name="generator">The file generator</param>
        public void UpdateArchive(object archive, Stream outputFileStream, IEnumerable<IArchiveImportData> files, IDisposable generator)
        {
            RCFCore.Logger?.LogInformationSource($"An IPK archive is being repacked...");

            // Get the archive data
            var data = archive.CastTo<UbiArtIpkData>();

            // Order files by path
            var allFiles = files.ToDictionary(x => x.FileEntryData.CastTo<UbiArtIPKFileEntry>().Path.FullPath);

            // Create the file generator
            using var fileGenerator = new ArchiveFileGenerator<UbiArtIPKFileEntry>();

            // The current pointer position
            ulong currentOffset = 0;

            // Handle each file
            foreach (var file in data.Files)
            {
                // Get the file
                var importData = allFiles.TryGetValue(file.Path.FullPath);

                // Get the entry
                var fileEntry = importData.FileEntryData.CastTo<UbiArtIPKFileEntry>();

                // Check if the file is one of the modified files
                if (importData.IsModified)
                {
                    RCFCore.Logger?.LogTraceSource($"{fileEntry.Path.FileName} as been modified");

                    // Remove the file from the dictionary
                    allFiles.Remove(file.Path.FullPath);

                    // Get the stream
                    using var stream = importData.GetDataStream;

                    // Compress the file if it was previously compressed
                    if (file.IsCompressed)
                    {
                        // Get the file bytes
                        var bytes = stream.ReadRemainingBytes();

                        // TODO: Allow two streams to be passed in and copy from one to the other?
                        // Compress the bytes
                        byte[] compressedBytes = UbiArtIpkData.CompressData(bytes, Settings.IPKVersion);

                        RCFCore.Logger?.LogTraceSource($"The file {fileEntry.Path.FileName} has been compressed");

                        // Write the compressed bytes to the stream
                        stream.Write(compressedBytes);

                        // Set the stream length
                        stream.SetLength(compressedBytes.Length);

                        // Set the file size
                        file.Size = (uint)bytes.Length;

                        // Set the compressed file size
                        file.CompressedSize = (uint)compressedBytes.Length;
                    }
                    else
                    {
                        // Set the file size
                        file.Size = (uint)stream.Length;
                    }

                    // TODO: Allow to pass in stream to generator?
                    // Add to the generator
                    fileGenerator.Add(file, () => importData.GetDataStream.RunAndDispose(s => s.ReadRemainingBytes()));
                }
                // Use the original file without decompressing it
                else
                {
                    // Add to the generator
                    fileGenerator.Add(file, () => generator.CastTo<IArchiveFileGenerator<UbiArtIPKFileEntry>>().GetBytes(fileEntry));
                }

                // Reset the offset array to always contain 1 item
                file.Offsets = new ulong[]
                {
                    // Set the offset
                    currentOffset
                };

                // Set the count
                file.OffsetCount = (uint)file.Offsets.Length;

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