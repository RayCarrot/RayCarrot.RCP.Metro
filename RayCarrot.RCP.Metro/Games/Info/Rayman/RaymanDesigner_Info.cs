﻿using System.Collections.Generic;
using System.IO;
using System.Windows;
using RayCarrot.IO;
using RayCarrot.Rayman;
using RayCarrot.Rayman.Ray1;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// The Rayman Designer game info
    /// </summary>
    public sealed class RaymanDesigner_Info : RCPGameInfo
    {
        #region Protected Override Properties

        /// <summary>
        /// Gets the backup directories for the game
        /// </summary>
        protected override IList<BackupDir> GetBackupDirectories => new BackupDir[]
        {
            new BackupDir(Game.GetInstallDir(), SearchOption.TopDirectoryOnly, "*.cfg", "0", 0),
            new BackupDir(Game.GetInstallDir() + "PCMAP", SearchOption.TopDirectoryOnly, "*.sct", "1", 0),
            //
            // Note:
            // This will backup the pre-installed maps and the world files as well. This is due to how the backup manager works.
            // In the future I might make a separate manager for the maps again, in which case the search pattern "MAPS???" should get the
            // correct mapper directories withing each world directory
            //
            new BackupDir(Game.GetInstallDir() + "CAKE", SearchOption.AllDirectories, "*", "Mapper0", 0),
            new BackupDir(Game.GetInstallDir() + "CAVE", SearchOption.AllDirectories, "*", "Mapper1", 0),
            new BackupDir(Game.GetInstallDir() + "IMAGE", SearchOption.AllDirectories, "*", "Mapper2", 0),
            new BackupDir(Game.GetInstallDir() + "JUNGLE", SearchOption.AllDirectories, "*", "Mapper3", 0),
            new BackupDir(Game.GetInstallDir() + "MOUNTAIN", SearchOption.AllDirectories, "*", "Mapper4", 0),
            new BackupDir(Game.GetInstallDir() + "MUSIC", SearchOption.AllDirectories, "*", "Mapper5", 0),
        };

        #endregion

        #region Public Override Properties

        /// <summary>
        /// The game
        /// </summary>
        public override Games Game => Games.RaymanDesigner;

        /// <summary>
        /// The category for the game
        /// </summary>
        public override GameCategory Category => GameCategory.Rayman;

        /// <summary>
        /// The game display name
        /// </summary>
        public override string DisplayName => "Rayman Designer";

        /// <summary>
        /// The game backup name
        /// </summary>
        public override string BackupName => "Rayman Designer";

        /// <summary>
        /// Gets the launch name for the game
        /// </summary>
        public override string DefaultFileName => "RAYKIT.bat";

        /// <summary>
        /// The config page view model, if any is available
        /// </summary>
        public override GameOptions_ConfigPageViewModel ConfigPageViewModel => new RaymanDesignerConfigViewModel(Game);

        /// <summary>
        /// The options UI, if any is available
        /// </summary>
        public override FrameworkElement OptionsUI => new DOSBoxOptions(Game);

        /// <summary>
        /// The progression view model, if any is available
        /// </summary>
        public override BaseProgressionViewModel ProgressionViewModel => new RaymanDesignerProgressionViewModel(Game);

        /// <summary>
        /// Optional RayMap URL
        /// </summary>
        public override string RayMapURL => CommonUrls.GetRay1MapGameURL("RaymanDesignerPC", "r1/pc_kit");

        /// <summary>
        /// Gets the file links for the game
        /// </summary>
        public override IList<GameFileLink> GetGameFileLinks => new GameFileLink[]
        {
            new GameFileLink(Resources.GameLink_RDMapper, Game.GetInstallDir() + "MAPPER.EXE")
        };

        /// <summary>
        /// Indicates if the game has archives which can be opened
        /// </summary>
        public override bool HasArchives => true;

        /// <summary>
        /// Gets the archive data manager for the game
        /// </summary>
        public override IArchiveDataManager GetArchiveDataManager => new Ray1PCArchiveDataManager(new Ray1PCArchiveConfigViewModel(Ray1Settings.GetDefaultSettings(Ray1Game.RayKit, Platform.PC)));

        /// <summary>
        /// Gets the archive file paths for the game
        /// </summary>
        /// <param name="installDir">The game's install directory</param>
        public override FileSystemPath[] GetArchiveFilePaths(FileSystemPath installDir) => Ray1PCArchiveDataManager.GetArchiveFiles(installDir);

        /// <summary>
        /// An optional emulator to use for the game
        /// </summary>
        public override Emulator Emulator => new DOSBoxEmulator(Game, GameType.DosBox);

        #endregion
    }
}