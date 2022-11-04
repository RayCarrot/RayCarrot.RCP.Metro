﻿using System.Collections.Generic;
using System.Threading.Tasks;
using BinarySerializer.OpenSpace;
using RayCarrot.RCP.Metro.Archive;
using RayCarrot.RCP.Metro.Archive.CPA;
using RayCarrot.RCP.Metro.Ini;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// The Rayman 2 (Win32) game descriptor
/// </summary>
public sealed class GameDescriptor_Rayman2_Win32 : Win32GameDescriptor
{
    #region Public Properties

    public override string Id => "Rayman2_Win32";
    public override Game Game => Game.Rayman2;
    public override GameCategory Category => GameCategory.Rayman;
    public override Games? LegacyGame => Games.Rayman2;

    public override string DisplayName => "Rayman 2";
    public override string BackupName => "Rayman 2";
    public override string DefaultFileName => "Rayman2.exe";

    public override string RayMapURL => AppURLs.GetRayMapGameURL("r2_pc", "r2_pc");

    public override IEnumerable<string> DialogGroupNames => new string[]
    {
        UbiIniFileGroupName
    };

    public override bool HasGameInstaller => true;
    public override bool HasArchives => true;

    #endregion

    #region Public Methods

    public override GameOptionsDialog_ConfigPageViewModel GetConfigPageViewModel(GameInstallation gameInstallation) => 
        new Config_Rayman2_ViewModel(gameInstallation);

    public override IEnumerable<ProgressionGameViewModel> GetProgressionGameViewModels(GameInstallation gameInstallation) =>
        new ProgressionGameViewModel_Rayman2(gameInstallation).Yield();

    public override IEnumerable<GameFileLink> GetGameFileLinks(GameInstallation gameInstallation) => new GameFileLink[]
    {
        new(Resources.GameLink_Setup, gameInstallation.InstallLocation + "GXSetup.exe"),
        new(Resources.GameLink_R2nGlide, gameInstallation.InstallLocation + "nglide_config.exe"),
        new(Resources.GameLink_R2dgVoodoo, gameInstallation.InstallLocation + "dgVoodooCpl.exe"),
        new(Resources.GameLink_R2Fix, gameInstallation.InstallLocation + "R2FixCfg.exe"),
    };

    public override IArchiveDataManager GetArchiveDataManager(GameInstallation? gameInstallation) =>
        new CPACntArchiveDataManager(
            settings: new OpenSpaceSettings(EngineVersion.Rayman2, BinarySerializer.OpenSpace.Platform.PC),
            gameInstallation: gameInstallation,
            cpaTextureSyncData: CPATextureSyncData.FromGameMode(CPAGameMode.Rayman2_PC));

    public override FileSystemPath[] GetArchiveFilePaths(FileSystemPath installDir) => new[]
    {
        installDir + "Data" + "Textures.cnt",
        installDir + "Data" + "Vignette.cnt",
    };

    public override IEnumerable<GamePurchaseLink> GetGamePurchaseLinks() => new GamePurchaseLink[]
    {
        new(Resources.GameDisplay_PurchaseGOG, "https://www.gog.com/game/rayman_2_the_great_escape"),
        new(Resources.GameDisplay_PurchaseUplay, "https://store.ubi.com/eu/rayman-2--the-great-escape/56c4947e88a7e300458b465c.html")
    };

    public override IEnumerable<Utility> GetUtilities(GameInstallation gameInstallation) => new Utility[]
    {
        new Utility_CPATextureSync(gameInstallation, CPATextureSyncData.FromGameMode(CPAGameMode.Rayman2_PC)),
    };

    public override async Task<IList<string>> GetAppliedUtilitiesAsync(GameInstallation gameInstallation)
    {
        // TODO-14: These are not utilities - these are game modifications caused by the config

        // Create the output
        var output = new List<string>();

        if (await Config_Rayman2_ViewModel.GetIsWidescreenHackAppliedAsync(gameInstallation) == true)
            output.Add(Resources.Config_WidescreenSupport);

        var dinput = Config_Rayman2_ViewModel.GetCurrentDinput(gameInstallation);

        if (dinput == Config_Rayman2_ViewModel.R2Dinput.Controller)
            output.Add(Resources.Config_UseController);

        if (dinput == Config_Rayman2_ViewModel.R2Dinput.Mapping)
            output.Add(Resources.Config_ButtonMapping);

        // Get other utilities
        output.AddRange(await base.GetAppliedUtilitiesAsync(gameInstallation));

        return output;
    }

    public override GameFinder_GameItem GetGameFinderItem() => new(UbiIniData_Rayman2.SectionName, "Rayman 2", new[]
    {
        "Rayman 2",
        "Rayman: 2",
        "Rayman 2 - The Great Escape",
        "Rayman: 2 - The Great Escape",
        "GOG.com Rayman 2",
    });

    public override GameInstallerInfo GetGameInstallerData() => new(
        discFilesListFileName: "Rayman2",
        gameLogoFileName: "Rayman2_Logo.png",
        gifFileNames: new[] { "ASTRO.gif", "CASK.gif", "CHASE.gif", "GLOB.gif", "RODEO.gif", });

    #endregion
}