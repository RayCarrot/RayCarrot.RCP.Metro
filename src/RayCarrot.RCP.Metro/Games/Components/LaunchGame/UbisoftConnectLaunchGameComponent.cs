namespace RayCarrot.RCP.Metro.Games.Components;

[RequiredGameComponents(typeof(UbisoftConnectGameClientComponent))]
public class UbisoftConnectLaunchGameComponent : UriLaunchGameComponent
{
    // TODO: Is there a way to pass in launch arguments using a Uplay URI?
    public override bool SupportsLaunchArguments => false;

    protected override string GetLaunchUri() =>
        UbisoftConnectHelpers.GetGameLaunchURI(GameInstallation.GetRequiredComponent<UbisoftConnectGameClientComponent>().GameId);
}