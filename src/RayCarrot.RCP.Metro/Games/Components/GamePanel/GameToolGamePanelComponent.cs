using RayCarrot.RCP.Metro.Games.Panels;

namespace RayCarrot.RCP.Metro.Games.Components;

public class GameToolGamePanelComponent : GamePanelComponent
{
    public GameToolGamePanelComponent(Func<GameInstallation, GamePanelViewModel> objFactory) : base(objFactory)
    {
        Priority = 3;
    }

    public GameToolGamePanelComponent(Func<GameInstallation, IEnumerable<GamePanelViewModel>> objFactory) : base(objFactory)
    {
        Priority = 3;
    }
}