using RayCarrot.RCP.Metro.Games.SetupGame;

namespace RayCarrot.RCP.Metro.Games.Components;

[GameComponentBase(SingleInstance = true)]
public class MsDosGameRequiresDiscComponent : GameComponent
{
    public override void RegisterComponents(IGameComponentBuilder builder)
    {
        builder.Register(new SetupGameActionComponent(_ => new MsDosGameRequiresDiscSetupGameAction()));
    }
}