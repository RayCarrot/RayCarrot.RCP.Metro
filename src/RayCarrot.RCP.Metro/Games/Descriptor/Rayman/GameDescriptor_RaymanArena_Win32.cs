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
    public override Games LegacyGame => Games.RaymanArena;

    public override string DisplayName => "Rayman Arena";
    public override string BackupName => "Rayman Arena";
    public override string DefaultFileName => "R_Arena.exe";

    public override string RayMapURL => AppURLs.GetRayMapGameURL("ra_pc", "ra_pc");

    public override IEnumerable<string> DialogGroupNames => new[] { UbiIniFileGroupName };

    public override bool CanBeInstalledFromDisc => true;
    public override string[] InstallerGifs
    {
        get
        {
            const string basePath = $"{AppViewModel.WPFApplicationBasePath}Installer/InstallerGifs/";

            return new[]
            {
                basePath + "ASTRO.gif",
                basePath + "CASK.gif",
                basePath + "CHASE.gif",
                basePath + "GLOB.gif",
                basePath + "RODEO.gif",
            };
        }
    }

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
        new CPACntArchiveDataManager(new OpenSpaceSettings(EngineVersion.RaymanArena, BinarySerializer.OpenSpace.Platform.PC), gameInstallation);

    public override FileSystemPath[] GetArchiveFilePaths(FileSystemPath installDir) => new[]
    {
        installDir + "MenuBin" + "tex32.cnt",
        installDir + "MenuBin" + "vignette.cnt",
        installDir + "MenuBin" + "Sound.cnt",

        installDir + "FishBin" + "tex32.cnt",
        installDir + "FishBin" + "vignette.cnt",
        installDir + "FishBin" + "Sound.cnt",

        installDir + "TribeBin" + "tex32.cnt",
        installDir + "TribeBin" + "vignette.cnt",
        installDir + "TribeBin" + "Sound.cnt",
    };

    public override IEnumerable<Utility> GetUtilities(GameInstallation gameInstallation) => new Utility[]
    {
        new Utility_RaymanArena_GameSyncTextureInfo(gameInstallation),
    };

    public override GameFinder_GameItem GetGameFinderItem() => new(UbiIniData_RaymanArena.SectionName, "Rayman Arena", new[]
    {
        "Rayman Arena",
        "Rayman: Arena",
    });

    #endregion
}