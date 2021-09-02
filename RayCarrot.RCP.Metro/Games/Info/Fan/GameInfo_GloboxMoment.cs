using System;
using System.Collections.Generic;
using System.IO;
using RayCarrot.IO;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// The Globox Moment game info
    /// </summary>
    public sealed class GameInfo_GloboxMoment : GameInfo
    {
        #region Protected Override Properties

        /// <summary>
        /// Gets the backup directories for the game
        /// </summary>
        protected override IList<GameBackups_Directory> GetBackupDirectories => new GameBackups_Directory[]
        {
            new GameBackups_Directory(Environment.SpecialFolder.ApplicationData.GetFolderPath() + "MMFApplications", SearchOption.TopDirectoryOnly, "globoxmoment.ini", "0", 0),
        };

        #endregion

        #region Public Override Properties

        /// <summary>
        /// The game
        /// </summary>
        public override Games Game => Games.GloboxMoment;

        /// <summary>
        /// The category for the game
        /// </summary>
        public override GameCategory Category => GameCategory.Fan;

        /// <summary>
        /// The game display name
        /// </summary>
        public override string DisplayName => "Globox Moment";

        /// <summary>
        /// The game backup name
        /// </summary>
        public override string BackupName => "Globox Moment";

        /// <summary>
        /// Gets the launch name for the game
        /// </summary>
        public override string DefaultFileName => "Globox Moment.exe";

        /// <summary>
        /// Indicates if the game can be downloaded
        /// </summary>
        public override bool CanBeDownloaded => false;

        /// <summary>
        /// The files to remove when uninstalling
        /// </summary>
        public override IEnumerable<FileSystemPath> UninstallFiles => new FileSystemPath[]
        {
            Environment.SpecialFolder.ApplicationData.GetFolderPath() + "MMFApplications" + "globoxmoment.ini"
        };

        #endregion
    }
}