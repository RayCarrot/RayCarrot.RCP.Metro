using System.Collections.Generic;
using BinarySerializer.OpenSpace;
using RayCarrot.RCP.Metro.Archive;
using RayCarrot.RCP.Metro.Archive.CPA;
using RayCarrot.RCP.Metro.Ini;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// The Rayman Arena (Win32) game descriptor
/// </summary>
public sealed class GameDescriptor_RaymanArena_Win32 : Win32GameDescriptor
{
    #region Public Properties

    public override string Id => "RaymanArena_Win32";
    public override Game Game => Game.RaymanMArena;
    public override GameCategory Category => GameCategory.Rayman;
    public override Games? LegacyGame => Games.RaymanArena;

    public override string DisplayName => "Rayman Arena";
    public override string BackupName => "Rayman Arena";
    public override string DefaultFileName => "R_Arena.exe";

    public override string RayMapURL => AppURLs.GetRayMapGameURL("ra_pc", "ra_pc");

    public override IEnumerable<string> DialogGroupNames => new[] { UbiIniFileGroupName };

    public override bool HasGameInstaller => true;
    public override bool HasArchives => true;

    #endregion

    #region Public Methods

    public override GameOptionsDialog_ConfigPageViewModel GetConfigPageViewModel(GameInstallation gameInstallation) => 
        new Config_RaymanArena_ViewModel(gameInstallation);

    public override IEnumerable<ProgressionGameViewModel> GetProgressionGameViewModels(GameInstallation gameInstallation) => 
        new ProgressionGameViewModel_RaymanMArena(gameInstallation, false).Yield();

    public override IEnumerable<GameFileLink> GetGameFileLinks(GameInstallation gameInstallation) => new GameFileLink[]
    {
        new(Resources.GameLink_Setup, gameInstallation.InstallLocation + "RM_Setup_DX8.exe")
    };

    public override IArchiveDataManager GetArchiveDataManager(GameInstallation? gameInstallation) => 
        new CPACntArchiveDataManager(
            settings: new OpenSpaceSettings(EngineVersion.RaymanM, BinarySerializer.OpenSpace.Platform.PC), 
            gameInstallation: gameInstallation, 
            cpaTextureSyncData: CPATextureSyncData.FromGameMode(CPAGameMode.RaymanArena_PC));

    public override IEnumerable<string> GetArchiveFilePaths(GameInstallation? gameInstallation) => new[]
    {
        @"MenuBin\tex32.cnt",
        @"MenuBin\vignette.cnt",
        @"MenuBin\Sound.cnt",

        @"FishBin\tex32.cnt",
        @"FishBin\vignette.cnt",
        @"FishBin\Sound.cnt",

        @"TribeBin\tex32.cnt",
        @"TribeBin\vignette.cnt",
        @"TribeBin\Sound.cnt",
    };

    public override IEnumerable<Utility> GetUtilities(GameInstallation gameInstallation) => new Utility[]
    {
        new Utility_CPATextureSync(gameInstallation, CPATextureSyncData.FromGameMode(CPAGameMode.RaymanArena_PC)),
    };

    public override GameFinder_GameItem GetGameFinderItem() => new(UbiIniData_RaymanArena.SectionName, "Rayman Arena", new[]
    {
        "Rayman Arena",
        "Rayman: Arena",
    });

    public override GameInstallerInfo GetGameInstallerData() => new(
        discFilesListFileName: "RaymanArena",
        gameLogoFileName: "RaymanArena_Logo.png",
        gifFileNames: new[] { "ASTRO.gif", "CASK.gif", "CHASE.gif", "GLOB.gif", "RODEO.gif", });

    #endregion
}