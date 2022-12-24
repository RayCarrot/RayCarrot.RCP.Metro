using RayCarrot.RCP.Metro.Games.Options;

namespace RayCarrot.RCP.Metro.Games.Components;

[GameComponent]
public class GameOptionsComponent : FactoryGameComponent<GameOptionsViewModel>
{
    public GameOptionsComponent(Func<GameInstallation, GameOptionsViewModel> objFactory) : base(objFactory) 
    { }
}