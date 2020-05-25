using RayCarrot.IO;
using RayCarrot.UI;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using RayCarrot.Common;
using RayCarrot.Rayman.UbiArt;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// View model for a UbiArt IPK explorer utility
    /// </summary>
    public class BaseUbiArtIPKExplorerUtilityViewModel : BaseRCPViewModel
    {
        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        public BaseUbiArtIPKExplorerUtilityViewModel(UbiArtGameMode gameMode, FileSystemPath[] archiveFiles)
        {
            // Set properties
            GameMode = gameMode;
            ArchiveFiles = archiveFiles;

            // Create commands
            OpenCommand = new AsyncRelayCommand(OpenAsync);
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// The game mode
        /// </summary>
        public UbiArtGameMode GameMode { get; }

        /// <summary>
        /// The game install directory
        /// </summary>
        public FileSystemPath[] ArchiveFiles { get; }

        #endregion

        #region Commands

        public ICommand OpenCommand { get; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Opens the archive explorer
        /// </summary>
        /// <returns>The task</returns>
        public async Task OpenAsync()
        {
            var attr = GameMode.GetAttribute<UbiArtGameModeInfoAttribute>();
            var settings = UbiArtSettings.GetDefaultSettings(attr.Game, attr.Platform);

            // Show the archive explorer
            await RCPServices.UI.ShowArchiveExplorerAsync(new UbiArtIPKArchiveExplorerDataManager(settings), ArchiveFiles.Where(x => x.FileExists));
        }

        #endregion
    }
}