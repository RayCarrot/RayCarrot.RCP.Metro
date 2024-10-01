using RayCarrot.RCP.Metro.Games.Settings;

namespace RayCarrot.RCP.Metro.Games.Components;

[GameComponentBase(SingleInstance = true)]
[GameFeature("Game settings", GenericIconKind.GameFeature_Settings)] // TODO-LOC
public class GameSettingsComponent : FactoryGameComponent<GameSettingsViewModel>
{
    public GameSettingsComponent(Func<GameInstallation, GameSettingsViewModel> objFactory) : base(objFactory) { }
}