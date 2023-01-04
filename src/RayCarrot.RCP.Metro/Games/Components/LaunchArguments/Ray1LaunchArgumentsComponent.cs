using RayCarrot.RCP.Metro.Games.Data;

namespace RayCarrot.RCP.Metro.Games.Components;

public class Ray1LaunchArgumentsComponent : LaunchArgumentsComponent
{
    public Ray1LaunchArgumentsComponent() : base(GetLaunchArgs) { }

    private static string GetLaunchArgs(GameInstallation gameInstallation)
    {
        string version = gameInstallation.GetRequiredObject<Ray1MsDosData>(GameDataKey.Ray1_MsDosData).SelectedVersion;
        return GetLaunchArgs(version);
    }

    public static string GetLaunchArgs(string version) => $"ver={version}";
}