﻿using System.Collections.Generic;
using BinarySerializer.OpenSpace;
using RayCarrot.RCP.Metro.Archive;
using RayCarrot.RCP.Metro.Archive.CPA;
using RayCarrot.RCP.Metro.Ini;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// The Rayman 3 (Win32) game descriptor
/// </summary>
public sealed class GameDescriptor_Rayman3_Win32 : Win32GameDescriptor
{
    #region Public Properties

    public override string Id => "Rayman3_Win32";
    public override Game Game => Game.Rayman3;
    public override GameCategory Category => GameCategory.Rayman;
    public override Games? LegacyGame => Games.Rayman3;

    public override string DisplayName => "Rayman 3";
    public override string BackupName => "Rayman 3";
    public override string DefaultFileName => "Rayman3.exe";

    public override string RayMapURL => AppURLs.GetRayMapGameURL("r3_pc", "r3_pc");

    public override IEnumerable<string> DialogGroupNames => new string[]
    {
        UbiIniFileGroupName
    };

    public override bool HasArchives => true;

    #endregion

    #region Public Methods

    public override GameOptionsDialog_ConfigPageViewModel GetConfigPageViewModel(GameInstallation gameInstallation) => 
        new Config_Rayman3_ViewModel(gameInstallation);

    public override GameProgressionManager GetGameProgressionManager(GameInstallation gameInstallation) => 
        new GameProgressionManager_Rayman3(gameInstallation);

    public override IEnumerable<GameUriLink> GetLocalUriLinks(GameInstallation gameInstallation) => new GameUriLink[]
    {
        new(new ResourceLocString(nameof(Resources.GameLink_Setup)), gameInstallation.InstallLocation + "R3_Setup_DX8.exe")
    };

    public override IArchiveDataManager GetArchiveDataManager(GameInstallation? gameInstallation) => 
        new CPACntArchiveDataManager(
            settings: new OpenSpaceSettings(EngineVersion.Rayman3, BinarySerializer.OpenSpace.Platform.PC), 
            gameInstallation: gameInstallation, 
            cpaTextureSyncData: CPATextureSyncData.FromGameMode(CPAGameMode.Rayman3_PC));

    public override IEnumerable<string> GetArchiveFilePaths(GameInstallation? gameInstallation) => new[]
    {
        @"Gamedatabin\tex32_1.cnt",
        @"Gamedatabin\tex32_2.cnt",
        @"Gamedatabin\vignette.cnt",
    };

    public override IEnumerable<Utility> GetUtilities(GameInstallation gameInstallation) => new Utility[]
    {
        new Utility_CPATextureSync(gameInstallation, CPATextureSyncData.FromGameMode(CPAGameMode.Rayman3_PC)),
        new Utility_Rayman3_DirectPlay(gameInstallation),
    };

    public override IEnumerable<GamePurchaseLink> GetGamePurchaseLinks() => new GamePurchaseLink[]
    {
        new(Resources.GameDisplay_PurchaseGOG, "https://www.gog.com/game/rayman_3_hoodlum_havoc"),
        new(Resources.GameDisplay_PurchaseUplay, "https://store.ubi.com/eu/rayman--3--hoodlum-havoc/5800b15eef3aa5ab3e8b4567.html")
    };

    public override GameFinder_GameItem GetGameFinderItem() => new(UbiIniData_Rayman3.SectionName, "Rayman 3", new[]
    {
        "Rayman 3",
        "Rayman: 3",
        "Rayman 3 - Hoodlum Havoc",
        "Rayman: 3 - Hoodlum Havoc",
    });

    #endregion
}