namespace RayCarrot.RCP.Metro.Games.Components;

[RequiredGameComponents(typeof(SteamGameClientComponent))]
public class SteamLaunchGameComponent : UriLaunchGameComponent
{
    protected override string GetLaunchUri() =>
        SteamHelpers.GetGameLaunchURI(GameInstallation.GetRequiredComponent<SteamGameClientComponent>().SteamId);
}