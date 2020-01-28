using RayCarrot.IO;
using RayCarrot.Rayman;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// The base Rayman 3 CNT explorer utility
    /// </summary>
    public abstract class R3BaseCNTExplorerUtility : BaseOpenSpaceCNTExplorerUtility
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="game">The game</param>
        protected R3BaseCNTExplorerUtility(Games game)
        {
            // Get the game install directory
            var installDir = game.GetInstallDir();

            // Set properties
            var archiveFiles = new FileSystemPath[]
            {
                // Demo
                installDir + "Gamedatabin" + "tex32.cnt",
                
                // Retail
                installDir + "Gamedatabin" + "tex32_1.cnt",
                installDir + "Gamedatabin" + "tex32_2.cnt",
                
                // Loading screen images
                installDir + "Gamedatabin" + "vignette.cnt",
            };

            ViewModel = new BaseOpenSpaceCNTExplorerUtilityViewModel(OpenSpaceGameMode.Rayman3PC, archiveFiles);
        }

        /// <summary>
        /// The view model
        /// </summary>
        public override BaseOpenSpaceCNTExplorerUtilityViewModel ViewModel { get; }
    }
}