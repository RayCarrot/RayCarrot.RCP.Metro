using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using BinarySerializer.Ray1;
using RayCarrot.RCP.Metro.Archive;
using RayCarrot.RCP.Metro.Archive.Ray1;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// The Rayman Designer (MS-DOS) game descriptor
/// </summary>
public sealed class GameDescriptor_RaymanDesigner_MSDOS : MsDosGameDescriptor
{
    #region Public Properties

    public override string GameId => "RaymanDesigner_MSDOS";
    public override Game Game => Game.RaymanDesigner;
    public override GameCategory Category => GameCategory.Rayman;
    public override LegacyGame? LegacyGame => Metro.LegacyGame.RaymanDesigner;
    
    public override string DisplayName => "Rayman Designer";
    public override string DefaultFileName => "RAYKIT.bat";

    public override GameIconAsset Icon => GameIconAsset.RaymanDesigner;

    public override bool HasArchives => true;

    public override string ExecutableName => "RAYKIT.EXE";

    #endregion

    #region Public Methods

    public override IEnumerable<GameAddAction> GetAddActions() => new GameAddAction[]
    {
        new LocateRayman1MSDOSGameAddAction(this),
    };

    public override FrameworkElement GetOptionsUI(GameInstallation gameInstallation) =>
        new GameOptions_DOSBox_Control(gameInstallation);

    public override GameOptionsDialog_ConfigPageViewModel GetConfigPageViewModel(GameInstallation gameInstallation) => 
        new Config_RaymanDesigner_ViewModel(this, gameInstallation);

    public override IEnumerable<GameProgressionManager> GetGameProgressionManagers(GameInstallation gameInstallation) => 
        new GameProgressionManager_RaymanDesigner(gameInstallation, "Rayman Designer").Yield();

    public override IEnumerable<GameUriLink> GetLocalUriLinks(GameInstallation gameInstallation) => new GameUriLink[]
    {
        new(new ResourceLocString(nameof(Resources.GameLink_RDMapper)), gameInstallation.InstallLocation + "MAPPER.EXE")
    };

    public override RayMapInfo GetRayMapInfo() => new(RayMapViewer.Ray1Map, "RaymanDesignerPC", "r1/pc_kit");

    public override IArchiveDataManager GetArchiveDataManager(GameInstallation? gameInstallation) => 
        new Ray1PCArchiveDataManager(new Ray1Settings(Ray1EngineVersion.PC_Kit));

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

    public override IEnumerable<Utility> GetUtilities(GameInstallation gameInstallation) => new Utility[]
    {
        new Utility_RaymanDesigner_ReplaceFiles(gameInstallation),
        new Utility_RaymanDesigner_CreateConfig(gameInstallation),
    };

    public override IEnumerable<GamePurchaseLink> GetPurchaseLinks() => new GamePurchaseLink[]
    {
        new(new ResourceLocString(nameof(Resources.GameDisplay_PurchaseGOG)), "https://www.gog.com/game/rayman_forever"),
        new(new ResourceLocString(nameof(Resources.GameDisplay_PurchaseUplay)), "https://store.ubi.com/eu/rayman--forever/5800d3fc4e016524248b4567.html")
    };

    public override GameFinder_GameItem GetGameFinderItem() => new(null, "Rayman Forever", new[] { "Rayman Forever", },
        // Navigate to the sub-directory
        x => x.Name.Equals("DOSBOX", StringComparison.OrdinalIgnoreCase) ? x.Parent + "RayKit" : x + "RayKit");

    public override async Task PostGameAddAsync(GameInstallation gameInstallation)
    {
        await base.PostGameAddAsync(gameInstallation);
        GameDescriptorHelpers.PostAddRaymanForever(gameInstallation);
    }

    #endregion
}