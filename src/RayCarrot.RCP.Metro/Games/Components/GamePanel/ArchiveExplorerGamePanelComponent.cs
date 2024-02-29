using RayCarrot.RCP.Metro.Games.Panels;

namespace RayCarrot.RCP.Metro.Games.Components;

public class ArchiveExplorerGamePanelComponent : GamePanelComponent
{
    public ArchiveExplorerGamePanelComponent(ArchiveComponent archiveComponent) : base(x => new ArchiveGamePanelViewModel(x, archiveComponent))
    {
        Priority = 1;
    }
}