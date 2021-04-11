using RayCarrot.Rayman;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// The Rayman by his Fans archive explorer utility
    /// </summary>
    public class RFArchiveExplorerUtility : BaseRay1ExplorerUtility
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public RFArchiveExplorerUtility()
        {
            // Get the game install directory
            var installDir = Games.RaymanByHisFans.GetInstallDir();

            ViewModel = new BaseRay1ArchiveExplorerUtilityViewModel(GameMode.RayFanPC, GetArchiveFiles(installDir));
        }

        /// <summary>
        /// The view model
        /// </summary>
        public override BaseRay1ArchiveExplorerUtilityViewModel ViewModel { get; }
    }
}