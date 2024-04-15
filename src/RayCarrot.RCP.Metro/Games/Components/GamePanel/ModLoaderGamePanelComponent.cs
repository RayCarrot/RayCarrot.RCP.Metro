using RayCarrot.RCP.Metro.Games.Panels;

namespace RayCarrot.RCP.Metro.Games.Components;

[GameComponentInstance(SingleInstance = true)]
public class ModLoaderGamePanelComponent : GamePanelComponent
{
    public ModLoaderGamePanelComponent() : base(GetModLoaderGamePanelViewModels)
    {
        Priority = 0;
    }

    private static IEnumerable<ModLoaderGamePanelViewModel> GetModLoaderGamePanelViewModels(GameInstallation gameInstallation)
    {
        return new ModLoaderGamePanelViewModel[]
        {
            new(gameInstallation)
        };
    }
}