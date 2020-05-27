using RayCarrot.IO;
using RayCarrot.Rayman;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// The base Rayman 2 CNT explorer utility
    /// </summary>
    public abstract class R2BaseCNTExplorerUtility : BaseOpenSpaceCNTExplorerUtility
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="game">The game</param>
        /// <param name="gameMode">The game mode</param>
        protected R2BaseCNTExplorerUtility(Games game, GameMode gameMode)
        {
            // Get the game install directory
            var installDir = game.GetInstallDir();

            // Set properties
            var archiveFiles = new FileSystemPath[]
            {
                // Demo
                installDir + "BinData" + "Textures.cnt",

                // Retail
                installDir + "Data" + "Textures.cnt",
                installDir + "Data" + "Vignette.cnt",
            };

            ViewModel = new BaseOpenSpaceCNTExplorerUtilityViewModel(gameMode, archiveFiles);
        }

        /// <summary>
        /// The view model
        /// </summary>
        public override BaseOpenSpaceCNTExplorerUtilityViewModel ViewModel { get; }
    }
}