using RayCarrot.Rayman;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// The Rayman Educational archive explorer utility
    /// </summary>
    public class EDUArchiveExplorerUtility : BaseRay1ExplorerUtility
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public EDUArchiveExplorerUtility()
        {
            // Get the game install directory
            var installDir = Games.EducationalDos.GetInstallDir();

            ViewModel = new BaseRay1ArchiveExplorerUtilityViewModel(GameMode.RayEduPC, GetArchiveFiles(installDir));
        }

        /// <summary>
        /// The view model
        /// </summary>
        public override BaseRay1ArchiveExplorerUtilityViewModel ViewModel { get; }
    }
}