using RayCarrot.RCP.Metro.Games.Panels;

namespace RayCarrot.RCP.Metro.Games.Components;

public class ModLoaderGamePanelComponent : GamePanelComponent
{
    public ModLoaderGamePanelComponent() : base(GetModLoaderGamePanelViewModels)
    {
        Priority = 0;
    }

    private static IEnumerable<ModLoaderGamePanelViewModel> GetModLoaderGamePanelViewModels(GameInstallation gameInstallation)
    {
        // Only create the panel if we have mod modules registered
        if (gameInstallation.GetComponents<ModModuleComponent>().Any())
            yield return new ModLoaderGamePanelViewModel(gameInstallation);
    }
}