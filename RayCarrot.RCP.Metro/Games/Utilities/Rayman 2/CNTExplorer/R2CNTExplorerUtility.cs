using RayCarrot.IO;
using RayCarrot.Rayman;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// The Rayman 2 CNT explorer utility
    /// </summary>
    public class R2CNTExplorerUtility : BaseOpenSpaceCNTExplorerUtility
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public R2CNTExplorerUtility()
        {
            // Get the game install directory
            var installDir = Games.Rayman2.GetInstallDir();

            // Set properties
            var archiveFiles = new FileSystemPath[]
            {
                installDir + "Data" + "Textures.cnt",
                installDir + "Data" + "Vignette.cnt",
            };

            ViewModel = new BaseOpenSpaceCNTExplorerUtilityViewModel(OpenSpaceGameMode.Rayman2PC, archiveFiles);
        }

        /// <summary>
        /// The view model
        /// </summary>
        public override BaseOpenSpaceCNTExplorerUtilityViewModel ViewModel { get; }
    }
}