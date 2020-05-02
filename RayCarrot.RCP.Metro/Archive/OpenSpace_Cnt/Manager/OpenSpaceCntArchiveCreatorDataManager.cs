using RayCarrot.Extensions;
using RayCarrot.IO;
using RayCarrot.Rayman.OpenSpace;
using System.Collections.Generic;
using System.Linq;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// Archive creator data manager for an OpenSpace .cnt file
    /// </summary>
    public class OpenSpaceCntArchiveCreatorDataManager : BaseOpenSpaceCntArchiveDataManager, IArchiveCreatorDataManager
    {
        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="settings">The settings when serializing the data</param>
        public OpenSpaceCntArchiveCreatorDataManager(OpenSpaceSettings settings) : base(settings)
        {

        }

        #endregion

        #region Public Properties

        /// <summary>
        /// The default archive file name to use when creating an archive
        /// </summary>
        public string DefaultArchiveFileName => "Textures.cnt";

        /// <summary>
        /// Gets the configuration UI to use for creator
        /// </summary>
        public object GetCreatorUIConfig => null;

        #endregion

        #region Public Methods

        /// <summary>
        /// Gets a new archive from a collection of modified files
        /// </summary>
        /// <param name="files">The files to import</param>
        /// <returns>The archive creation data</returns>
        public ArchiveCreationData GetArchive(IEnumerable<FileSystemPath> files)
        {
            // Create the entry data for each file
            var fileEntries = files.Select(x => new ArchiveCreationData.FileEntryImportData(new OpenSpaceCntFileEntry()
            {
                XORKey = 0,
                FileName = x.Name
            }, x)).ToArray();

            // Create the .cnt data
            var data = new OpenSpaceCntData
            {
                // Set the paths
                Directories = fileEntries.Select(x => x.RelativeImportFilePath.Parent.FullPath).Where(x => !x.IsNullOrWhiteSpace()).Distinct().ToArray(),
                Files = fileEntries.Select(x => x.FileEntry.CastTo<OpenSpaceCntFileEntry>()).ToArray(),

                // Disable the XOR encryption
                IsXORUsed = false
            };

            // Set the directory indexes
            foreach (var entry in fileEntries)
            {
                // Get the file entry
                var file = entry.FileEntry.CastTo<OpenSpaceCntFileEntry>();

                // Set the directory index
                file.DirectoryIndex = data.Directories.FindItemIndex(x => x == entry.RelativeImportFilePath.Parent.FullPath);
            }

            return new ArchiveCreationData(data, fileEntries);
        }

        #endregion
    }
}