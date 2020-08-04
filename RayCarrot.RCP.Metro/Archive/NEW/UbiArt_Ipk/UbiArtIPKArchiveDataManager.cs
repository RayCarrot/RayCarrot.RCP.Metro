using RayCarrot.Binary;
using RayCarrot.Common;
using RayCarrot.IO;
using RayCarrot.Logging;
using RayCarrot.Rayman;
using RayCarrot.Rayman.UbiArt;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ByteSizeLib;
using RayCarrot.WPF;

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
        /// <param name="configViewModel">The configuration view model</param>
        public UbiArtIPKArchiveDataManager(UbiArtIPKArchiveConfigViewModel configViewModel)
        {
            Config = configViewModel;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// The path separator character to use. This is usually \ or /.
        /// </summary>
        public char PathSeparatorCharacter => '/';

        /// <summary>
        /// The file extension for the archive file
        /// </summary>
        public FileExtension ArchiveFileExtension => new FileExtension(".ipk");

        /// <summary>
        /// The serializer settings to use for the archive
        /// </summary>
        public BinarySerializerSettings SerializerSettings => Settings;

        /// <summary>
        /// The default archive file name to use when creating an archive
        /// </summary>
        public string DefaultArchiveFileName => "patch_PC.ipk";

        /// <summary>
        /// Gets the configuration UI to use for creator
        /// </summary>
        public object GetCreatorUIConfig => new UbiArtIPKArchiveConfigUI()
        {
            DataContext = Config
        };

        /// <summary>
        /// The settings when serializing the data
        /// </summary>
        protected UbiArtSettings Settings => Config.Settings;

        /// <summary>
        /// The configuration view model
        /// </summary>
        protected UbiArtIPKArchiveConfigViewModel Config { get; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Encodes the file data from the input stream, or nothing if the data does not need to be encoded
        /// </summary>
        /// <param name="inputStream">The input data stream to encode</param>
        /// <param name="outputStream">The output data stream for the encoded data</param>
        /// <param name="fileEntry">The file entry for the file to encode</param>
        public void EncodeFile(Stream inputStream, Stream outputStream, object fileEntry)
        {
            // Get the file entry
            var entry = fileEntry.CastTo<UbiArtIPKFileEntry>();

            // Set the file size
            entry.Size = (uint)inputStream.Length;

            // Return the data as is if the file should not be compressed
            if (!Config.ShouldCompress(entry))
                return;

            // Compress the bytes
            UbiArtIpkData.GetEncoder(entry.IPKVersion, entry.Size).Encode(inputStream, outputStream);

            RL.Logger?.LogTraceSource($"The file {entry.Path.FileName} has been compressed");

            // Set the compressed file size
            entry.CompressedSize = (uint)outputStream.Length;
        }

        /// <summary>
        /// Decodes the file data from the input stream, or nothing if the data is not encoded
        /// </summary>
        /// <param name="inputStream">The input data stream to decode</param>
        /// <param name="outputStream">The output data stream for the decoded data</param>
        /// <param name="fileEntry">The file entry for the file to decode</param>
        public void DecodeFile(Stream inputStream, Stream outputStream, object fileEntry)
        {
            var entry = (UbiArtIPKFileEntry)fileEntry;

            // Decompress the data if compressed
            if (entry.IsCompressed)
                UbiArtIpkData.GetEncoder(entry.IPKVersion, entry.Size).Decode(inputStream, outputStream);
        }

        /// <summary>
        /// Gets the file data from the archive using a generator
        /// </summary>
        /// <param name="generator">The generator</param>
        /// <param name="fileEntry">The file entry</param>
        /// <returns>The encoded file data</returns>
        public byte[] GetFileData(IDisposable generator, object fileEntry) => generator.CastTo<IArchiveFileGenerator<UbiArtIPKFileEntry>>().GetBytes((UbiArtIPKFileEntry)fileEntry);

        /// <summary>
        /// Writes the files to the archive
        /// </summary>
        /// <param name="generator">The generator</param>
        /// <param name="archive">The loaded archive data</param>
        /// <param name="outputFileStream">The file output stream for the archive</param>
        /// <param name="files">The files to include</param>
        public void WriteArchive(IDisposable generator, object archive, Stream outputFileStream, IList<ArchiveFileItem> files)
        {
            RL.Logger?.LogInformationSource($"An IPK archive is being repacked...");

            // Get the archive data
            var data = archive.CastTo<UbiArtIpkData>();

            // Create the file generator
            using var fileGenerator = new ArchiveFileGenerator<UbiArtIPKFileEntry>();

            // Get files and entries
            var archiveFiles = files.Select(x => new
            {
                Entry = (x.ArchiveEntry as UbiArtIPKFileEntry) ?? new UbiArtIPKFileEntry
                {
                    Path = new UbiArtPath(this.CombinePaths(x.Directory, x.FileName)),
                    IPKVersion = data.Version
                },
                FileItme = x
            }).ToArray();

            // Set the files
            data.Files = archiveFiles.Select(x => x.Entry).ToArray();
            data.FilesCount = (uint)data.Files.Length;

            // Keep track of the current pointer position
            ulong currentOffset = 0;

            // Handle each file
            foreach (var file in archiveFiles)
            {
                // Get the file
                var entry = file.Entry;

                // Reset the offset array to always contain 1 item
                entry.Offsets = new ulong[]
                {
                    entry.Offsets?.FirstOrDefault() ?? 0
                };

                // Set the count
                entry.OffsetCount = (uint)entry.Offsets.Length;

                // Add to the generator
                fileGenerator.Add(entry, () =>
                {
                    // Get the file bytes to write to the archive
                    var fileStream = file.FileItme.GetFileData(generator);

                    // Set the offset
                    entry.Offsets[0] = currentOffset;

                    // Increase by the file size
                    currentOffset += entry.ArchiveSize;

                    return fileStream.Stream.ReadRemainingBytes();
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

            RL.Logger?.LogInformationSource("The directories are being retrieved for an IPK archive");

            // Helper method for getting the directories
            IEnumerable<ArchiveDirectory> GetDirectories()
            {
                // Add the directories to the collection
                foreach (var file in data.Files.GroupBy(x => x.Path.DirectoryPath))
                {
                    // Return each directory with the available files, including the root directory
                    yield return new ArchiveDirectory(file.Key, file.Select(x => new ArchiveFileItem(this, x.Path.FileName, x.Path.DirectoryPath, x)).ToArray());
                }
            }

            // Return the data
            return new ArchiveData(GetDirectories(), data.GetArchiveContent(archiveFileStream));
        }

        /// <summary>
        /// Loads the archive from a stream
        /// </summary>
        /// <param name="archiveFileStream">The file stream for the archive</param>
        /// <returns>The archive data</returns>
        public object LoadArchive(Stream archiveFileStream)
        {
            // Set the stream position to 0
            archiveFileStream.Position = 0;

            // Load the current file
            var data = BinarySerializableHelpers.ReadFromStream<UbiArtIpkData>(archiveFileStream, Settings, RCPServices.App.GetBinarySerializerLogger());

            RL.Logger?.LogInformationSource($"Read IPK file ({data.Version}) with {data.FilesCount} files");

            return data;
        }

        /// <summary>
        /// Creates a new archive object
        /// </summary>
        /// <returns>The archive object</returns>
        public object CreateArchive()
        {
            // Create the data
            var data = new UbiArtIpkData();

            // Configure the data
            Config.ConfigureIpkData(data);

            return data;
        }

        /// <summary>
        /// Gets info to display about the file
        /// </summary>
        /// <param name="archive">The archive</param>
        /// <param name="fileEntry">The file entry</param>
        /// <returns>The info items to display</returns>
        public IEnumerable<DuoGridItemViewModel> GetFileInfo(object archive, object fileEntry)
        {
            var entry = (UbiArtIPKFileEntry)fileEntry;
            var ipk = (UbiArtIpkData)archive;

            // TODO-UPDATE: Localize
            yield return new DuoGridItemViewModel("File size:", $"{ByteSize.FromBytes(entry.Size)}");
            yield return new DuoGridItemViewModel("Compressed file size:", $"{ByteSize.FromBytes(entry.CompressedSize)}");
            yield return new DuoGridItemViewModel("Pointer:", $"0x{entry.Offsets.First() + ipk.BaseOffset:X8}", UserLevel.Technical);
            yield return new DuoGridItemViewModel("Compressed:", $"{entry.IsCompressed}");
        }

        #endregion
    }
}