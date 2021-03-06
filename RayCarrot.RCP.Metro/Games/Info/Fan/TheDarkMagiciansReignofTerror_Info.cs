﻿using System;
using System.Collections.Generic;
using System.IO;
using RayCarrot.IO;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// The Rayman The Dark Magician's Reign of Terror game info
    /// </summary>
    public sealed class TheDarkMagiciansReignofTerror_Info : RCPGameInfo
    {
        #region Protected Override Properties

        /// <summary>
        /// Gets the backup directories for the game
        /// </summary>
        protected override IList<BackupDir> GetBackupDirectories => new BackupDir[]
        {
            new BackupDir(Environment.SpecialFolder.LocalApplicationData.GetFolderPath() + "Rayman__Dark_Magician_s_reign_of_terror_", SearchOption.AllDirectories, "*", "0", 0),
        };

        #endregion

        #region Public Override Properties

        /// <summary>
        /// The game
        /// </summary>
        public override Games Game => Games.TheDarkMagiciansReignofTerror;

        /// <summary>
        /// The category for the game
        /// </summary>
        public override GameCategory Category => GameCategory.Fan;

        /// <summary>
        /// The game display name
        /// </summary>
        public override string DisplayName => "Rayman: The Dark Magician's Reign of Terror";

        /// <summary>
        /// The game backup name
        /// </summary>
        public override string BackupName => "Rayman The Dark Magicians Reign of Terror";

        /// <summary>
        /// Gets the launch name for the game
        /// </summary>
        public override string DefaultFileName => "Rayman! Dark Magician's reign of terror!.exe";

        /// <summary>
        /// Indicates if the game can be downloaded
        /// </summary>
        public override bool CanBeDownloaded => false;

        /// <summary>
        /// The directories to remove when uninstalling. This should not include the game install directory as that is included by default.
        /// </summary>
        public override IEnumerable<FileSystemPath> UninstallDirectories => new FileSystemPath[]
        {
            Environment.SpecialFolder.LocalApplicationData.GetFolderPath() + "Rayman__Dark_Magician_s_reign_of_terror_"
        };

        #endregion
    }
}