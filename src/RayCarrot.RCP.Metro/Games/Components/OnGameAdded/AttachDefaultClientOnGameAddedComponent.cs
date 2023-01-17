namespace RayCarrot.RCP.Metro.Games.Components;

public class AttachDefaultClientOnGameAddedComponent : OnGameAddedComponent
{
    public AttachDefaultClientOnGameAddedComponent() : base(AttachDefaultClientAsync) { }

    private static Task AttachDefaultClientAsync(GameInstallation gameInstallation)
    {
        return Services.GameClients.AttachDefaultGameClientAsync(gameInstallation);
    }
}