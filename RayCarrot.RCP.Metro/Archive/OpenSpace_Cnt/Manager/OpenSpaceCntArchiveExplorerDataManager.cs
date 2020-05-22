using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using RayCarrot.Binary;
using RayCarrot.CarrotFramework.Abstractions;
using RayCarrot.Extensions;
using RayCarrot.Logging;
using RayCarrot.Rayman.OpenSpace;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// Archive explorer data manager for an OpenSpace .cnt file
    /// </summary>
    public class OpenSpaceCntArchiveExplorerDataManager : BaseOpenSpaceCntArchiveDataManager, IArchiveExplorerDataManager
    {
        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="settings">The settings when serializing the data</param>
        public OpenSpaceCntArchiveExplorerDataManager(OpenSpaceSettings settings) : base(settings)
        {

        }

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
                        Select(f => new OpenSpaceCntArchiveFileData(f, Settings, dir) as IArchiveFileData).
                        ToArray());
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
            var data = BinarySerializableHelpers.ReadFromStream<OpenSpaceCntData>(archiveFileStream, Settings, RCFRCP.App.GetBinarySerializerLogger());

            RL.Logger?.LogInformationSource($"Read CNT file with {data.Files.Length} files and {data.Directories.Length} directories");

            return data;
        }

        #endregion
    }
}