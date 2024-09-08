using RayCarrot.RCP.Metro.Games.SetupGame;

namespace RayCarrot.RCP.Metro.Games.Components;

[GameComponentBase]
public class SetupGameActionComponent : FactoryGameComponent<SetupGameAction>
{
    public SetupGameActionComponent(Func<GameInstallation, SetupGameAction> objFactory) : base(objFactory) { }
}