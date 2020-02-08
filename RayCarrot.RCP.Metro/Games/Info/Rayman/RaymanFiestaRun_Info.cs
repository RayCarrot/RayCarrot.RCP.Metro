using RayCarrot.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// The Rayman Fiesta Run game info
    /// </summary>
    public sealed class RaymanFiestaRun_Info : RCPGameInfo
    {
        #region Public Override Properties

        /// <summary>
        /// The game
        /// </summary>
        public override Games Game => Games.RaymanFiestaRun;

        /// <summary>
        /// The category for the game
        /// </summary>
        public override GameCategory Category => GameCategory.Rayman;

        /// <summary>
        /// The game display name
        /// </summary>
        public override string DisplayName => "Rayman Fiesta Run";

        /// <summary>
        /// The game backup name
        /// </summary>
        public override string BackupName => $"Rayman Fiesta Run ({RCFRCP.Data.FiestaRunVersion})";

        /// <summary>
        /// Gets the default file name for launching the game, if available
        /// </summary>
        public override string DefaultFileName => GetFiestaRunFileName(RCFRCP.Data.FiestaRunVersion);

        /// <summary>
        /// The options UI, if any is available
        /// </summary>
        public override FrameworkElement OptionsUI => new FiestaRunOptions();

        /// <summary>
        /// The config UI, if any is available
        /// </summary>
        public override FrameworkElement ConfigUI => new RaymanFiestaRunConfig();

        /// <summary>
        /// Gets the file links for the game
        /// </summary>
        public override IList<GameFileLink> GetGameFileLinks => new GameFileLink[0];

        /// <summary>
        /// Gets the backup directories for the game
        /// </summary>
        public override IList<IBackupInfo> GetBackupInfos
        {
            get
            {
                var manager = Game.GetManager<RaymanFiestaRun_WinStore>(GameType.WinStore);

                // Get every installed version
                var versions = EnumHelpers.GetValues<FiestaRunEdition>().Where(x => manager.GetGamePackage(manager.GetFiestaRunPackageName(x)) != null);

                // Return a backup info for each version
                return versions.Select(x =>
                {
                    var backupName = $"Rayman Fiesta Run ({x})";

                    return new BaseBackupInfo(backupName, RCPWinStoreGame.GetWinStoreBackupDirs(manager.GetFiestaRunFullPackageName(x)), $"{Games.RaymanFiestaRun.GetGameInfo().DisplayName} {manager.GetFiestaRunEditionDisplayName(x)}") as IBackupInfo;
                }).ToArray();
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Gets the Fiesta Run default file name based on version
        /// </summary>
        /// <param name="version">The version</param>
        /// <returns>The file name</returns>
        public string GetFiestaRunFileName(FiestaRunEdition version)
        {
            return version switch
            {
                FiestaRunEdition.Default => "RFR_WinRT.exe",
                FiestaRunEdition.Preload => "RFR_WinRT_OEM.exe",
                FiestaRunEdition.Win10 => "RFRXAML.exe",

                _ => throw new ArgumentOutOfRangeException()
            };
        }

        #endregion
    }
}