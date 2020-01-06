using RayCarrot.IO;
using RayCarrot.Rayman;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// The Rayman 3 CNT explorer utility
    /// </summary>
    public class R3CNTExplorerUtility : BaseOpenSpaceCNTExplorerUtility
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public R3CNTExplorerUtility()
        {
            // Get the game install directory
            var installDir = Games.Rayman3.GetInstallDir();

            // Set properties
            var archiveFiles = new FileSystemPath[]
            {
                installDir + "Gamedatabin" + "tex32_1.cnt",
                installDir + "Gamedatabin" + "tex32_2.cnt",
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