using RayCarrot.RCP.Metro.Games.Panels;

namespace RayCarrot.RCP.Metro.Games.Components;

[GameComponentBase]
[GameFeature("Per-level Soundtrack", GenericIconKind.GameFeature_PerLevelSoundtrack)] // TODO-LOC
public class PerLevelSoundtrackComponent : GameComponent
{
    public override void RegisterComponents(IGameComponentBuilder builder)
    {
        base.RegisterComponents(builder);

        builder.Register<GamePanelComponent>(new GameToolGamePanelComponent(x => new PerLevelSoundtrackGamePanelViewModel(x)));
        builder.Register<DosBoxLaunchCommandsComponent, PerLevelSoundtrackDosBoxLaunchCommandsComponent>();
    }
}