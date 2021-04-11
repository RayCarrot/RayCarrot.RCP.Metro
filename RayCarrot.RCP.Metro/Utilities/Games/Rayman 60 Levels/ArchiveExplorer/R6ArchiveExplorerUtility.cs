using RayCarrot.Rayman;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// The Rayman 60 Levels archive explorer utility
    /// </summary>
    public class R6ArchiveExplorerUtility : BaseRay1ExplorerUtility
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public R6ArchiveExplorerUtility()
        {
            // Get the game install directory
            var installDir = Games.Rayman60Levels.GetInstallDir();

            ViewModel = new BaseRay1ArchiveExplorerUtilityViewModel(GameMode.Ray60nPC, GetArchiveFiles(installDir));
        }

        /// <summary>
        /// The view model
        /// </summary>
        public override BaseRay1ArchiveExplorerUtilityViewModel ViewModel { get; }
    }
}