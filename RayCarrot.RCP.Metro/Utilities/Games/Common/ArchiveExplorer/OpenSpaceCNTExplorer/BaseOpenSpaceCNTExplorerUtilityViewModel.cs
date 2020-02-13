using RayCarrot.IO;
using RayCarrot.Rayman;
using RayCarrot.UI;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using RayCarrot.Rayman.OpenSpace;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// View model for a OpenSpace CNT explorer utility
    /// </summary>
    public class BaseOpenSpaceCNTExplorerUtilityViewModel : BaseRCPViewModel
    {
        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        public BaseOpenSpaceCNTExplorerUtilityViewModel(OpenSpaceGameMode gameMode, FileSystemPath[] archiveFiles)
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
        public OpenSpaceGameMode GameMode { get; }

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
            // Show the archive explorer
            await RCFRCP.UI.ShowArchiveExplorerAsync(new OpenSpaceCntArchiveDataManager(GameMode.GetSettings()), ArchiveFiles.Where(x => x.FileExists));
        }

        #endregion
    }
}