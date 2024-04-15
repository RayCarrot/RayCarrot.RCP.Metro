using RayCarrot.RCP.Metro.Games.OptionsDialog;

namespace RayCarrot.RCP.Metro.Games.Components;

[GameComponentBase(SingleInstance = true)]
[GameFeature(nameof(Resources.GameOptions_Config), GenericIconKind.GameFeature_Config)]
public class GameConfigComponent : FactoryGameComponent<ConfigPageViewModel>
{
    public GameConfigComponent(Func<GameInstallation, ConfigPageViewModel> objFactory) : base(objFactory) { }
}