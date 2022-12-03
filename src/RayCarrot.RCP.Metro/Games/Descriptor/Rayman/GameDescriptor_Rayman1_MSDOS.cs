﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// The Rayman 1 (MS-DOS) game descriptor
/// </summary>
public sealed class GameDescriptor_Rayman1_MSDOS : MSDOSGameDescriptor
{
    #region Public Properties

    public override string Id => "Rayman1_MSDOS";
    public override Game Game => Game.Rayman1;
    public override GameCategory Category => GameCategory.Rayman;
    public override Games? LegacyGame => Games.Rayman1;

    public override string DisplayName => "Rayman";
    public override string DefaultFileName => "Rayman.exe";

    public override GameIconAsset Icon => GameIconAsset.Rayman1;

    public override string ExecutableName => "RAYMAN.EXE";

    // TODO-14: Fix this. Perhaps have TPLS add an EmulatorInstallation? Somehow it has to override it anyway.
    //public override FileSystemPath DOSBoxFilePath => Services.Data.Utility_TPLSData?.IsEnabled != true 
    //    ? base.DOSBoxFilePath 
    //    : Services.Data.Utility_TPLSData.DOSBoxFilePath;
    //public override IEnumerable<FileSystemPath> AdditionalConfigFiles => Services.Data.Utility_TPLSData?.IsEnabled != true 
    //    ? base.AdditionalConfigFiles 
    //    : new[] { Services.Data.Utility_TPLSData.ConfigFilePath };

    #endregion

    #region Public Methods

    public override IEnumerable<GameAddAction> GetAddActions() => new GameAddAction[]
    {
        new LocateRayman1MSDOSGameAddAction(this),
    };

    public override FrameworkElement GetOptionsUI(GameInstallation gameInstallation) =>
        new GameOptions_DOSBox_Control(gameInstallation);

    public override GameOptionsDialog_ConfigPageViewModel GetConfigPageViewModel(GameInstallation gameInstallation) =>
        new Config_Rayman1_ViewModel(this, gameInstallation);

    public override IEnumerable<GameProgressionManager> GetGameProgressionManagers(GameInstallation gameInstallation) =>
        new GameProgressionManager_Rayman1(gameInstallation, "Rayman 1").Yield();

    public override RayMapInfo GetRayMapInfo() => new(RayMapViewer.Ray1Map, "RaymanPC_1_21", "r1/pc_121");

    public override IEnumerable<Utility> GetUtilities(GameInstallation gameInstallation) => new Utility[]
    {
        new Utility_Rayman1_TPLS(gameInstallation),
        new Utility_Rayman1_CompleteSoundtrack(gameInstallation),
    };

    public override IEnumerable<GamePurchaseLink> GetPurchaseLinks() => new GamePurchaseLink[]
    {
        new(new ResourceLocString(nameof(Resources.GameDisplay_PurchaseGOG)), "https://www.gog.com/game/rayman_forever"),
        new(new ResourceLocString(nameof(Resources.GameDisplay_PurchaseUplay)), "https://store.ubi.com/eu/rayman--forever/5800d3fc4e016524248b4567.html")
    };

    public override GameFinder_GameItem GetGameFinderItem() => new(null, "Rayman Forever", new[] { "Rayman Forever", },
        // Navigate to the sub-directory
        x => x.Name.Equals("DOSBOX", StringComparison.OrdinalIgnoreCase) ? x.Parent + "Rayman" : x + "Rayman");

    public override async Task PostGameAddAsync(GameInstallation gameInstallation)
    {
        await base.PostGameAddAsync(gameInstallation);
        GameDescriptorHelpers.PostAddRaymanForever(gameInstallation);
    }

    #endregion
}