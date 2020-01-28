using RayCarrot.IO;
using RayCarrot.Rayman;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// The Rayman Legends IPK explorer utility
    /// </summary>
    public class RLIPKExplorerUtility : UbiArtIPKExplorerUtility
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public RLIPKExplorerUtility()
        {
            // Get the game install directory
            var installDir = Games.RaymanLegends.GetInstallDir();

            // Set properties
            var archiveFiles = new FileSystemPath[]
            {
                installDir + "Bundle_PC.ipk",
                installDir + "persistentLoading_PC.ipk",
            };

            ViewModel = new BaseUbiArtIPKExplorerUtilityViewModel(UbiArtGameMode.RaymanLegendsPC, archiveFiles);
        }

        /// <summary>
        /// The view model
        /// </summary>
        public override BaseUbiArtIPKExplorerUtilityViewModel ViewModel { get; }
    }
}