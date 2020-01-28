using RayCarrot.IO;
using RayCarrot.Rayman;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// The base Rayman M CNT explorer utility
    /// </summary>
    public abstract class RMBaseCNTExplorerUtility : BaseOpenSpaceCNTExplorerUtility
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="game">The game</param>
        protected RMBaseCNTExplorerUtility(Games game)
        {
            // Get the game install directory
            var installDir = game.GetInstallDir();

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