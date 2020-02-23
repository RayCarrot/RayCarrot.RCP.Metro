using System.Collections.Generic;
using System.IO;
using System.Linq;
using RayCarrot.CarrotFramework.Abstractions;
using RayCarrot.Extensions;
using RayCarrot.Rayman.UbiArt;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// Archive explorer data manager for a UbiArt .ipk file
    /// </summary>
    public class UbiArtIPKArchiveExplorerDataManager : BaseUbiArtIPKArchiveDataManager, IArchiveExplorerDataManager
    {
        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="settings">The settings when serializing the data</param>
        public UbiArtIPKArchiveExplorerDataManager(UbiArtSettings settings) : base(new UbiArtIPKArchiveConfigViewModel(settings, UbiArtIPKArchiveConfigViewModel.FileCompressionMode.WasCompressed))
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

        #endregion
    }
}