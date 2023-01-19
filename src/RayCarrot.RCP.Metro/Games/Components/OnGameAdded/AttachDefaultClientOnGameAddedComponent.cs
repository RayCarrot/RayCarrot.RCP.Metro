using RayCarrot.RCP.Metro.Games.Clients;

namespace RayCarrot.RCP.Metro.Games.Components;

public class AttachDefaultClientOnGameAddedComponent : OnGameAddedComponent
{
    public AttachDefaultClientOnGameAddedComponent() : base(AttachDefaultClientAsync) { }

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    private static async Task AttachDefaultClientAsync(GameInstallation gameInstallation)
    {
        // Get the first available game client
        GameClientInstallation? gameClientInstallation = Services.GameClients.GetFirstAvailableGameClient(gameInstallation);

        // Return if none was found
        if (gameClientInstallation == null)
        {
            Logger.Trace("Failed to attach a default game client for {0} due to one not being found",
                gameInstallation.FullId);
            return;
        }

        await Services.GameClients.AttachGameClientAsync(gameInstallation, gameClientInstallation);
    }
}