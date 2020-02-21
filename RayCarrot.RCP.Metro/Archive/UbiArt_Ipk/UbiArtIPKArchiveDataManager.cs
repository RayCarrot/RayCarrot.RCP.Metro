using RayCarrot.CarrotFramework.Abstractions;
using RayCarrot.Extensions;
using RayCarrot.IO;
using RayCarrot.Rayman;
using RayCarrot.Rayman.UbiArt;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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
        /// <param name="archive">The archive data</param>
        /// <param name="archiveFileStream">The archive file stream</param>
        /// <returns>The archive data</returns>
        public ArchiveData LoadArchiveData(object archive, Stream archiveFileStream)
        {
            // Get the data
            var data = archive.CastTo<UbiArtIpkData>();

            RCFCore.Logger?.LogInformationSource("The directories are being retrieved for an IPK archive");

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
            var data = UbiArtIpkData.GetSerializer(Settings).Deserialize(archiveFileStream);

            RCFCore.Logger?.LogInformationSource($"Read IPK file ({data.Version}) with {data.FilesCount} files");

            return data;
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

            // TODO-UPDATE: Option for setting block properties
            // Set default properties based on settings
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
                    data.EngineVersion = 0;

                    break;

                case UbiArtGame.RaymanLegends:

                    if (Settings.Platform != UbiArtPlatform.PlayStation4)
                    {
                        data.Version = 5;
                        data.Unknown1 = 0;
                    }
                    else
                    {
                        data.Version = 7;
                        data.Unknown1 = 8;
                    }

                    data.Unknown3 = false;
                    data.Unknown4 = true;
                    data.Unknown5 = true;
                    data.Unknown6 = 0;
                    data.EngineVersion = 30765;

                    break;

                case UbiArtGame.RaymanAdventures:

                    data.Version = 8;
                    data.Unknown1 = 2;
                    data.Unknown2 = 11;
                    data.Unknown3 = true;
                    data.Unknown4 = true;
                    data.Unknown5 = true;
                    data.Unknown6 = 0;
                    data.EngineVersion = 30765;

                    break;

                case UbiArtGame.RaymanMini:

                    data.Version = 8;
                    data.Unknown1 = 12;
                    data.Unknown2 = 12;
                    data.Unknown3 = true;
                    data.Unknown4 = true;
                    data.Unknown5 = true;
                    data.Unknown6 = 3826;
                    data.EngineVersion = 3826;

                    break;

                case UbiArtGame.JustDance2017:

                    data.Version = 5;
                    data.Unknown1 = 8;
                    data.Unknown3 = false;
                    data.Unknown4 = false;
                    data.Unknown5 = false;
                    data.Unknown6 = 0;
                    data.EngineVersion = 241478;

                    break;

                case UbiArtGame.ValiantHearts:

                    data.Version = 7;
                    data.Unknown1 = 10;
                    data.Unknown3 = false;
                    data.Unknown4 = true;
                    data.Unknown5 = true;
                    data.Unknown6 = 0;
                    data.EngineVersion = 0;
                    data.Unknown9 = 0;

                    break;

                case UbiArtGame.ChildOfLight:

                    data.Version = 7;
                    data.Unknown1 = 0;
                    data.Unknown3 = false;
                    data.Unknown4 = true;
                    data.Unknown5 = true;
                    data.Unknown6 = 0;
                    data.EngineVersion = 30765;

                    break;

                case UbiArtGame.GravityFalls:

                    data.Version = 7;
                    data.Unknown1 = 10;
                    data.Unknown3 = false;
                    data.Unknown4 = true;
                    data.Unknown5 = true;
                    data.Unknown6 = 0;
                    data.EngineVersion = 0;

                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(Settings.Game), Settings.Game, null);
            }

            // Unknown value used for all versions which we set to 0
            data.Unknown7 = 0;

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
            // IDEA: Allow this to be configured
            var compressExtensions = new FileExtension[]
            {
                new FileExtension(".dtape.ckd"), 
                new FileExtension(".fx.fxb"), 
                new FileExtension(".m3d.ckd"), 
                new FileExtension(".png.ckd"), 
                new FileExtension(".tga.ckd"), 
            };

            bool compress = compressExtensions.Any(x => x == relativePath.FileExtension);

            return new UbiArtIPKFileEntry()
            {
                Path = new UbiArtPath(relativePath),

                // IDEA: Perhaps find a cleaner way of handling this...
                CompressedSize = compress ? 1u : 0u
            };
        }

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

            // Make sure the file is compressed
            if (!file.IsCompressed)
                return fileData;

            // Compress the bytes
            var compressedBytes = UbiArtIpkData.GetEncoder(Settings.IPKVersion, file.Size).Encode(fileData);

            RCFCore.Logger?.LogTraceSource($"The file {file.Path.FileName} has been compressed");

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
            RCFCore.Logger?.LogInformationSource($"An IPK archive is being repacked...");

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
            data.WriteArchiveContent(outputFileStream, fileGenerator);

            outputFileStream.Position = 0;

            // Serialize the data
            UbiArtIpkData.GetSerializer(Settings).Serialize(outputFileStream, data);

            RCFCore.Logger?.LogInformationSource($"The IPK archive has been repacked");
        }

        #endregion
    }
}