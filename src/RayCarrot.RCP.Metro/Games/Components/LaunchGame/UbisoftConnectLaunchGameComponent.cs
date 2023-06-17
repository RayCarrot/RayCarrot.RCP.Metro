namespace RayCarrot.RCP.Metro.Games.Components;

[RequiredGameComponents(typeof(UbisoftConnectGameClientComponent))]
public class UbisoftConnectLaunchGameComponent : UriLaunchGameComponent
{
    protected override string GetLaunchUri() =>
        UbisoftConnectHelpers.GetGameLaunchURI(GameInstallation.GetRequiredComponent<UbisoftConnectGameClientComponent>().GameId);
}