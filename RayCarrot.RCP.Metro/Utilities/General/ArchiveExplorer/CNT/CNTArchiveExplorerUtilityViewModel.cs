using RayCarrot.Common;
using RayCarrot.Rayman;
using RayCarrot.Rayman.OpenSpace;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// A view model for the CNT Archive Explorer utility
    /// </summary>
    public class CNTArchiveExplorerUtilityViewModel : BaseArchiveExplorerUtilityViewModel<GameMode>
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public CNTArchiveExplorerUtilityViewModel()
        {
            GameModeSelection = new EnumSelectionViewModel<GameMode>(GameMode.Rayman2PC, new GameMode[]
            {
                GameMode.Rayman2PC,
                GameMode.Rayman2PCDemo1,
                GameMode.Rayman2PCDemo2,
                GameMode.RaymanMPC,
                GameMode.RaymanArenaPC,
                GameMode.Rayman3PC,
                GameMode.TonicTroublePC,
                GameMode.TonicTroubleSEPC,
                GameMode.DonaldDuckPC,
                GameMode.PlaymobilHypePC,
            });
        }

        /// <summary>
        /// Gets a new archive explorer data manager
        /// </summary>
        protected override IArchiveDataManager GetArchiveDataManager
        {
            get
            {
                // Get the settings
                var attr = GameModeSelection.SelectedValue.GetAttribute<OpenSpaceGameModeInfoAttribute>();
                var gameSettings = OpenSpaceSettings.GetDefaultSettings(attr.Game, attr.Platform);

                return new OpenSpaceCntArchiveDataManager(gameSettings);
            }
        }

        /// <summary>
        /// The file extension for the archive
        /// </summary>
        public override string ArchiveFileExtension => ".cnt";

        /// <summary>
        /// The game mode selection
        /// </summary>
        public override EnumSelectionViewModel<GameMode> GameModeSelection { get; }
    }
}