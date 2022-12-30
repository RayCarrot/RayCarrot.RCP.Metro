using RayCarrot.RCP.Metro.Games.Components;
using RayCarrot.RCP.Metro.Games.OptionsDialog;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// The Rayman Raving Rabbids (Win32) game descriptor
/// </summary>
public sealed class GameDescriptor_RaymanRavingRabbids_Win32 : Win32GameDescriptor
{
    #region Constant Fields

    private const string SteamId = "15080";

    #endregion

    #region Public Properties

    public override string GameId => "RaymanRavingRabbids_Win32";
    public override Game Game => Game.RaymanRavingRabbids;
    public override GameCategory Category => GameCategory.Rabbids;
    public override LegacyGame? LegacyGame => Metro.LegacyGame.RaymanRavingRabbids;

    public override LocalizedString DisplayName => "Rayman Raving Rabbids";
    // TODO-14: Launch game exe directly and allow custom args like for RGH?
    public override string DefaultFileName => "CheckApplication.exe";
    public override DateTime ReleaseDate => new(2006, 12, 07);

    public override GameIconAsset Icon => GameIconAsset.RaymanRavingRabbids;

    #endregion

    #region Private Methods

    private static IEnumerable<GameLinksComponent.GameUriLink> GetLocalGameLinks(GameInstallation gameInstallation) => new[]
    {
        new GameLinksComponent.GameUriLink(
            Header: new ResourceLocString(nameof(Resources.GameLink_Setup)), 
            Uri: gameInstallation.InstallLocation + "SettingsApplication.exe")
    };

    #endregion

    #region Protected Methods

    protected override void RegisterComponents(IGameComponentBuilder builder)
    {
        base.RegisterComponents(builder);

        builder.Register(new SteamGameClientComponent(SteamId));

        builder.Register(new ProgressionManagersComponent(x => new GameProgressionManager_RaymanRavingRabbids(x, "Rayman Raving Rabbids")));
        builder.Register(new GameConfigComponent(x => new RaymanRavingRabbidsConfigViewModel(x)));
        builder.Register<OnGameAddedComponent, AddToJumpListOnGameAddedComponent>();
        builder.Register(new LocalGameLinksComponent(GetLocalGameLinks));
    }

    #endregion

    #region Public Methods

    public override IEnumerable<GameAddAction> GetAddActions() => base.GetAddActions().Concat(new GameAddAction[]
    {
        new FindSteamGameAddAction(this, SteamId),
    });

    public override IEnumerable<GamePurchaseLink> GetPurchaseLinks() => new GamePurchaseLink[]
    {
        new(new ResourceLocString(nameof(Resources.GameDisplay_PurchaseGOG)), "https://www.gog.com/game/rayman_raving_rabbids"),
        new(new ResourceLocString(nameof(Resources.GameDisplay_PurchaseUplay)), "https://store.ubi.com/eu/rayman-raving-rabbids/56c4948888a7e300458b47de.html"),
        new(new ResourceLocString(nameof(Resources.GameDisplay_Steam)), SteamHelpers.GetStorePageURL(SteamId)),
    };

    public override GameFinder_GameItem GetGameFinderItem() => new(null, "Rayman Raving Rabbids", new[]
    {
        "Rayman Raving Rabbids",
        "Rayman: Raving Rabbids",
        "RRR",
    });

    #endregion
}