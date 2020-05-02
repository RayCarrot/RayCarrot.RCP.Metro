using RayCarrot.Extensions;
using RayCarrot.Rayman.OpenSpace;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// A view model for the CNT Archive Explorer utility
    /// </summary>
    public class CNTArchiveExplorerUtilityViewModel : BaseArchiveExplorerUtilityViewModel<OpenSpaceGameMode>
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public CNTArchiveExplorerUtilityViewModel()
        {
            GameModeSelection = new EnumSelectionViewModel<OpenSpaceGameMode>(OpenSpaceGameMode.Rayman2PC, new OpenSpaceGameMode[]
            {
                OpenSpaceGameMode.Rayman2PC,
                OpenSpaceGameMode.Rayman2PCDemo1,
                OpenSpaceGameMode.Rayman2PCDemo2,
                OpenSpaceGameMode.RaymanMPC,
                OpenSpaceGameMode.RaymanArenaPC,
                OpenSpaceGameMode.Rayman3PC,
                OpenSpaceGameMode.TonicTroublePC,
                OpenSpaceGameMode.TonicTroubleSEPC,
                OpenSpaceGameMode.DonaldDuckPC,
                OpenSpaceGameMode.PlaymobilHypePC,
            });
        }

        /// <summary>
        /// Gets a new archive explorer data manager
        /// </summary>
        protected override IArchiveExplorerDataManager GetArchiveExplorerDataManager
        {
            get
            {
                // Get the settings
                var attr = GameModeSelection.SelectedValue.GetAttribute<OpenSpaceGameModeInfoAttribute>();
                var gameSettings = OpenSpaceSettings.GetDefaultSettings(attr.Game, attr.Platform);

                return new OpenSpaceCntArchiveExplorerDataManager(gameSettings);
            }
        }

        /// <summary>
        /// Gets a new archive creator data manager
        /// </summary>
        protected override IArchiveCreatorDataManager GetArchiveCreatorDataManager
        {
            get
            {
                // Get the settings
                var attr = GameModeSelection.SelectedValue.GetAttribute<OpenSpaceGameModeInfoAttribute>();
                var gameSettings = OpenSpaceSettings.GetDefaultSettings(attr.Game, attr.Platform);

                return new OpenSpaceCntArchiveCreatorDataManager(gameSettings);
            }
        }

        /// <summary>
        /// The file extension for the archive
        /// </summary>
        public override string ArchiveFileExtension => ".cnt";

        /// <summary>
        /// The game mode selection
        /// </summary>
        public override EnumSelectionViewModel<OpenSpaceGameMode> GameModeSelection { get; }
    }
}