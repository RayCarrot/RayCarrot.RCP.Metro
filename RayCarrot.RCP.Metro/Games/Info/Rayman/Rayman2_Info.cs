using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using RayCarrot.RCP.Core;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// The Rayman 2 game info
    /// </summary>
    public sealed class Rayman2_Info : RCPGameInfo
    {
        #region Protected Override Properties

        /// <summary>
        /// Gets the backup directories for the game
        /// </summary>
        protected override IList<BackupDir> GetBackupDirectories => new BackupDir[]
        {
            new BackupDir(Game.GetInstallDir() + "Data" + "SaveGame", SearchOption.AllDirectories, "*", "0", 0),
            new BackupDir(Game.GetInstallDir() + "Data" + "Options", SearchOption.AllDirectories, "*", "1", 0)
        };

        #endregion

        #region Public Override Properties

        /// <summary>
        /// The game
        /// </summary>
        public override Games Game => Games.Rayman2;

        /// <summary>
        /// The category for the game
        /// </summary>
        public override GameCategory Category => GameCategory.Rayman;

        /// <summary>
        /// The game display name
        /// </summary>
        public override string DisplayName => "Rayman 2";

        /// <summary>
        /// The game backup name
        /// </summary>
        public override string BackupName => "Rayman 2";

        /// <summary>
        /// Gets the launch name for the game
        /// </summary>
        public override string DefaultFileName => "Rayman2.exe";

        /// <summary>
        /// The config UI, if any is available
        /// </summary>
        public override FrameworkElement ConfigUI => new Rayman2Config();

        /// <summary>
        /// Gets the file links for the game
        /// </summary>
        public override IList<GameFileLink> GetGameFileLinks => new GameFileLink[]
        {
            new GameFileLink(Resources.GameLink_Setup, Game.GetInstallDir() + "GXSetup.exe"),
            new GameFileLink(Resources.GameLink_R2nGlide, Game.GetInstallDir() + "nglide_config.exe"),
            new GameFileLink(Resources.GameLink_R2dgVoodoo, Game.GetInstallDir() + "dgVoodooCpl.exe")
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
                var basePath = $"{AppViewModel.ApplicationBasePath}Installer/InstallerGifs/";

                return new string[]
                {
                    basePath + "ASTRO.gif",
                    basePath + "BAST.gif",
                    basePath + "CASK.gif",
                    basePath + "CHASE.gif",
                    basePath + "GLOB.gif",
                    basePath + "SKI.gif",
                    basePath + "WHALE.gif"
                };
            }
        }

        #endregion

        #region Public Override Methods

        /// <summary>
        /// Gets the applied utilities for the specified game
        /// </summary>
        /// <returns>The applied utilities</returns>
        public override async Task<IList<string>> GetAppliedUtilitiesAsync()
        {
            // Create the output
            var output = new List<string>();

            if (await Rayman2ConfigViewModel.GetIsWidescreenHackAppliedAsync() == true)
                output.Add(Resources.Config_WidescreenSupport);

            var dinput = Rayman2ConfigViewModel.GetCurrentDinput();

            if (dinput == Rayman2ConfigViewModel.R2Dinput.Controller)
                output.Add(Resources.Config_UseController);

            if (dinput == Rayman2ConfigViewModel.R2Dinput.Mapping)
                output.Add(Resources.Config_ButtonMapping);

            // Get other utilities
            output.AddRange(await base.GetAppliedUtilitiesAsync());

            return output;
        }

        #endregion
    }
}