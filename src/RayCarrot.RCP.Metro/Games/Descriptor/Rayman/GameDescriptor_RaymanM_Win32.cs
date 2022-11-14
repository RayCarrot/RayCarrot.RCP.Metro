using System.Collections.Generic;
using BinarySerializer.OpenSpace;
using RayCarrot.RCP.Metro.Archive;
using RayCarrot.RCP.Metro.Archive.CPA;
using RayCarrot.RCP.Metro.Ini;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// The Rayman M (Win32) game descriptor
/// </summary>
public sealed class GameDescriptor_RaymanM_Win32 : Win32GameDescriptor
{
    #region Public Properties

    public override string Id => "RaymanM_Win32";
    public override Game Game => Game.RaymanMArena;
    public override GameCategory Category => GameCategory.Rayman;
    public override Games? LegacyGame => Games.RaymanM;

    public override string DisplayName => "Rayman M";
    public override string BackupName => "Rayman M";
    public override string DefaultFileName => "RaymanM.exe";

    public override string RayMapURL => AppURLs.GetRayMapGameURL("rm_pc", "rm_pc");

    public override IEnumerable<string> DialogGroupNames => new[] { UbiIniFileGroupName };

    public override bool HasGameInstaller => true;
    public override bool HasArchives => true;

    #endregion

    #region Public Methods

    public override GameOptionsDialog_ConfigPageViewModel GetConfigPageViewModel(GameInstallation gameInstallation) => 
        new Config_RaymanM_ViewModel(gameInstallation);

    public override GameProgressionManager GetGameProgressionManager(GameInstallation gameInstallation) => 
        new GameProgressionManager_RaymanMArena(gameInstallation, false);

    public override IEnumerable<GameUriLink> GetLocalUriLinks(GameInstallation gameInstallation) => new GameUriLink[]
    {
        new(new ResourceLocString(nameof(Resources.GameLink_Setup)), gameInstallation.InstallLocation + "RM_Setup_DX8.exe")
    };

    public override IArchiveDataManager GetArchiveDataManager(GameInstallation? gameInstallation) => 
        new CPACntArchiveDataManager(
            settings: new OpenSpaceSettings(EngineVersion.RaymanM, BinarySerializer.OpenSpace.Platform.PC), 
            gameInstallation: gameInstallation,
            cpaTextureSyncData: CPATextureSyncData.FromGameMode(CPAGameMode.RaymanM_PC));

    public override IEnumerable<string> GetArchiveFilePaths(GameInstallation? gameInstallation) => new[]
    {
        @"FishBin\tex32.cnt",
        @"FishBin\vignette.cnt",
        @"MenuBin\tex32.cnt",
        @"MenuBin\vignette.cnt",
        @"TribeBin\tex32.cnt",
        @"TribeBin\vignette.cnt",
    };

    public override IEnumerable<Utility> GetUtilities(GameInstallation gameInstallation) => new Utility[]
    {
        new Utility_CPATextureSync(gameInstallation, CPATextureSyncData.FromGameMode(CPAGameMode.RaymanM_PC)),
    };

    public override GameFinder_GameItem GetGameFinderItem() => new(UbiIniData_RaymanM.SectionName, "Rayman M", new[]
    {
        "Rayman M",
        "Rayman: M",
    });

    public override GameInstallerInfo GetGameInstallerData() => new(
        discFilesListFileName: "RaymanM",
        gameLogoFileName: "RaymanM_Logo.png",
        gifFileNames: new[] { "ASTRO.gif", "CASK.gif", "CHASE.gif", "GLOB.gif", "RODEO.gif", });

    #endregion
}