using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using RayCarrot.Binary;
using RayCarrot.Common;
using RayCarrot.IO;
using RayCarrot.Logging;
using RayCarrot.Rayman;
using RayCarrot.Rayman.OpenSpace;

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
        public byte[] GetFileData(IDisposable generator, object fileEntry) => generator.CastTo<IArchiveFileGenerator<OpenSpaceCntFileEntry>>().GetBytes((OpenSpaceCntFileEntry)fileEntry);

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
                Entry = (x.ArchiveEntry as OpenSpaceCntFileEntry) ?? new OpenSpaceCntFileEntry
                {
                    FileName = x.FileName
                },
                FileItem = x
            }).ToArray();

            // Set files and directories
            data.Directories = files.Select(x => x.Directory).Distinct().ToArray();
            data.Files = archiveFiles.Select(x => x.Entry).ToArray();

            // Set the directory indexes
            foreach (var file in archiveFiles)
                // Set the directory index
                file.Entry.DirectoryIndex = data.Directories.FindItemIndex(x => x == file.FileItem.Directory);

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
                    // Get the file bytes to write to the archive
                    var fileStream = file.FileItem.GetFileData(generator);

                    // Set the pointer
                    entry.Pointer = pointer;

                    // Update the pointer by the file size
                    pointer += entry.Size;

                    // TODO-UPDATE: Return stream directly
                    return fileStream.Stream.ReadRemainingBytes();
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
        /// <returns>The archive data</returns>
        public ArchiveData LoadArchiveData(object archive, Stream archiveFileStream)
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

        #endregion
    }
}