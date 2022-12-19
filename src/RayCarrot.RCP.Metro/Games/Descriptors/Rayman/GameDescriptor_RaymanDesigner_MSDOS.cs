using BinarySerializer.Ray1;
using RayCarrot.RCP.Metro.Archive;
using RayCarrot.RCP.Metro.Archive.Ray1;
using RayCarrot.RCP.Metro.Games.Components;
using RayCarrot.RCP.Metro.Games.OptionsDialog;

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
    
    public override LocalizedString DisplayName => "Rayman Designer";
    public override string DefaultFileName => "RAYKIT.bat";
    public override DateTime ReleaseDate => new(1997, 01, 01); // Not exact

    public override GameIconAsset Icon => GameIconAsset.RaymanDesigner;

    public override bool HasArchives => true;

    public override string ExecutableName => "RAYKIT.EXE";

    #endregion

    #region Protected Methods

    protected override void RegisterComponents(DescriptorComponentBuilder builder)
    {
        base.RegisterComponents(builder);

        builder.Register(new ProgressionManagersComponent(x => new GameProgressionManager_RaymanDesigner(x, "Rayman Designer")));
        builder.Register(new GameConfigComponent(x => new RaymanDesignerConfigViewModel(this, x)));
        builder.Register<OnGameAddedComponent, AddToJumpListOnGameAddedComponent>();
        builder.Register<OnGameAddedComponent, FindRaymanForeverFilesOnGameAddedComponent>();

        builder.Register(new UtilityComponent(x => new Utility_RaymanDesigner_ReplaceFiles(x)));
        builder.Register(new UtilityComponent(x => new Utility_RaymanDesigner_CreateConfig(x)));
    }

    #endregion

    #region Public Methods

    public override IEnumerable<GameAddAction> GetAddActions() => new GameAddAction[]
    {
        new LocateRayman1MSDOSGameAddAction(this),
    };

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

    public override IEnumerable<GamePurchaseLink> GetPurchaseLinks() => new GamePurchaseLink[]
    {
        new(new ResourceLocString(nameof(Resources.GameDisplay_PurchaseGOG)), "https://www.gog.com/game/rayman_forever"),
        new(new ResourceLocString(nameof(Resources.GameDisplay_PurchaseUplay)), "https://store.ubi.com/eu/rayman--forever/5800d3fc4e016524248b4567.html")
    };

    public override GameFinder_GameItem GetGameFinderItem() => new(null, "Rayman Forever", new[] { "Rayman Forever", },
        // Navigate to the sub-directory
        x => x.Name.Equals("DOSBOX", StringComparison.OrdinalIgnoreCase) ? x.Parent + "RayKit" : x + "RayKit");

    #endregion
}