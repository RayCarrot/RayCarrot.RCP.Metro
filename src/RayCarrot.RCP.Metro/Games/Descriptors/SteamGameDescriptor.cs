using RayCarrot.RCP.Metro.Games.Components;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// A game descriptor for a Steam game
/// </summary>
public abstract class SteamGameDescriptor : GameDescriptor
{
    #region Public Properties

    public override GamePlatform Platform => GamePlatform.Steam;

    /// <summary>
    /// The Steam product id
    /// </summary>
    public abstract string SteamID { get; }

    #endregion

    #region Private Methods

    private IEnumerable<DuoGridItemViewModel> GetGameInfoItems(GameInstallation gameInstallation) => new[]
    {
        new DuoGridItemViewModel(
            header: new ResourceLocString(nameof(Resources.GameInfo_SteamID)),
            text: SteamID,
            minUserLevel: UserLevel.Advanced)
    };

    #endregion

    #region Protected Methods

    protected override void RegisterComponents(GameComponentBuilder builder)
    {
        base.RegisterComponents(builder);

        builder.Register(new GameInfoComponent(GetGameInfoItems));
        builder.Register<LaunchGameComponent>(new SteamLaunchGameComponent(SteamID));
    }

    #endregion

    #region Public Methods

    public override IEnumerable<GameAddAction> GetAddActions() => new GameAddAction[]
    {
        new LocateGameAddAction(this, singleInstallationOnly: true),
        new FindSteamGameAddAction(this),
    };

    public override IEnumerable<GameUriLink> GetExternalUriLinks(GameInstallation gameInstallation) => new[]
    {
        new GameUriLink(
            Header: new ResourceLocString(nameof(Resources.GameDisplay_OpenSteamStore)),
            Uri: SteamHelpers.GetStorePageURL(SteamID),
            Icon: GenericIconKind.GameAction_Steam),
        new GameUriLink(
            Header: new ResourceLocString(nameof(Resources.GameDisplay_OpenSteamCommunity)),
            Uri: SteamHelpers.GetCommunityPageURL(SteamID),
            Icon: GenericIconKind.GameAction_Steam)
    };

    public override IEnumerable<GamePurchaseLink> GetPurchaseLinks() => new GamePurchaseLink[]
    {
        new(new ResourceLocString(nameof(Resources.GameDisplay_Steam)), SteamHelpers.GetStorePageURL(SteamID)),
    };

    public override GameFinder_GameItem GetGameFinderItem() => new(SteamID);

    #endregion
}