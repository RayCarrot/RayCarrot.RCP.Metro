using RayCarrot.RCP.Metro.Games.SetupGame;

namespace RayCarrot.RCP.Metro.Games.Components;

[GameComponentBase]
[GameFeature("Setup game", GenericIconKind.GameFeature_SetupGame)] // TODO-LOC
public class SetupGameActionComponent : FactoryGameComponent<SetupGameAction>
{
    public SetupGameActionComponent(Func<GameInstallation, SetupGameAction> objFactory) : base(objFactory) { }
}