using RayCarrot.IO;
using RayCarrot.Rayman;
using RayCarrot.Rayman.OpenSpace;
using System.Collections.Generic;
using System.IO;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// The Rayman M game info
    /// </summary>
    public sealed class GameInfo_RaymanM : GameInfo
    {
        #region Protected Override Properties

        /// <summary>
        /// Gets the backup directories for the game
        /// </summary>
        protected override IList<GameBackups_Directory> GetBackupDirectories => new List<GameBackups_Directory>()
        {
            new GameBackups_Directory(Game.GetInstallDir() + "Menu" + "SaveGame", SearchOption.TopDirectoryOnly, "*", "0", 0)
        };

        #endregion

        #region Public Override Properties

        /// <summary>
        /// The game
        /// </summary>
        public override Games Game => Games.RaymanM;

        /// <summary>
        /// The category for the game
        /// </summary>
        public override GameCategory Category => GameCategory.Rayman;

        /// <summary>
        /// The game display name
        /// </summary>
        public override string DisplayName => "Rayman M";

        /// <summary>
        /// The game backup name
        /// </summary>
        public override string BackupName => "Rayman M";

        /// <summary>
        /// Gets the launch name for the game
        /// </summary>
        public override string DefaultFileName => "RaymanM.exe";

        /// <summary>
        /// The config page view model, if any is available
        /// </summary>
        public override GameOptions_ConfigPageViewModel ConfigPageViewModel => new Config_RaymanM_ViewModel();

        /// <summary>
        /// The progression view model, if any is available
        /// </summary>
        public override GameProgression_BaseViewModel ProgressionViewModel => new GameProgression_RaymanM_ViewModel();

        /// <summary>
        /// Optional RayMap URL
        /// </summary>
        public override string RayMapURL => CommonUrls.GetRayMapGameURL("rm_pc", "rm_pc");

        /// <summary>
        /// Gets the file links for the game
        /// </summary>
        public override IList<GameFileLink> GetGameFileLinks => new GameFileLink[]
        {
            new GameFileLink(Resources.GameLink_Setup, Game.GetInstallDir() + "RM_Setup_DX8.exe")
        };

        /// <summary>
        /// The group names to use for the options, config and utility dialog
        /// </summary>
        public override IEnumerable<string> DialogGroupNames => new string[]
        {
            UbiIniFileGroupName
        };

        /// <summary>
        /// Indicates if the game can be installed from a disc in this program
        /// </summary>
        public override bool CanBeInstalledFromDisc => true;

        /// <summary>
        /// The .gif files to use during the game installation if installing from a disc
        /// </summary>
        public override string[] InstallerGifs
        {
            get
            {
                var basePath = $"{AppViewModel.WPFApplicationBasePath}Installer/InstallerGifs/";

                return new string[]
                {
                    basePath + "BAST.gif",
                    basePath + "CHASE.gif",
                    basePath + "GLOB.gif",
                    basePath + "RAY.gif"
                };
            }
        }

        /// <summary>
        /// Indicates if the game has archives which can be opened
        /// </summary>
        public override bool HasArchives => true;

        /// <summary>
        /// Gets the archive data manager for the game
        /// </summary>
        public override IArchiveDataManager GetArchiveDataManager => new OpenSpaceCntArchiveDataManager(OpenSpaceSettings.GetDefaultSettings(OpenSpaceGame.RaymanM, Platform.PC));

        /// <summary>
        /// Gets the archive file paths for the game
        /// </summary>
        /// <param name="installDir">The game's install directory</param>
        public override FileSystemPath[] GetArchiveFilePaths(FileSystemPath installDir) => new FileSystemPath[]
        {
            installDir + "FishBin" + "tex32.cnt",
            installDir + "FishBin" + "vignette.cnt",
            installDir + "MenuBin" + "tex32.cnt",
            installDir + "MenuBin" + "vignette.cnt",
            installDir + "TribeBin" + "tex32.cnt",
            installDir + "TribeBin" + "vignette.cnt",
        };

        #endregion
    }
}