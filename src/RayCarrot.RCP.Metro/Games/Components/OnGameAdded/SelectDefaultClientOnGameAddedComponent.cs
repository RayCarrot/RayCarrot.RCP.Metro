namespace RayCarrot.RCP.Metro.Games.Components;

public class SelectDefaultClientOnGameAddedComponent : OnGameAddedComponent
{
    public SelectDefaultClientOnGameAddedComponent() : base(SelectDefaultClientAsync) { }

    private static Task SelectDefaultClientAsync(GameInstallation gameInstallation)
    {
        return gameInstallation.GameDescriptor.SetGameClientAsync(gameInstallation, null);
    }
}