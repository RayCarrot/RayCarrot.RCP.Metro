namespace RayCarrot.RCP.Metro.Games.Components;

public class OptionallyCloseAppOnGameLaunchedComponent : OnGameLaunchedComponent
{
    public OptionallyCloseAppOnGameLaunchedComponent() : base(OptionallyCloseAppAsync) { }

    private static async Task OptionallyCloseAppAsync(GameInstallation gameInstallation)
    {
        // Check if the application should close
        if (Services.Data.App_CloseAppOnGameLaunch)
            await App.Current.ShutdownAppAsync(false);
    }
}