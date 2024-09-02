using RayCarrot.RCP.Metro.Games.SetupGame;

namespace RayCarrot.RCP.Metro.Games.Components;

[GameComponentBase]
public class SetupGameManagerComponent : FactoryGameComponent<SetupGameManager>
{
    public SetupGameManagerComponent(Func<GameInstallation, SetupGameManager> objFactory) : base(objFactory) { }
}