using RayCarrot.Extensions;
using RayCarrot.IO;
using RayCarrot.Rayman.UbiArt;
using System.Collections.Generic;
using System.Linq;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// Archive creator data manager for a UbiArt .ipk file
    /// </summary>
    public class UbiArtIPKArchiveCreatorDataManager : BaseUbiArtIPKArchiveDataManager, IArchiveCreatorDataManager
    {
        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="settings">The settings when serializing the data</param>
        public UbiArtIPKArchiveCreatorDataManager(UbiArtSettings settings) : base(new UbiArtIPKArchiveConfigViewModel(settings, UbiArtIPKArchiveConfigViewModel.FileCompressionMode.MatchesSetting))
        { }

        #endregion

        #region Public Properties

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
            var fileEntries = files.Select(x => new ArchiveCreationData.FileEntryImportData(new UbiArtIPKFileEntry()
            {
                Path = new UbiArtPath(x),
            }, x)).ToArray();

            // Create the data
            var data = new UbiArtIpkData();

            // Configure the data
            Config.ConfigureIpkData(data);

            // Set the files
            data.Files = fileEntries.Select(x => x.FileEntry.CastTo<UbiArtIPKFileEntry>()).ToArray();
            data.FilesCount = (uint)data.Files.Length;

            return new ArchiveCreationData(data, fileEntries);
        }

        #endregion
    }
}