namespace RayCarrot.RCP.Metro.Games.Components;

[RequiredGameComponents(typeof(SteamGameClientComponent))]
public class SteamLaunchGameComponent : UriLaunchGameComponent
{
    protected override string GetLaunchUri()
    {
        string steamId = GameInstallation.GetRequiredComponent<SteamGameClientComponent>().SteamId;
        string? launchArgs = GameInstallation.GetComponent<LaunchArgumentsComponent>()?.CreateObject();

        if (launchArgs != null)
            return SteamHelpers.GetGameLaunchURI(steamId, launchArgs);
        else
            return SteamHelpers.GetGameLaunchURI(steamId);
    }
}