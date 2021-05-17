using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ByteSizeLib;
using RayCarrot.Binary;
using RayCarrot.Common;
using RayCarrot.IO;
using RayCarrot.Logging;
using RayCarrot.Rayman;
using RayCarrot.Rayman.OpenSpace;
using RayCarrot.WPF;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// Archive data manager for an OpenSpace .cnt file
    /// </summary>
    public class OpenSpaceCntArchiveDataManager : IArchiveDataManager
    {
        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="settings">The settings when serializing the data</param>
        public OpenSpaceCntArchiveDataManager(OpenSpaceSettings settings) => Settings = settings;

        #endregion

        #region Public Properties

        /// <summary>
        /// The path separator character to use. This is usually \ or /.
        /// </summary>
        public char PathSeparatorCharacter => '\\';

        /// <summary>
        /// Indicates if directories can be created and deleted
        /// </summary>
        public bool CanModifyDirectories => true;

        /// <summary>
        /// The file extension for the archive file
        /// </summary>
        public FileExtension ArchiveFileExtension => new FileExtension(".cnt");

        /// <summary>
        /// The serializer settings to use for the archive
        /// </summary>
        public BinarySerializerSettings SerializerSettings => Settings;

        /// <summary>
        /// The default archive file name to use when creating an archive
        /// </summary>
        public string DefaultArchiveFileName => "Textures.cnt";

        /// <summary>
        /// Gets the configuration UI to use for creator
        /// </summary>
        public object GetCreatorUIConfig => null;

        public OpenSpaceSettings Settings { get; }

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
            var file = fileEntry.CastTo<OpenSpaceCntFileEntry>();

            // Update the size
            file.Size = (uint)inputStream.Length;

            // Remove the encryption
            file.FileXORKey = new byte[4];
        }

        /// <summary>
        /// Decodes the file data from the input stream, or nothing if the data is not encoded
        /// </summary>
        /// <param name="inputStream">The input data stream to decode</param>
        /// <param name="outputStream">The output data stream for the decoded data</param>
        /// <param name="fileEntry">The file entry for the file to decode</param>
        public void DecodeFile(Stream inputStream, Stream outputStream, object fileEntry)
        {
            var entry = (OpenSpaceCntFileEntry)fileEntry;

            if (entry.FileXORKey.Any(x => x != 0))
                // Decrypt the bytes
                new MultiXORDataEncoder(entry.FileXORKey, true).Decode(inputStream, outputStream);
        }

        /// <summary>
        /// Gets the file data from the archive using a generator
        /// </summary>
        /// <param name="generator">The generator</param>
        /// <param name="fileEntry">The file entry</param>
        /// <returns>The encoded file data</returns>
        public Stream GetFileData(IDisposable generator, object fileEntry) => generator.CastTo<IArchiveFileGenerator<OpenSpaceCntFileEntry>>().GetFileStream((OpenSpaceCntFileEntry)fileEntry);

        /// <summary>
        /// Writes the files to the archive
        /// </summary>
        /// <param name="generator">The generator</param>
        /// <param name="archive">The loaded archive data</param>
        /// <param name="outputFileStream">The file output stream for the archive</param>
        /// <param name="files">The files to include</param>
        public void WriteArchive(IDisposable generator, object archive, Stream outputFileStream, IList<ArchiveFileItem> files)
        {
            RL.Logger?.LogInformationSource($"A CNT archive is being repacked...");

            // Get the archive data
            var data = archive.CastTo<OpenSpaceCntData>();

            // Create the file generator
            using var fileGenerator = new ArchiveFileGenerator<OpenSpaceCntFileEntry>();

            // Get files and entries
            var archiveFiles = files.Select(x => new
            {
                Entry = (OpenSpaceCntFileEntry)x.ArchiveEntry,
                FileItem = x
            }).ToArray();

            // Set files and directories
            data.Directories = files.Select(x => x.Directory).Distinct().Where(x => !x.IsNullOrWhiteSpace()).ToArray();
            data.Files = archiveFiles.Select(x => x.Entry).ToArray();

            // Set the directory indexes
            foreach (var file in archiveFiles)
                // Set the directory index
                file.Entry.DirectoryIndex = file.FileItem.Directory == String.Empty ? -1 : data.Directories.FindItemIndex(x => x == file.FileItem.Directory);

            // Set the current pointer position to the header size
            var pointer = data.GetHeaderSize(Settings);

            // Disable checksum
            data.IsChecksumUsed = false;
            data.DirChecksum = 0;

            // NOTE: We can't disable the XOR key entirely as that would disable it for the file bytes too, which would require them all to be decrypted
            // Reset XOR keys
            data.XORKey = 0;

            // Load each file
            foreach (var file in archiveFiles)
            {
                // Get the file entry
                var entry = file.Entry;

                // Reset checksum and XOR key
                entry.Checksum = 0;
                entry.XORKey = 0;

                // Add to the generator
                fileGenerator.Add(entry, () =>
                {
                    // Get the file stream to write to the archive
                    var fileStream = file.FileItem.GetFileData(generator).Stream;

                    // Set the pointer
                    entry.Pointer = pointer;

                    // Update the pointer by the file size
                    pointer += entry.Size;

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

            RL.Logger?.LogInformationSource($"The CNT archive has been repacked");
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
            var data = archive.CastTo<OpenSpaceCntData>();

            RL.Logger?.LogInformationSource("The directories are being retrieved for a CNT archive");

            // Helper method for getting the directories
            IEnumerable<ArchiveDirectory> GetDirectories()
            {
                // Add the directories to the collection
                for (var i = -1; i < data.Directories.Length; i++)
                {
                    // Get the directory path
                    var dir = i == -1 ? String.Empty : data.Directories[i];

                    // Get the directory index
                    var dirIndex = i;

                    // Return each directory with the available files, including the root directory
                    yield return new ArchiveDirectory(dir, data.Files.
                        Where(x => x.DirectoryIndex == dirIndex).
                        Select(f => new ArchiveFileItem(this, f.FileName, dir, f)).
                        ToArray());
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
            var data = BinarySerializableHelpers.ReadFromStream<OpenSpaceCntData>(archiveFileStream, Settings, RCPServices.App.GetBinarySerializerLogger());

            RL.Logger?.LogInformationSource($"Read CNT file with {data.Files.Length} files and {data.Directories.Length} directories");

            return data;
        }

        /// <summary>
        /// Creates a new archive object
        /// </summary>
        /// <returns>The archive object</returns>
        public object CreateArchive()
        {
            // Create the .cnt data
            return new OpenSpaceCntData
            {
                // Disable the XOR encryption
                IsXORUsed = false
            };
        }

        /// <summary>
        /// Gets info to display about the file
        /// </summary>
        /// <param name="archive">The archive</param>
        /// <param name="fileEntry">The file entry</param>
        /// <returns>The info items to display</returns>
        public IEnumerable<DuoGridItemViewModel> GetFileInfo(object archive, object fileEntry)
        {
            var entry = (OpenSpaceCntFileEntry)fileEntry;
            var cnt = (OpenSpaceCntData)archive;

            yield return new DuoGridItemViewModel(Resources.Archive_FileInfo_Size, $"{ByteSize.FromBytes(entry.Size)}");

            if (cnt.Files.Contains(entry))
                yield return new DuoGridItemViewModel(Resources.Archive_FileInfo_Pointer, $"0x{entry.Pointer:X8}", UserLevel.Technical);
            
            yield return new DuoGridItemViewModel(Resources.Archive_FileInfo_IsEncrypted, $"{entry.FileXORKey.Any(x => x != 0)}", UserLevel.Advanced);
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
            return new OpenSpaceCntFileEntry()
            {
                FileName = fileName
            };
        }

        /// <summary>
        /// Gets the size of the file entry
        /// </summary>
        /// <param name="fileEntry">The file entry object</param>
        /// <param name="encoded">True if the size is for the encoded file, false if for the decoded file</param>
        /// <returns>The size, or null if it could not be determined</returns>
        public long? GetFileSize(object fileEntry, bool encoded)
        {
            var entry = fileEntry.CastTo<OpenSpaceCntFileEntry>();

            return entry.Size;
        }

        #endregion

        #region Events

        /// <summary>
        /// Occurs when a file is being written to an archive
        /// </summary>
        public event EventHandler<ValueEventArgs<ArchiveFileItem>> OnWritingFileToArchive;

        #endregion
    }
}