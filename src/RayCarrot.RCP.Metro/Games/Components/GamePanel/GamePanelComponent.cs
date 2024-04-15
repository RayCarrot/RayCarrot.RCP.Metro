using RayCarrot.RCP.Metro.Games.Panels;

namespace RayCarrot.RCP.Metro.Games.Components;

[GameComponentBase]
public abstract class GamePanelComponent : FactoryGameComponent<IEnumerable<GamePanelViewModel>>
{
    protected GamePanelComponent(Func<GameInstallation, GamePanelViewModel> objFactory) : base(x => objFactory(x).Yield())
    { }

    protected GamePanelComponent(Func<GameInstallation, IEnumerable<GamePanelViewModel>> objFactory) : base(objFactory)
    { }
}