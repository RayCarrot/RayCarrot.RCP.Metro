using RayCarrot.Common;
using RayCarrot.IO;
using RayCarrot.Rayman;
using RayCarrot.Rayman.Ray1;
using RayCarrot.UI;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// View model for a Rayman 1 archive explorer utility
    /// </summary>
    public class BaseRay1ArchiveExplorerUtilityViewModel : BaseRCPViewModel
    {
        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        public BaseRay1ArchiveExplorerUtilityViewModel(GameMode gameMode, FileSystemPath[] archiveFiles)
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
        public GameMode GameMode { get; }

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
            var attr = GameMode.GetAttribute<Ray1GameModeInfoAttribute>();
            var settings = Ray1Settings.GetDefaultSettings(attr.Game, attr.Platform);

            // Show the archive explorer
            await RCPServices.UI.ShowArchiveExplorerAsync(new Ray1PCArchiveDataManager(settings), ArchiveFiles.Where(x => x.FileExists));
        }

        #endregion
    }
}