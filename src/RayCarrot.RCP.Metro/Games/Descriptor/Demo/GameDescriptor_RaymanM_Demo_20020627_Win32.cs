using System;
using System.Collections.Generic;
using BinarySerializer.OpenSpace;
using RayCarrot.RCP.Metro.Archive;
using RayCarrot.RCP.Metro.Archive.CPA;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// The Rayman M Demo 2002/06/27 (Win32) game descriptor
/// </summary>
public sealed class GameDescriptor_RaymanM_Demo_20020627_Win32 : Win32GameDescriptor
{
    #region Protected Properties

    protected override string IconName => "RaymanMDemo";

    #endregion

    #region Public Properties

    public override string Id => "RaymanM_Demo_20020627_Win32";
    public override Game Game => Game.RaymanMArena;
    public override GameCategory Category => GameCategory.Demo;
    public override bool IsDemo => true;
    public override Games? LegacyGame => Games.Demo_RaymanM;

    public override string DisplayName => "Rayman M Demo (2002/06/27)";
    public override string BackupName => "Rayman M Demo";
    public override string DefaultFileName => "RaymanM.exe";

    public override IEnumerable<string> DialogGroupNames => new[] { UbiIniFileGroupName };

    public override bool CanBeDownloaded => true;
    public override IList<Uri> DownloadURLs => new Uri[]
    {
        new(AppURLs.Games_RMDemo_Url),
    };

    public override bool HasArchives => true;

    #endregion

    #region Public Methods

    public override GameOptionsDialog_ConfigPageViewModel GetConfigPageViewModel(GameInstallation gameInstallation) => 
        new Config_RaymanMDemo_ViewModel(gameInstallation);

    public override GameProgressionManager GetGameProgressionManager(GameInstallation gameInstallation) => 
        new GameProgressionManager_RaymanMArena(gameInstallation, true);

    public override IEnumerable<GameFileLink> GetGameFileLinks(GameInstallation gameInstallation) => new GameFileLink[]
    {
        new(Resources.GameLink_Setup, gameInstallation.InstallLocation + "RM_Setup_DX8.exe")
    };

    public override IArchiveDataManager GetArchiveDataManager(GameInstallation? gameInstallation) => 
        new CPACntArchiveDataManager(
            settings: new OpenSpaceSettings(EngineVersion.RaymanM, BinarySerializer.OpenSpace.Platform.PC), 
            gameInstallation: gameInstallation,
            cpaTextureSyncData: null);

    public override IEnumerable<string> GetArchiveFilePaths(GameInstallation? gameInstallation) => new[]
    {
        @"FishBin\tex32.cnt",
        @"FishBin\vignette.cnt",
        @"MenuBin\tex32.cnt",
        @"MenuBin\vignette.cnt",
        @"TribeBin\tex32.cnt",
        @"TribeBin\vignette.cnt",
    };

    #endregion
}