using RayCarrot.IO;
using System;
using System.Collections.Generic;
using System.IO;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// The Rayman Raving Rabbids game info
    /// </summary>
    public sealed class GameInfo_RaymanRavingRabbids : GameInfo
    {
        #region Protected Override Properties

        /// <summary>
        /// Gets the backup directories for the game
        /// </summary>
        protected override IList<GameBackups_Directory> GetBackupDirectories
        {
            get
            {
                // By default the save file is located in the game directory, but if not running as admin it might get redirected to VirtualStore
                var dirs = new List<GameBackups_Directory>()
                {
                    new GameBackups_Directory(Game.GetInstallDir(), SearchOption.TopDirectoryOnly, "*.sav", "0", 0),
                    new GameBackups_Directory(Environment.SpecialFolder.LocalApplicationData.GetFolderPath() + "VirtualStore" + Game.GetInstallDir().RemoveRoot(), SearchOption.TopDirectoryOnly, "*.sav", "0", 0)
                };

                return dirs;
            }
        }

        #endregion

        #region Public Override Properties

        /// <summary>
        /// The game
        /// </summary>
        public override Games Game => Games.RaymanRavingRabbids;

        /// <summary>
        /// The category for the game
        /// </summary>
        public override GameCategory Category => GameCategory.Rabbids;

        /// <summary>
        /// The game display name
        /// </summary>
        public override string DisplayName => "Rayman Raving Rabbids";

        /// <summary>
        /// The game backup name
        /// </summary>
        public override string BackupName => "Rayman Raving Rabbids";

        /// <summary>
        /// Gets the launch name for the game
        /// </summary>
        public override string DefaultFileName => "CheckApplication.exe";

        /// <summary>
        /// The config page view model, if any is available
        /// </summary>
        public override GameOptionsDialog_ConfigPageViewModel ConfigPageViewModel => new Config_RaymanRavingRabbids_ViewModel();

        /// <summary>
        /// Gets the file links for the game
        /// </summary>
        public override IList<GameFileLink> GetGameFileLinks => new GameFileLink[]
        {
            new GameFileLink(Resources.GameLink_Setup, Game.GetInstallDir() + "SettingsApplication.exe")
        };

        #endregion
    }
}