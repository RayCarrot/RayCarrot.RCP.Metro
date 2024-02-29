using RayCarrot.RCP.Metro.Games.Panels;

namespace RayCarrot.RCP.Metro.Games.Components;

public class ProgressionGamePanelComponent : GamePanelComponent
{
    public ProgressionGamePanelComponent(ProgressionManagersComponent progressionManagersComponent) 
        : base(x => progressionManagersComponent.CreateObject().Select(p => new ProgressionGamePanelViewModel(x, p)))
    {
        Priority = 2;
    }
}