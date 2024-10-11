using RayCarrot.RCP.Metro.Games.SetupGame;

namespace RayCarrot.RCP.Metro.Games.Components;

[GameComponentBase]
[GameFeature(nameof(Resources.SetupGame_Header), GenericIconKind.GameFeature_SetupGame)]
public class SetupGameActionComponent : FactoryGameComponent<SetupGameAction>
{
    public SetupGameActionComponent(Func<GameInstallation, SetupGameAction> objFactory) : base(objFactory) { }
}