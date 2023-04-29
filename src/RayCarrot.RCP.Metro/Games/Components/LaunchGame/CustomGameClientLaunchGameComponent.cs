using RayCarrot.RCP.Metro.Games.Clients;

namespace RayCarrot.RCP.Metro.Games.Components;

public class CustomGameClientLaunchGameComponent : DefaultGameClientLaunchGameComponent
{
    protected override string GetLaunchArgs(GameClientInstallation gameClientInstallation)
    {
        string args = gameClientInstallation.GetValue<string>(GameClientDataKey.Custom_LaunchArgs) ?? String.Empty;

        args = args.Replace("%game%", GameInstallation.InstallLocation.ToString());

        return args;
    }
}