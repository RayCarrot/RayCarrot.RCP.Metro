using RayCarrot.RCP.Metro.Games.Clients;

namespace RayCarrot.RCP.Metro.Games.Components;

public class DeselectClientOnGameRemovedComponent : OnGameRemovedComponent
{
    public DeselectClientOnGameRemovedComponent() : base(DeselectClientAsync) { }

    private static async Task DeselectClientAsync(GameInstallation gameInstallation)
    {
        // Get the previous game client installation and invoke it being deselected
        GameClientInstallation? prevClient = gameInstallation.GameDescriptor.GetAttachedGameClient(gameInstallation);
        if (prevClient != null)
            await prevClient.GameClientDescriptor.OnGameClientDetachedAsync(gameInstallation, prevClient);
    }
}