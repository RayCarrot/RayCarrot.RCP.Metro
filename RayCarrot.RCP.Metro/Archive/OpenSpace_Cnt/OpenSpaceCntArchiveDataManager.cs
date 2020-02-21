using RayCarrot.CarrotFramework.Abstractions;
using RayCarrot.Extensions;
using RayCarrot.Rayman;
using RayCarrot.Rayman.OpenSpace;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using RayCarrot.IO;

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
        public OpenSpaceCntArchiveDataManager(OpenSpaceSettings settings)
        {
            Settings = settings;
        }

        #endregion

        #region Protected Properties

        /// <summary>
        /// The settings when serializing the data
        /// </summary>
        protected OpenSpaceSettings Settings { get; }

        // TODO: Option for this
        /// <summary>
        /// Indicates if the files should be encrypted when imported. Defaulted to false due to encrypting the file being very slow.
        /// </summary>
        protected virtual bool EncryptFiles => false;

        #endregion

        #region Public Properties

        /// <summary>
        /// The path separator character to use. This is usually \ or /.
        /// </summary>
        public char PathSeparatorCharacter => '\\';

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
            var data = archive.CastTo<OpenSpaceCntData>();

            RCFCore.Logger?.LogInformationSource("The directories are being retrieved for a CNT archive");

            // Helper method for getting the directories
            IEnumerable<ArchiveDirectory> GetDirectories()
            {
                // Add the directories to the collection
                for (var i = -1; i < data.Directories.Length; i++)
                {
                    // Get the directory path
                    var dir = i == -1 ? String.Empty : data.Directories[i];

                    // Return each directory with the available files, including the root directory
                    yield return new ArchiveDirectory(dir, data.Files.Where(x => x.DirectoryIndex == i).Select(f => new OpenSpaceCntArchiveFileData(f, Settings, dir) as IArchiveFileData).ToArray());
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
            var data = OpenSpaceCntData.GetSerializer(Settings).Deserialize(archiveFileStream);

            RCFCore.Logger?.LogInformationSource($"Read CNT file ({data.VersionID}) with {data.Files.Length} files and {data.Directories.Length} directories");

            return data;
        }

        /// <summary>
        /// Gets a new archive from a collection of modified files
        /// </summary>
        /// <param name="files"></param>
        /// <returns>The archive</returns>
        public object GetArchive(IEnumerable<IArchiveImportData> files)
        {
            throw new NotImplementedException();

            return new OpenSpaceCntData
            {
                Signature = 0,
                XORKey = 0,
                Directories = new string[]
                {
                },
                VersionID = 0,
                Files = new OpenSpaceCntFileEntry[]
                {
                }
            };
        }

        /// <summary>
        /// Gets a new file entry from the specified path
        /// </summary>
        /// <param name="relativePath">The relative path of the file</param>
        /// <returns>The file entry</returns>
        public object GetFileEntry(FileSystemPath relativePath)
        {
            throw new NotImplementedException();

            // TODO-UPDATE: Clean up and handle dir
            return new OpenSpaceCntFileEntry(0)
            {
                DirectoryIndex = 0,
                FileName = relativePath.Name,
                FileXORKey = new byte[]
                {
                    0, 0, 0, 0
                },
                Unknown1 = 0,
                Pointer = 0,
                Size = 0
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
            var file = fileEntry.CastTo<OpenSpaceCntFileEntry>();

            // Update the size
            file.Size = fileData.Length;

            // Remove the encryption if not encrypting files
            if (!EncryptFiles)
            {
                file.FileXORKey = new byte[]
                {
                    0, 0, 0, 0
                };

                RCFCore.Logger?.LogTraceSource($"The encryption has been removed for {file.FileName}");

                return fileData;
            }
            else
            {
                return new MultiXORDataEncoder(file.FileXORKey, true).Encode(fileData);
            }
        }

        /// <summary>
        /// Updates the archive with the modified files
        /// </summary>
        /// <param name="archive">The loaded archive data</param>
        /// <param name="outputFileStream">The file stream for the updated archive</param>
        /// <param name="files">The import data for the archive files</param>
        public void UpdateArchive(object archive, Stream outputFileStream, IEnumerable<IArchiveImportData> files)
        {
            RCFCore.Logger?.LogInformationSource($"A CNT archive is being repacked...");

            // Get the archive data
            var data = archive.CastTo<OpenSpaceCntData>();

            // Create the file generator
            using var fileGenerator = new ArchiveFileGenerator<OpenSpaceCntFileEntry>();

            // Set the current pointer position to the header size
            var pointer = data.GetHeaderSize(Settings);

            // Load each file
            foreach (var importData in files)
            {
                // Get the file entry
                var file = importData.FileEntryData.CastTo<OpenSpaceCntFileEntry>();

                // NOTE: Leaving this unknown value causes the game to crash if the texture is modified - why? Setting it to 0 always seems to work.
                // Remove unknown value
                file.Unknown1 = 0;

                // Add to the generator
                fileGenerator.Add(file, () =>
                {
                    // Get the file bytes to write to the archive
                    var bytes = importData.GetData(file);

                    // Set the pointer
                    file.Pointer = pointer;

                    // Update the pointer by the file size
                    pointer += file.Size;

                    return bytes;
                });
            }

            // Write the files
            data.WriteArchiveContent(outputFileStream, fileGenerator);

            outputFileStream.Position = 0;

            // Serialize the data
            OpenSpaceCntData.GetSerializer(Settings).Serialize(outputFileStream, data);

            RCFCore.Logger?.LogInformationSource($"The CNT archive has been repacked");
        }

        #endregion
    }
}