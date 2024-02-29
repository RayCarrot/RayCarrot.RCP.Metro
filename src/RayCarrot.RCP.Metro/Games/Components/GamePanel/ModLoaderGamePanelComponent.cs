using RayCarrot.RCP.Metro.Games.Panels;

namespace RayCarrot.RCP.Metro.Games.Components;

[SingleInstanceGameComponent]
public class ModLoaderGamePanelComponent : GamePanelComponent
{
    public ModLoaderGamePanelComponent() : base(x => new ModLoaderGamePanelViewModel(x))
    {
        Priority = 0;
    }
}