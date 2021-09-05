using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ByteSizeLib;
using RayCarrot.Binary;
using RayCarrot.IO;
using RayCarrot.Logging;
using RayCarrot.Rayman;
using RayCarrot.Rayman.Ray1;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// Archive data manager for Rayman 1 PC spin-off .dat files
    /// </summary>
    public class Ray1PCArchiveDataManager : IArchiveDataManager
    {
        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="configViewModel">The configuration view model</param>
        public Ray1PCArchiveDataManager(Ray1PCArchiveConfigViewModel configViewModel)
        {
            Config = configViewModel;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// The path separator character to use. This is usually \ or /.
        /// </summary>
        public char PathSeparatorCharacter => '\\';

        /// <summary>
        /// Indicates if directories can be created and deleted
        /// </summary>
        public bool CanModifyDirectories => false;

        /// <summary>
        /// The file extension for the archive file
        /// </summary>
        public FileExtension ArchiveFileExtension => new FileExtension(".dat");

        /// <summary>
        /// The serializer settings to use for the archive
        /// </summary>
        public BinarySerializerSettings SerializerSettings => Settings;

        /// <summary>
        /// The default archive file name to use when creating an archive
        /// </summary>
        public string DefaultArchiveFileName => "Archive.dat";

        /// <summary>
        /// Gets the configuration UI to use for creator
        /// </summary>
        public object GetCreatorUIConfig => new Ray1PCArchiveConfigUI()
        {
            DataContext = Config
        };

        /// <summary>
        /// The settings when serializing the data
        /// </summary>
        protected Ray1Settings Settings => Config.Settings;

        /// <summary>
        /// The configuration view model
        /// </summary>
        protected Ray1PCArchiveConfigViewModel Config { get; }

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
            var file = fileEntry.CastTo<Rayman1PCArchiveEntry>();

            // Update the size
            file.FileSize = (uint)inputStream.Length;

            // Remove the encryption
            file.XORKey = 0;

            // Calculate the checksum
            var c = new Checksum8Calculator();
            c.AddBytes(inputStream.ReadRemainingBytes());
            file.Checksum = c.ChecksumValue;

            inputStream.Position = 0;
        }

        /// <summary>
        /// Decodes the file data from the input stream, or nothing if the data is not encoded
        /// </summary>
        /// <param name="inputStream">The input data stream to decode</param>
        /// <param name="outputStream">The output data stream for the decoded data</param>
        /// <param name="fileEntry">The file entry for the file to decode</param>
        public void DecodeFile(Stream inputStream, Stream outputStream, object fileEntry)
        {
            var entry = (Rayman1PCArchiveEntry)fileEntry;

            if (entry.XORKey != 0)
                // Decrypt the bytes
                new XORDataEncoder(entry.XORKey).Decode(inputStream, outputStream);
        }

        /// <summary>
        /// Gets the file data from the archive using a generator
        /// </summary>
        /// <param name="generator">The generator</param>
        /// <param name="fileEntry">The file entry</param>
        /// <returns>The encoded file data</returns>
        public Stream GetFileData(IDisposable generator, object fileEntry) => generator.CastTo<IArchiveFileGenerator<Rayman1PCArchiveEntry>>().GetFileStream((Rayman1PCArchiveEntry)fileEntry);

        /// <summary>
        /// Writes the files to the archive
        /// </summary>
        /// <param name="generator">The generator</param>
        /// <param name="archive">The loaded archive data</param>
        /// <param name="outputFileStream">The file output stream for the archive</param>
        /// <param name="files">The files to include</param>
        public void WriteArchive(IDisposable generator, object archive, Stream outputFileStream, IList<ArchiveFileItem> files)
        {
            RL.Logger?.LogInformationSource($"An R1 PC archive is being repacked...");

            // Get the archive data
            var data = archive.CastTo<Rayman1PCArchiveData>();

            // Create the file generator
            using var fileGenerator = new ArchiveFileGenerator<Rayman1PCArchiveEntry>();

            // Get files and entries
            var archiveFiles = files.Select(x => new
            {
                Entry = (Rayman1PCArchiveEntry)x.ArchiveEntry,
                FileItem = x
            }).ToArray();

            // Set files and directories
            data.Files = archiveFiles.Select(x => x.Entry).ToArray();

            // Set the current pointer position to the header size
            var pointer = data.GetHeaderSize(Settings);

            // Load each file
            foreach (var file in archiveFiles)
            {
                // Process the file name
                file.Entry.FileName = ProcessFileName(file.Entry.FileName);

                // Get the file entry
                var entry = file.Entry;

                // Add to the generator
                fileGenerator.Add(entry, () =>
                {
                    // Get the file stream to write to the archive
                    var fileStream = file.FileItem.GetFileData(generator).Stream;

                    // Set the pointer
                    entry.FileOffset = pointer;

                    // Update the pointer by the file size
                    pointer += entry.FileSize;

                    // Invoke event
                    OnWritingFileToArchive?.Invoke(this, new ValueEventArgs<ArchiveFileItem>(file.FileItem));

                    return fileStream;
                });
            }

            // Write the files
            data.WriteArchiveContent(outputFileStream, fileGenerator);

            outputFileStream.Position = 0;

            // Serialize the data
            BinarySerializableHelpers.WriteToStream(data, outputFileStream, Settings, RCPServices.App.GetBinarySerializerLogger());

            RL.Logger?.LogInformationSource($"The R1 PC archive has been repacked");
        }

        /// <summary>
        /// Loads the archive data
        /// </summary>
        /// <param name="archive">The archive data</param>
        /// <param name="archiveFileStream">The archive file stream</param>
        /// <param name="fileName">The name of the file the archive is loaded from, if available</param>
        /// <returns>The archive data</returns>
        public ArchiveData LoadArchiveData(object archive, Stream archiveFileStream, string fileName)
        {
            // Get the data
            var data = archive.CastTo<Rayman1PCArchiveData>();

            RL.Logger?.LogInformationSource("The files are being retrieved for an R1 PC archive");

            var fileExt = fileName.StartsWith("VIGNET", StringComparison.OrdinalIgnoreCase) || fileName.StartsWith("WldDesc", StringComparison.OrdinalIgnoreCase) ? ".pcx" : ".dat";

            // Return the data
            return new ArchiveData(new ArchiveDirectory[]
            {
                new ArchiveDirectory(String.Empty, data.Files.Select(f => new ArchiveFileItem(this, $"{f.FileName}{fileExt}", String.Empty, f)).ToArray())
            }, data.GetArchiveContent(archiveFileStream));
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
            var data = BinarySerializableHelpers.ReadFromStream<Rayman1PCArchiveData>(archiveFileStream, Settings, RCPServices.App.GetBinarySerializerLogger());

            RL.Logger?.LogInformationSource($"Read R1 PC archive file with {data.Files.Length} files");

            return data;
        }

        /// <summary>
        /// Creates a new archive object
        /// </summary>
        /// <returns>The archive object</returns>
        public object CreateArchive()
        {
            // Create the archive data
            var archive = new Rayman1PCArchiveData();

            Config.ConfigureArchiveData(archive);

            return archive;
        }

        /// <summary>
        /// Gets info to display about the file
        /// </summary>
        /// <param name="archive">The archive</param>
        /// <param name="fileEntry">The file entry</param>
        /// <returns>The info items to display</returns>
        public IEnumerable<DuoGridItemViewModel> GetFileInfo(object archive, object fileEntry)
        {
            var entry = (Rayman1PCArchiveEntry)fileEntry;
            var archiveData = (Rayman1PCArchiveData)archive;

            yield return new DuoGridItemViewModel(Resources.Archive_FileInfo_Size, $"{ByteSize.FromBytes(entry.FileSize)}");

            if (archiveData.Files.Contains(entry))
                yield return new DuoGridItemViewModel(Resources.Archive_FileInfo_Pointer, $"0x{entry.FileOffset:X8}", UserLevel.Technical);
            
            yield return new DuoGridItemViewModel(Resources.Archive_FileInfo_IsEncrypted, $"{entry.XORKey != 0}", UserLevel.Advanced);
        }

        /// <summary>
        /// Gets a new file entry object for the file
        /// </summary>
        /// <param name="archive">The archive</param>
        /// <param name="directory">The directory</param>
        /// <param name="fileName">The file name</param>
        /// <returns>The file entry object</returns>
        public object GetNewFileEntry(object archive, string directory, string fileName)
        {
            return new Rayman1PCArchiveEntry()
            {
                FileName = ProcessFileName(fileName)
            };
        }

        /// <summary>
        /// Updates the file name by removing added file extensions and truncates the length
        /// </summary>
        /// <param name="fileName">The file name</param>
        /// <returns>The processed file name</returns>
        public string ProcessFileName(string fileName) => fileName.Replace(".dat", String.Empty).Replace(".pcx", String.Empty).Truncate(9);

        /// <summary>
        /// Gets the size of the file entry
        /// </summary>
        /// <param name="fileEntry">The file entry object</param>
        /// <param name="encoded">True if the size is for the encoded file, false if for the decoded file</param>
        /// <returns>The size, or null if it could not be determined</returns>
        public long? GetFileSize(object fileEntry, bool encoded)
        {
            var entry = fileEntry.CastTo<Rayman1PCArchiveEntry>();

            return entry.FileSize;
        }

        #endregion

        #region Events

        /// <summary>
        /// Occurs when a file is being written to an archive
        /// </summary>
        public event EventHandler<ValueEventArgs<ArchiveFileItem>> OnWritingFileToArchive;

        #endregion

        #region Static Helpers

        public static FileSystemPath[] GetArchiveFiles(FileSystemPath gameDir)
        {
            var nonArchiveDatFiles = new string[]
            {
                "ALLFIX.DAT",
                "BIGRAY.DAT",
                "CONCLU.DAT",
                "INTRO.DAT",
            };

            return Directory.GetFiles(gameDir, "*.dat", SearchOption.AllDirectories).Where(x => !nonArchiveDatFiles.Any(f => Path.GetFileName(x).Equals(f, StringComparison.OrdinalIgnoreCase))).Select(x => new FileSystemPath(x)).ToArray();
        }

        #endregion
    }
}