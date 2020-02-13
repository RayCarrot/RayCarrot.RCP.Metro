using RayCarrot.Rayman;
using RayCarrot.Rayman.UbiArt;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// A view model for the IPK Archive Explorer utility
    /// </summary>
    public class IPKArchiveExplorerUtilityViewModel : BaseArchiveExplorerUtilityViewModel<UbiArtGameMode>
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public IPKArchiveExplorerUtilityViewModel()
        {
            GameModeSelection = new EnumSelectionViewModel<UbiArtGameMode>(UbiArtGameMode.RaymanOriginsPC, new UbiArtGameMode[]
            {
                UbiArtGameMode.RaymanOriginsPC,
                UbiArtGameMode.RaymanOriginsPS3,
                UbiArtGameMode.RaymanOriginsWii,
                UbiArtGameMode.RaymanOriginsPSVita,
                UbiArtGameMode.RaymanLegendsPC,
                UbiArtGameMode.RaymanLegendsWiiU,
                UbiArtGameMode.RaymanLegendsPSVita,
                UbiArtGameMode.RaymanLegendsPS4,
                UbiArtGameMode.RaymanLegendsSwitch,
                UbiArtGameMode.RaymanAdventuresAndroid,
                UbiArtGameMode.RaymanAdventuresiOS,
                UbiArtGameMode.RaymanMiniMac,
                UbiArtGameMode.JustDance2017WiiU,
                UbiArtGameMode.ChildOfLightPC,
                UbiArtGameMode.ChildOfLightPSVita,
                UbiArtGameMode.ValiantHeartsAndroid,
                UbiArtGameMode.GravityFalls3DS,
            });
        }

        /// <summary>
        /// Gets a new archive data manager
        /// </summary>
        protected override IArchiveDataManager GetArchiveDataManager => new UbiArtIPKArchiveDataManager(GameModeSelection.SelectedValue.GetSettings());

        /// <summary>
        /// The file extension for the archive
        /// </summary>
        public override string ArchiveFileExtension => ".ipk";

        /// <summary>
        /// The game mode selection
        /// </summary>
        public override EnumSelectionViewModel<UbiArtGameMode> GameModeSelection { get; }
    }
}