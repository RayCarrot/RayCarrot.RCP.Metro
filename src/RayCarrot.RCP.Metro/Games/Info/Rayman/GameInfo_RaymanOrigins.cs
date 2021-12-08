﻿#nullable disable
using RayCarrot.IO;
using RayCarrot.Rayman;
using RayCarrot.Rayman.UbiArt;
using System;
using System.Collections.Generic;
using System.IO;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// The Rayman Origins game info
/// </summary>
public sealed class GameInfo_RaymanOrigins : GameInfo
{
    #region Protected Override Properties

    /// <summary>
    /// Gets the backup directories for the game
    /// </summary>
    protected override IList<GameBackups_Directory> GetBackupDirectories => new GameBackups_Directory[]
    {
        new GameBackups_Directory(Environment.SpecialFolder.MyDocuments.GetFolderPath() + "My games" + "Rayman origins", SearchOption.AllDirectories, "*", "0", 0)
    };

    #endregion

    #region Public Override Properties

    /// <summary>
    /// The game
    /// </summary>
    public override Games Game => Games.RaymanOrigins;

    /// <summary>
    /// The category for the game
    /// </summary>
    public override GameCategory Category => GameCategory.Rayman;

    /// <summary>
    /// The game display name
    /// </summary>
    public override string DisplayName => "Rayman Origins";

    /// <summary>
    /// The game backup name
    /// </summary>
    public override string BackupName => "Rayman Origins";

    /// <summary>
    /// Gets the launch name for the game
    /// </summary>
    public override string DefaultFileName => "Rayman Origins.exe";

    /// <summary>
    /// The config page view model, if any is available
    /// </summary>
    public override GameOptionsDialog_ConfigPageViewModel ConfigPageViewModel => new Config_UbiArt_ViewModel(Game);

    /// <summary>
    /// The progression view model, if any is available
    /// </summary>
    public override GameProgression_BaseViewModel ProgressionViewModel => new GameProgression_Origins_ViewModel();

    public override IEnumerable<ProgressionGameViewModel> GetProgressionGameViewModels => new ProgressionGameViewModel_RaymanOrigins().Yield();

    /// <summary>
    /// Gets the file links for the game
    /// </summary>
    public override IList<GameFileLink> GetGameFileLinks => new GameFileLink[0];

    /// <summary>
    /// Indicates if the game has archives which can be opened
    /// </summary>
    public override bool HasArchives => true;

    /// <summary>
    /// Gets the archive data manager for the game
    /// </summary>
    public override IArchiveDataManager GetArchiveDataManager => new UbiArtIPKArchiveDataManager(new UbiArtIPKArchiveConfigViewModel(UbiArtSettings.GetDefaultSettings(UbiArtGame.RaymanOrigins, Platform.PC), UbiArtIPKArchiveConfigViewModel.FileCompressionMode.WasCompressed));

    /// <summary>
    /// Gets the archive file paths for the game
    /// </summary>
    /// <param name="installDir">The game's install directory</param>
    public override FileSystemPath[] GetArchiveFilePaths(FileSystemPath installDir) => new FileSystemPath[]
    {
        installDir + "GameData" + "bundle_PC.ipk",
    };

    #endregion
}