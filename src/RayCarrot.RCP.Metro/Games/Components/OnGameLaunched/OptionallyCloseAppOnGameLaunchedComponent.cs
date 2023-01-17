namespace RayCarrot.RCP.Metro.Games.Components;

public class OptionallyCloseAppOnGameLaunchedComponent : OnGameLaunchedComponent
{
    public OptionallyCloseAppOnGameLaunchedComponent() : base(OptionallyCloseAppAsync) { }

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    private static async Task OptionallyCloseAppAsync(GameInstallation gameInstallation)
    {
        // Check if the application should close
        if (Services.Data.App_CloseAppOnGameLaunch)
        {
            Logger.Info("Shutting down app after having launched game {0}", gameInstallation.FullId);
            await App.Current.ShutdownAppAsync(false);
        }
    }
}