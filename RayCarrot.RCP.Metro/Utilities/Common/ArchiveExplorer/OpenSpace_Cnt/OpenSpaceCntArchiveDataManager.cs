using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using RayCarrot.Extensions;
using RayCarrot.IO;
using RayCarrot.Rayman;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// Archive data manager for an OpenSpace .cnt file
    /// </summary>
    public class OpenSpaceCntArchiveDataManager : IArchiveDataManager
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="settings">The settings when serializing the data</param>
        public OpenSpaceCntArchiveDataManager(OpenSpaceSettings settings)
        {
            Settings = settings;
        }

        /// <summary>
        /// The settings when serializing the data
        /// </summary>
        protected OpenSpaceSettings Settings { get; }

        /// <summary>
        /// Gets the available directories from the archive along with their contents
        /// </summary>
        /// <param name="archiveFileStream">The file stream for the archive</param>
        /// <returns>The directories</returns>
        public IEnumerable<ArchiveDirectory> GetDirectories(Stream archiveFileStream)
        {
            // Read the file data
            var data = new OpenSpaceCntSerializer(Settings).Deserialize(archiveFileStream);

            // Add the directories to the collection
            for (var i = -1; i < data.Directories.Length; i++)
            {
                // Get the directory path
                var dir = i == -1 ? String.Empty : data.Directories[i];

                // Return each directory with the available files, including the root directory
                yield return new ArchiveDirectory(dir, data.Files.Where(x => x.DirectoryIndex == i).Select(f => new OpenSpaceCntArchiveFileData(f, Settings, dir) as IArchiveFileData).ToArray());
            }
        }

        /// <summary>
        /// Updates the archive with the modified files
        /// </summary>
        /// <param name="archiveFileStream">The file stream for the archive</param>
        /// <param name="modifiedFiles">The modified files to update in the archive</param>
        public void UpdateArchive(Stream archiveFileStream, IEnumerable<IArchiveFileData> modifiedFiles)
        {
            // Set the stream position to 0
            archiveFileStream.Position = 0;

            // Load the current file
            var data = new OpenSpaceCntSerializer(Settings).Deserialize(archiveFileStream);

            // Get the modified files
            var files = modifiedFiles.ToDictionary(x => Path.Combine(x.Directory, x.FileName));

            // Create a temporary directory to load the current files into 
            using var tempDir = new TempDirectory(true);

            // Create the file generator
            data.FileGenerator = new Dictionary<string, Func<byte[]>>();

            // The current pointer position
            var pointer = data.GetHeaderSize();

            // Load each file data into temporary files as we're changing the archive structure by modifying the pointers
            foreach (var file in data.Files)
            {
                // Get the full path
                var fullPath = Path.Combine(file.DirectoryIndex == -1 ? String.Empty : data.Directories[file.DirectoryIndex], file.FileName);

                // Attempt to get the modified version of the file
                var modifiedFile = files.TryGetValue(fullPath);

                // Get the temporary file path
                FileSystemPath tempFilePath = tempDir.TempPath + $"{file.DirectoryIndex.ToString().PadLeft(3, '0')}{file.FileName}";

                // Check if the file is one of the modified files
                if (modifiedFile != null)
                {
                    // Remove the file from the dictionary
                    files.Remove(fullPath);

                    // Move the file
                    RCFRCP.File.MoveFile(modifiedFile.PendingImportTempPath, tempFilePath, true);

                    // Set the file size
                    file.Size = (int)tempFilePath.GetSize().Bytes;

                    //// Remove the encryption
                    file.FileXORKey = new byte[]
                    {
                        0, 0, 0, 0
                    };
                }
                // Use the original file without decrypting it
                else
                {
                    File.WriteAllBytes(tempFilePath, file.GetFileBytes(archiveFileStream, false));
                }

                // Add to the generator
                data.FileGenerator.Add(fullPath, () => File.ReadAllBytes(tempFilePath));

                // Set the pointer
                file.Pointer = pointer;

                // Increase by the file size
                pointer += file.Size;
            }

            // TODO: Backup existing .cnt file in case it fails

            // Set the stream position to 0
            archiveFileStream.Position = 0;

            // Serialize the data
            new OpenSpaceCntSerializer(Settings).Serialize(archiveFileStream, data);
        }
    }
}