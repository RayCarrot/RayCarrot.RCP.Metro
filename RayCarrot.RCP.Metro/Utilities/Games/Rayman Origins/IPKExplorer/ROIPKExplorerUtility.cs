using RayCarrot.IO;
using RayCarrot.Rayman;
using RayCarrot.Rayman.UbiArt;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// The Rayman Origins IPK explorer utility
    /// </summary>
    public class ROIPKExplorerUtility : UbiArtIPKExplorerUtility
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public ROIPKExplorerUtility()
        {
            // Get the game install directory
            var installDir = Games.RaymanOrigins.GetInstallDir();

            // Set properties
            var archiveFiles = new FileSystemPath[]
            {
                installDir + "GameData" + "bundle_PC.ipk",
            };

            ViewModel = new BaseUbiArtIPKExplorerUtilityViewModel(UbiArtGameMode.RaymanOriginsPC, archiveFiles);
        }

        /// <summary>
        /// The view model
        /// </summary>
        public override BaseUbiArtIPKExplorerUtilityViewModel ViewModel { get; }
    }
}