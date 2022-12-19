using RayCarrot.RCP.Metro.Games.Components;
using RayCarrot.RCP.Metro.Games.OptionsDialog;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// The Rayman 1 (MS-DOS) game descriptor
/// </summary>
public sealed class GameDescriptor_Rayman1_MSDOS : MsDosGameDescriptor
{
    #region Public Properties

    public override string GameId => "Rayman1_MSDOS";
    public override Game Game => Game.Rayman1;
    public override GameCategory Category => GameCategory.Rayman;
    public override LegacyGame? LegacyGame => Metro.LegacyGame.Rayman1;

    public override LocalizedString DisplayName => "Rayman";
    public override string DefaultFileName => "Rayman.exe";
    public override DateTime ReleaseDate => new(1995, 09, 01);

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

    #region Protected Methods

    protected override void RegisterComponents(DescriptorComponentBuilder builder)
    {
        base.RegisterComponents(builder);

        builder.Register(new ProgressionManagersComponent(x => new GameProgressionManager_Rayman1(x, "Rayman 1")));
        builder.Register(new GameConfigComponent(x => new Rayman1ConfigViewModel(this, x)));
        builder.Register<OnGameAddedComponent, AddToJumpListOnGameAddedComponent>();
        builder.Register<OnGameAddedComponent, FindRaymanForeverFilesOnGameAddedComponent>();

        builder.Register(new UtilityComponent(x => new Utility_Rayman1_TPLS(x)));
        builder.Register(new UtilityComponent(x => new Utility_Rayman1_CompleteSoundtrack(x)));
    }

    #endregion

    #region Public Methods

    public override IEnumerable<GameAddAction> GetAddActions() => new GameAddAction[]
    {
        new LocateRayman1MSDOSGameAddAction(this),
    };

    public override RayMapInfo GetRayMapInfo() => new(RayMapViewer.Ray1Map, "RaymanPC_1_21", "r1/pc_121");

    public override IEnumerable<GamePurchaseLink> GetPurchaseLinks() => new GamePurchaseLink[]
    {
        new(new ResourceLocString(nameof(Resources.GameDisplay_PurchaseGOG)), "https://www.gog.com/game/rayman_forever"),
        new(new ResourceLocString(nameof(Resources.GameDisplay_PurchaseUplay)), "https://store.ubi.com/eu/rayman--forever/5800d3fc4e016524248b4567.html")
    };

    public override GameFinder_GameItem GetGameFinderItem() => new(null, "Rayman Forever", new[] { "Rayman Forever", },
        // Navigate to the sub-directory
        x => x.Name.Equals("DOSBOX", StringComparison.OrdinalIgnoreCase) ? x.Parent + "Rayman" : x + "Rayman");

    #endregion
}