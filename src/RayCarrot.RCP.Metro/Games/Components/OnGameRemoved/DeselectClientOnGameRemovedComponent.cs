using RayCarrot.RCP.Metro.Games.Emulators;

namespace RayCarrot.RCP.Metro.Games.Components;

public class DeselectClientOnGameRemovedComponent : OnGameRemovedComponent
{
    public DeselectClientOnGameRemovedComponent() : base(DeselectClientAsync) { }

    private static async Task DeselectClientAsync(GameInstallation gameInstallation)
    {
        // Get the previous emulator installation and invoke it being deselected
        EmulatorInstallation? prevEmulatorInstallation = gameInstallation.GameDescriptor.GetGameClient(gameInstallation);
        if (prevEmulatorInstallation != null)
            await prevEmulatorInstallation.EmulatorDescriptor.OnEmulatorDeselectedAsync(gameInstallation, prevEmulatorInstallation);
    }
}