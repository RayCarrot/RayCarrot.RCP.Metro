using RayCarrot.RCP.Metro.Games.Components;
using RayCarrot.RCP.Metro.Games.Finder;

namespace RayCarrot.RCP.Metro.Games.Clients.UbisoftConnect;

public class UbisoftConnectGameClientDescriptor : GameClientDescriptor
{
    #region Public Properties

    public override string GameClientId => "UbisoftConnect";
    public override LocalizedString DisplayName => "Ubisoft Connect"; // TODO-UPDATE: Localize
    public override GameClientIconAsset Icon => GameClientIconAsset.UbisoftConnect;

    #endregion

    #region Private Methods

    private static IEnumerable<DuoGridItemViewModel> GetGameInfoItems(GameInstallation gameInstallation) => new[]
    {
        new DuoGridItemViewModel(
            header: "Game ID:", // TODO-UPDATE: Localize
            text: gameInstallation.GetRequiredComponent<UbisoftConnectGameClientComponent>().GameId,
            minUserLevel: UserLevel.Advanced)
    };

    #endregion

    #region Public Methods

    public override void RegisterComponents(IGameComponentBuilder builder)
    {
        base.RegisterComponents(builder);

        builder.Register(new GameInfoComponent(GetGameInfoItems));
        builder.Register<LaunchGameComponent, UbisoftConnectLaunchGameComponent>();
    }

    public override bool SupportsGame(GameInstallation gameInstallation, GameClientInstallation gameClientInstallation) =>
        base.SupportsGame(gameInstallation, gameClientInstallation) && gameInstallation.HasComponent<UbisoftConnectGameClientComponent>();

    public override FinderQuery[] GetFinderQueries() => new FinderQuery[]
    {
        new UninstallProgramFinderQuery("Ubisoft Connect"),

        new Win32ShortcutFinderQuery("Ubisoft Connect"),
        new Win32ShortcutFinderQuery("Uplay"),
    };

    #endregion
}