using RayCarrot.RCP.Metro.Games.Components;

namespace RayCarrot.RCP.Metro;

[GameComponent(IsBase = true)]
public class GameInfoComponent : FactoryGameComponent<IEnumerable<DuoGridItemViewModel>>
{
    public GameInfoComponent(Func<GameInstallation, IEnumerable<DuoGridItemViewModel>> objFactory) : base(objFactory) { }
}