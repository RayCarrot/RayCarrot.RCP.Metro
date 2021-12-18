#nullable disable
using System;
using System.Collections.Generic;
using RayCarrot.IO;
using RayCarrot.Rayman;
using RayCarrot.Rayman.OpenSpace;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// The Rayman M Demo game info
/// </summary>
public sealed class GameInfo_RaymanMDemo : GameInfo
{
    #region Protected Override Properties

    protected override string IconName => $"RaymanMDemo";

    #endregion

    #region Public Override Properties

    /// <summary>
    /// The game
    /// </summary>
    public override Games Game => Games.Demo_RaymanM;

    /// <summary>
    /// The category for the game
    /// </summary>
    public override GameCategory Category => GameCategory.Demo;

    /// <summary>
    /// The game display name
    /// </summary>
    public override string DisplayName => "Rayman M Demo (2002/06/27)";

    /// <summary>
    /// The game backup name
    /// </summary>
    public override string BackupName => "Rayman M Demo";

    /// <summary>
    /// Gets the launch name for the game
    /// </summary>
    public override string DefaultFileName => "RaymanM.exe";

    /// <summary>
    /// Indicates if the game is a demo
    /// </summary>
    public override bool IsDemo => true;

    /// <summary>
    /// Indicates if the game can be downloaded
    /// </summary>
    public override bool CanBeDownloaded => true;

    /// <summary>
    /// The download URLs for the game if it can be downloaded. All sources must be compressed.
    /// </summary>
    public override IList<Uri> DownloadURLs => new Uri[]
    {
        new Uri(AppURLs.Games_RMDemo_Url),
    };

    /// <summary>
    /// The config page view model, if any is available
    /// </summary>
    public override GameOptionsDialog_ConfigPageViewModel ConfigPageViewModel => new Config_RaymanMDemo_ViewModel();

    public override IEnumerable<ProgressionGameViewModel> GetProgressionGameViewModels => new ProgressionGameViewModel_RaymanMArena(Game).Yield();

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