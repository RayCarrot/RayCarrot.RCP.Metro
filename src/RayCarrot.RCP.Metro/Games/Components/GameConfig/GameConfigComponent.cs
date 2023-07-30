using RayCarrot.RCP.Metro.Games.OptionsDialog;

namespace RayCarrot.RCP.Metro.Games.Components;

[BaseGameComponent]
[SingleInstanceGameComponent]
[GameFeature(nameof(Resources.GameOptions_Config), GenericIconKind.GameOptions_Config)]
public class GameConfigComponent : FactoryGameComponent<ConfigPageViewModel>
{
    public GameConfigComponent(Func<GameInstallation, ConfigPageViewModel> objFactory) : base(objFactory) { }
}