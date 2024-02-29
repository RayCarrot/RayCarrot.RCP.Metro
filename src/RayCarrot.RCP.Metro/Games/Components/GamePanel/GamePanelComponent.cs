using RayCarrot.RCP.Metro.Games.Panels;

namespace RayCarrot.RCP.Metro.Games.Components;

[BaseGameComponent]
public class GamePanelComponent : FactoryGameComponent<IEnumerable<GamePanelViewModel>>
{
    public GamePanelComponent(Func<GameInstallation, GamePanelViewModel> objFactory) : base(x => objFactory(x).Yield())
    { }

    public GamePanelComponent(Func<GameInstallation, IEnumerable<GamePanelViewModel>> objFactory) : base(objFactory)
    { }
}