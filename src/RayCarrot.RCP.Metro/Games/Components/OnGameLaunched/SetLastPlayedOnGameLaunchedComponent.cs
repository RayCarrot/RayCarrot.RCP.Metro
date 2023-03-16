namespace RayCarrot.RCP.Metro.Games.Components;

public class SetLastPlayedOnGameLaunchedComponent : OnGameLaunchedComponent
{
    public SetLastPlayedOnGameLaunchedComponent() : base(SetLastPlayed) { }

    private static void SetLastPlayed(GameInstallation gameInstallation)
    {
        gameInstallation.SetValue(GameDataKey.RCP_LastPlayed, DateTime.Now);
    }
}