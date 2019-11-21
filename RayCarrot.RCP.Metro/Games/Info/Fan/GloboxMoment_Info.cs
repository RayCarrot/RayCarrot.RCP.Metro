using System;
using System.Collections.Generic;
using System.IO;
using System.Security;
using System.Threading.Tasks;
using System.Windows;
using RayCarrot.IO;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// The Globox Moment game info
    /// </summary>
    public sealed class GloboxMoment_Info : RCPGameInfo
    {
        #region Protected Override Properties

        /// <summary>
        /// Gets the backup directories for the game
        /// </summary>
        protected override IList<BackupDir> GetBackupDirectories => new BackupDir[]
        {
            new BackupDir(Environment.SpecialFolder.ApplicationData.GetFolderPath() + "MMFApplications", SearchOption.TopDirectoryOnly, "globoxmoment.ini", "0", 0),
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
        public override bool CanBeDownloaded => true;

        /// <summary>
        /// The download URLs for the game if it can be downloaded. All sources must be compressed.
        /// </summary>
        public override IList<Uri> DownloadURLs => new Uri[]
        {
            new Uri(CommonUrls.Games_GloboxMoment_Url),
        };

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