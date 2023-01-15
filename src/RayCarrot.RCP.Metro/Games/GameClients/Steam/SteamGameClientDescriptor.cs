using RayCarrot.RCP.Metro.Games.Components;

namespace RayCarrot.RCP.Metro.Games.Clients.Steam;

public class SteamGameClientDescriptor : GameClientDescriptor
{
    #region Public Properties

    public override string GameClientId => "Steam";
    public override LocalizedString DisplayName => "Steam"; // TODO-UPDATE: Localize
    public override GameClientIconAsset Icon => GameClientIconAsset.Steam;

    #endregion

    #region Private Methods

    private static IEnumerable<DuoGridItemViewModel> GetGameInfoItems(GameInstallation gameInstallation) => new[]
    {
        new DuoGridItemViewModel(
            header: new ResourceLocString(nameof(Resources.GameInfo_SteamID)),
            text: gameInstallation.GetRequiredComponent<SteamGameClientComponent>().SteamId,
            minUserLevel: UserLevel.Advanced)
    };

    private static IEnumerable<GameLinksComponent.GameUriLink> GetExternalUriLinks(GameInstallation gameInstallation)
    {
        string steamId = gameInstallation.GetRequiredComponent<SteamGameClientComponent>().SteamId;

        return new[]
        {
            new GameLinksComponent.GameUriLink(
                Header: new ResourceLocString(nameof(Resources.GameDisplay_OpenSteamStore)),
                Uri: SteamHelpers.GetStorePageURL(steamId),
                Icon: GenericIconKind.GameAction_Steam),
            new GameLinksComponent.GameUriLink(
                Header: new ResourceLocString(nameof(Resources.GameDisplay_OpenSteamCommunity)),
                Uri: SteamHelpers.GetCommunityPageURL(steamId),
                Icon: GenericIconKind.GameAction_Steam)
        };
    }

    #endregion

    #region Public Methods

    public override void RegisterComponents(IGameComponentBuilder builder)
    {
        base.RegisterComponents(builder);

        builder.Register(new GameInfoComponent(GetGameInfoItems));
        builder.Register<LaunchGameComponent, SteamLaunchGameComponent>();
        builder.Register(new ExternalGameLinksComponent(GetExternalUriLinks));
    }

    public override bool SupportsGame(GameInstallation gameInstallation, GameClientInstallation gameClientInstallation) =>
        base.SupportsGame(gameInstallation, gameClientInstallation) && gameInstallation.HasComponent<SteamGameClientComponent>();

    #endregion
}