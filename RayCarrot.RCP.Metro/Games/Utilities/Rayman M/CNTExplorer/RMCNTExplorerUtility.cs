using RayCarrot.IO;
using RayCarrot.Rayman;
using RayCarrot.RCP.Core;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// The Rayman M CNT explorer utility
    /// </summary>
    public class RMCNTExplorerUtility : BaseOpenSpaceCNTExplorerUtility
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public RMCNTExplorerUtility()
        {
            // Get the game install directory
            var installDir = Games.RaymanM.GetInstallDir();

            // Set properties
            var archiveFiles = new FileSystemPath[]
            {
                installDir + "FishBin" + "tex32.cnt",
                installDir + "FishBin" + "vignette.cnt",
                installDir + "MenuBin" + "tex32.cnt",
                installDir + "MenuBin" + "vignette.cnt",
                installDir + "TribeBin" + "tex32.cnt",
                installDir + "TribeBin" + "vignette.cnt",
            };

            ViewModel = new BaseOpenSpaceCNTExplorerUtilityViewModel(OpenSpaceGameMode.RaymanMPC, archiveFiles);
        }

        /// <summary>
        /// The view model
        /// </summary>
        public override BaseOpenSpaceCNTExplorerUtilityViewModel ViewModel { get; }
    }
}