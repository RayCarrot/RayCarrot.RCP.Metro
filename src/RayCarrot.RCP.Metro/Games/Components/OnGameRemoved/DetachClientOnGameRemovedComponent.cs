using RayCarrot.RCP.Metro.Games.Clients;

namespace RayCarrot.RCP.Metro.Games.Components;

public class DetachClientOnGameRemovedComponent : OnGameRemovedComponent
{
    public DetachClientOnGameRemovedComponent() : base(DetachClientAsync) { }

    private static async Task DetachClientAsync(GameInstallation gameInstallation)
    {
        // Get the previous game client installation and invoke it being deselected
        GameClientInstallation? prevClient = Services.GameClients.GetAttachedGameClient(gameInstallation);
        if (prevClient != null)
            await gameInstallation.GetComponents<OnGameClientDetachedComponent>().InvokeAllAsync(prevClient);
    }
}