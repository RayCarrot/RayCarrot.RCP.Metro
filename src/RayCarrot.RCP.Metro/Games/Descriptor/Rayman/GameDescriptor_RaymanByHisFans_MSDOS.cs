using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using BinarySerializer.Ray1;
using RayCarrot.RCP.Metro.Archive;
using RayCarrot.RCP.Metro.Archive.Ray1;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// The Rayman by his Fans (MS-DOS) game descriptor
/// </summary>
public sealed class GameDescriptor_RaymanByHisFans_MSDOS : MSDOSGameDescriptor
{
    #region Public Properties

    public override string Id => "RaymanByHisFans_MSDOS";
    public override Game Game => Game.RaymanByHisFans;
    public override GameCategory Category => GameCategory.Rayman;
    public override Games? LegacyGame => Games.RaymanByHisFans;

    public override string DisplayName => "Rayman by his Fans";
    public override string DefaultFileName => "rayfan.bat";

    public override GameIconAsset Icon => GameIconAsset.RaymanByHisFans;

    public override bool HasArchives => true;

    public override string ExecutableName => "RAYFAN.EXE";

    #endregion

    #region Public Methods

    public override IEnumerable<GameAddAction> GetAddActions() => new GameAddAction[]
    {
        new LocateRayman1MSDOSGameAddAction(this),
    };

    public override FrameworkElement GetOptionsUI(GameInstallation gameInstallation) =>
        new GameOptions_DOSBox_Control(gameInstallation);

    public override GameOptionsDialog_ConfigPageViewModel GetConfigPageViewModel(GameInstallation gameInstallation) => 
        new Config_RaymanByHisFans_ViewModel(this, gameInstallation);

    public override RayMapInfo GetRayMapInfo() => new(RayMapViewer.Ray1Map, "RaymanByHisFansPC", "r1/pc_fan");

    public override IEnumerable<GameProgressionManager> GetGameProgressionManagers(GameInstallation gameInstallation) => 
        new GameProgressionManager_RaymanByHisFans(gameInstallation, "Rayman by his Fans").Yield();

    public override IArchiveDataManager GetArchiveDataManager(GameInstallation? gameInstallation) => 
        new Ray1PCArchiveDataManager(new Ray1Settings(Ray1EngineVersion.PC_Fan));

    public override IEnumerable<string> GetArchiveFilePaths(GameInstallation? gameInstallation) => new[]
    {
        @"PCMAP\COMMON.DAT",
        @"PCMAP\SNDD8B.DAT",
        @"PCMAP\SNDH8B.DAT",
        @"PCMAP\VIGNET.DAT",

        @"PCMAP\AL\SNDSMP.DAT",
        @"PCMAP\AL\SPECIAL.DAT",

        @"PCMAP\FR\SNDSMP.DAT",
        @"PCMAP\FR\SPECIAL.DAT",

        @"PCMAP\USA\SNDSMP.DAT",
        @"PCMAP\USA\SPECIAL.DAT",
    };

    public override IEnumerable<GamePurchaseLink> GetPurchaseLinks() => new GamePurchaseLink[]
    {
        new(new ResourceLocString(nameof(Resources.GameDisplay_PurchaseGOG)), "https://www.gog.com/game/rayman_forever"),
        new(new ResourceLocString(nameof(Resources.GameDisplay_PurchaseUplay)), "https://store.ubi.com/eu/rayman--forever/5800d3fc4e016524248b4567.html")
    };

    public override GameFinder_GameItem GetGameFinderItem() => new(null, "Rayman Forever", new[] { "Rayman Forever", },
        // Navigate to the sub-directory
        x => x.Name.Equals("DOSBOX", StringComparison.OrdinalIgnoreCase) ? x.Parent + "RayFan" : x + "RayFan");

    public override async Task PostGameAddAsync(GameInstallation gameInstallation)
    {
        await base.PostGameAddAsync(gameInstallation);
        GameDescriptorHelpers.PostAddRaymanForever(gameInstallation);
    }

    #endregion
}