using RayCarrot.RCP.Metro.Games.Panels;

namespace RayCarrot.RCP.Metro.Games.Components;

[GameComponentBase]
[GameFeature(nameof(Resources.GameTool_PerLevelSoundtrack), GenericIconKind.GameFeature_PerLevelSoundtrack)]
public class PerLevelSoundtrackComponent : GameComponent
{
    public override void RegisterComponents(IGameComponentBuilder builder)
    {
        base.RegisterComponents(builder);

        builder.Register<GamePanelComponent>(new GameToolGamePanelComponent(x => new PerLevelSoundtrackGamePanelViewModel(x)));
        builder.Register<DosBoxLaunchCommandsComponent, PerLevelSoundtrackDosBoxLaunchCommandsComponent>();
    }
}