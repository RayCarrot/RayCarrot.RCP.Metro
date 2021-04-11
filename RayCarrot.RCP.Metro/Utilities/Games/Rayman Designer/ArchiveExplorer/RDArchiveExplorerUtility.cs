using RayCarrot.Rayman;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// The Rayman Designer archive explorer utility
    /// </summary>
    public class RDArchiveExplorerUtility : BaseRay1ExplorerUtility
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public RDArchiveExplorerUtility()
        {
            // Get the game install directory
            var installDir = Games.RaymanDesigner.GetInstallDir();

            ViewModel = new BaseRay1ArchiveExplorerUtilityViewModel(GameMode.RayKitPC, GetArchiveFiles(installDir));
        }

        /// <summary>
        /// The view model
        /// </summary>
        public override BaseRay1ArchiveExplorerUtilityViewModel ViewModel { get; }
    }
}