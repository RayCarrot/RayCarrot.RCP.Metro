using System.IO;
using RayCarrot.RCP.Metro.Games.Components;
using RayCarrot.RCP.Metro.Games.Finder;

namespace RayCarrot.RCP.Metro.Games.Clients.UbisoftConnect;

public class UbisoftConnectGameClientDescriptor : GameClientDescriptor
{
    #region Public Properties

    public override string GameClientId => "UbisoftConnect";
    public override LocalizedString DisplayName => new ResourceLocString(nameof(Resources.GameClients_UbisoftConnect));
    public override GameClientIconAsset Icon => GameClientIconAsset.UbisoftConnect;

    #endregion

    #region Private Methods

    private static IEnumerable<DuoGridItemViewModel> GetGameInfoItems(GameInstallation gameInstallation) => new[]
    {
        new DuoGridItemViewModel(
            header: new ResourceLocString(nameof(Resources.GameInfo_UbisoftGameId)),
            text: gameInstallation.GetRequiredComponent<UbisoftConnectGameClientComponent>().GameId,
            minUserLevel: UserLevel.Advanced)
    };

    private static IEnumerable<GameLinksComponent.GameUriLink> GetExternalUriLinks(GameInstallation gameInstallation)
    {
        string productId = gameInstallation.GetRequiredComponent<UbisoftConnectGameClientComponent>().ProductId;

        return new[]
        {
            new GameLinksComponent.GameUriLink(
                Header: new ResourceLocString(nameof(Resources.GameDisplay_OpenInUbisoftStore)),
                Uri: UbisoftConnectHelpers.GetStorePageURL(productId),
                Icon: GenericIconKind.GameAction_Web)
        };
    }

    #endregion

    #region Public Methods

    public override void RegisterComponents(IGameComponentBuilder builder)
    {
        base.RegisterComponents(builder);

        builder.Register(new GameInfoComponent(GetGameInfoItems));
        builder.Register<LaunchGameComponent, UbisoftConnectLaunchGameComponent>();
        builder.Register(new ExternalGameLinksComponent(GetExternalUriLinks));
    }

    public override GameClientOptionsViewModel GetGameClientOptionsViewModel(GameClientInstallation gameClientInstallation) =>
        new UbisoftConnectGameClientOptionsViewModel(gameClientInstallation);

    public override bool SupportsGame(GameInstallation gameInstallation, GameClientInstallation gameClientInstallation) =>
        base.SupportsGame(gameInstallation, gameClientInstallation) && gameInstallation.HasComponent<UbisoftConnectGameClientComponent>();

    public override async Task OnGameClientAddedAsync(GameClientInstallation gameClientInstallation)
    {
        await base.OnGameClientAddedAsync(gameClientInstallation);

        // Set a default user id
        FileSystemPath saveGamesDir = gameClientInstallation.InstallLocation.Directory + "savegames";

        if (saveGamesDir.DirectoryExists)
        {
            string[] subDirs = Directory.GetDirectories(saveGamesDir);

            if (subDirs.Length > 0)
            {
                string userId = new FileSystemPath(subDirs[0]).Name;
                gameClientInstallation.SetValue<string>(GameClientDataKey.UbisoftConnect_UserId, userId);
            }
        }
    }

    public override FinderQuery[] GetFinderQueries() => new FinderQuery[]
    {
        new UninstallProgramFinderQuery("Ubisoft Connect"),

        new Win32ShortcutFinderQuery("Ubisoft Connect"),
        new Win32ShortcutFinderQuery("Uplay"),
    };

    #endregion
}