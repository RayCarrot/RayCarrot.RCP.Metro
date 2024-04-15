using RayCarrot.RCP.Metro.Games.Panels;

namespace RayCarrot.RCP.Metro.Games.Components;

// TODO-LOC
[GameComponentBase]
[GameFeature("Prototype Restoration", GenericIconKind.GameFeature_PrototypeRestoration)]
public class PrototypeRestorationComponent : GameComponent
{
    public override void RegisterComponents(IGameComponentBuilder builder)
    {
        base.RegisterComponents(builder);

        builder.Register<GamePanelComponent>(new GameToolGamePanelComponent(x => new PrototypeRestorationGamePanelViewModel(x)));
    }
}