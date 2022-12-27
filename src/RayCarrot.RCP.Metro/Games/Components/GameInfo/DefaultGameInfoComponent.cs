namespace RayCarrot.RCP.Metro;

public class DefaultGameInfoComponent : GameInfoComponent
{
    public DefaultGameInfoComponent() : base(GetGameInfo) { }

    private static IEnumerable<DuoGridItemViewModel> GetGameInfo(GameInstallation gameInstallation) => new[]
    {
        new DuoGridItemViewModel(
            header: "Game id:",
            text: gameInstallation.GameId,
            minUserLevel: UserLevel.Debug),
        new DuoGridItemViewModel(
            header: "Installation id:",
            text: gameInstallation.InstallationId,
            minUserLevel: UserLevel.Debug),
        new DuoGridItemViewModel(
            header: new ResourceLocString(nameof(Resources.GameInfo_InstallDir)),
            text: gameInstallation.InstallLocation.FullPath),
        //new DuoGridItemViewModel("Install size", GameData.InstallDirectory.GetSize().ToString())
    };
}