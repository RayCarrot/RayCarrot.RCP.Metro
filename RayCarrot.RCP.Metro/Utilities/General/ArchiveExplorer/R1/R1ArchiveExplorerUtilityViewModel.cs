﻿using RayCarrot.Common;
using RayCarrot.Rayman;
using RayCarrot.Rayman.Ray1;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// A view model for the R1 Archive Explorer utility
    /// </summary>
    public class R1ArchiveExplorerUtilityViewModel : BaseArchiveExplorerUtilityViewModel<GameMode>
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public R1ArchiveExplorerUtilityViewModel()
        {
            GameModeSelection = new EnumSelectionViewModel<GameMode>(GameMode.RayKitPC, new GameMode[]
            {
                GameMode.RayEduPC,
                GameMode.RayKitPC,
                GameMode.RayFanPC,
                GameMode.Ray60nPC,
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
                var attr = GameModeSelection.SelectedValue.GetAttribute<Ray1GameModeInfoAttribute>();
                var gameSettings = Ray1Settings.GetDefaultSettings(attr.Game, attr.Platform);

                return new Ray1PCArchiveDataManager(gameSettings);
            }
        }

        /// <summary>
        /// The file extension for the archive
        /// </summary>
        public override string ArchiveFileExtension => ".dat";

        /// <summary>
        /// The game mode selection
        /// </summary>
        public override EnumSelectionViewModel<GameMode> GameModeSelection { get; }
    }
}