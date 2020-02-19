using RayCarrot.IO;
using RayCarrot.Rayman.OpenSpace;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// The Rayman Arena CNT explorer utility
    /// </summary>
    public class RACNTExplorerUtility : BaseOpenSpaceCNTExplorerUtility
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public RACNTExplorerUtility()
        {
            // Get the game install directory
            var installDir = Games.RaymanArena.GetInstallDir();

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

            ViewModel = new BaseOpenSpaceCNTExplorerUtilityViewModel(OpenSpaceGameMode.RaymanArenaPC, archiveFiles);
        }

        /// <summary>
        /// The view model
        /// </summary>
        public override BaseOpenSpaceCNTExplorerUtilityViewModel ViewModel { get; }
    }
}