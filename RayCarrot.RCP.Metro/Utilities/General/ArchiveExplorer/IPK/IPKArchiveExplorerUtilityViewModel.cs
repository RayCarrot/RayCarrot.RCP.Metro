using RayCarrot.Common;
using RayCarrot.Rayman;
using RayCarrot.Rayman.UbiArt;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// A view model for the IPK Archive Explorer utility
    /// </summary>
    public class IPKArchiveExplorerUtilityViewModel : BaseArchiveExplorerUtilityViewModel<GameMode>
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public IPKArchiveExplorerUtilityViewModel()
        {
            GameModeSelection = new EnumSelectionViewModel<GameMode>(GameMode.RaymanOriginsPC, new GameMode[]
            {
                GameMode.RaymanOriginsPC,
                GameMode.RaymanOriginsPS3,
                GameMode.RaymanOriginsWii,
                GameMode.RaymanOriginsPSVita,
                GameMode.RaymanLegendsPC,
                GameMode.RaymanLegendsWiiU,
                GameMode.RaymanLegendsPSVita,
                GameMode.RaymanLegendsPS4,
                GameMode.RaymanLegendsSwitch,
                GameMode.RaymanAdventuresAndroid,
                GameMode.RaymanAdventuresiOS,
                GameMode.RaymanMiniMac,
                GameMode.JustDance2017WiiU,
                GameMode.ChildOfLightPC,
                GameMode.ChildOfLightPSVita,
                GameMode.ValiantHeartsAndroid,
                GameMode.GravityFalls3DS,
            });
        }

        /// <summary>
        /// Gets a new archive explorer data manager
        /// </summary>
        protected override IArchiveExplorerDataManager GetArchiveExplorerDataManager
        {
            get
            {
                var attr = GameModeSelection.SelectedValue.GetAttribute<UbiArtGameModeInfoAttribute>();
                var settings = UbiArtSettings.GetDefaultSettings(attr.Game, attr.Platform);

                return new UbiArtIPKArchiveExplorerDataManager(settings);
            }
        }

        /// <summary>
        /// Gets a new archive creator data manager
        /// </summary>
        protected override IArchiveCreatorDataManager GetArchiveCreatorDataManager
        {
            get
            {
                var attr = GameModeSelection.SelectedValue.GetAttribute<UbiArtGameModeInfoAttribute>();
                var settings = UbiArtSettings.GetDefaultSettings(attr.Game, attr.Platform);

                return new UbiArtIPKArchiveCreatorDataManager(settings);
            }
        }

        /// <summary>
        /// The file extension for the archive
        /// </summary>
        public override string ArchiveFileExtension => ".ipk";

        /// <summary>
        /// The game mode selection
        /// </summary>
        public override EnumSelectionViewModel<GameMode> GameModeSelection { get; }
    }
}