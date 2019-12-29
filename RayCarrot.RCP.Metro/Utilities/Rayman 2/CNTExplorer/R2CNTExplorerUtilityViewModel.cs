using RayCarrot.IO;
using RayCarrot.Rayman;
using RayCarrot.RCP.ArchiveExplorer;
using RayCarrot.RCP.Core;
using RayCarrot.UI;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// View model for the Rayman 2 CNT explorer utility
    /// </summary>
    public class R2CNTExplorerUtilityViewModel : BaseRCPViewModel
    {
        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        public R2CNTExplorerUtilityViewModel()
        {
            // Get the game install directory
            var installDir = Games.Rayman2.GetInstallDir();

            // Set properties
            ArchiveFiles = new FileSystemPath[]
            {
                installDir + "Data" + "Textures.cnt",
                installDir + "Data" + "Vignette.cnt",
            };

            // Create commands
            OpenCommand = new AsyncRelayCommand(OpenAsync);
        }

        #endregion

        #region Public Properties

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
            await ArchiveExplorerUI.ShowAsync(new OpenSpaceCntArchiveDataManager(OpenSpaceGameMode.Rayman2PC.GetSettings()), ArchiveFiles.Where(x => x.FileExists));
        }

        #endregion
    }
}