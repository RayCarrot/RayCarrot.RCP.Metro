namespace RayCarrot.RCP.Metro.Games.Components;

public class UbiArtLaunchArgumentsComponent : LaunchArgumentsComponent
{
    public UbiArtLaunchArgumentsComponent() : base(GetLaunchArgs) { }

    private static string? GetLaunchArgs(GameInstallation gameInstallation)
    {
        string? commandArgs = gameInstallation.GetValue<string>(GameDataKey.UbiArt_CommandArgs);

        if (commandArgs == null)
            return null;

        // Prefix with a dummy value to escape any previous launch arguments. For example the Steam version
        // of Rayman Legends passes in the launch argument "-uplay_steam_mode" before this. So by setting the
        // dummy text we can get the game to parse it as "-uplay_steam_modedummy=1" and thus ignore it.
        return $"dummy=1;{commandArgs}";
    }
}