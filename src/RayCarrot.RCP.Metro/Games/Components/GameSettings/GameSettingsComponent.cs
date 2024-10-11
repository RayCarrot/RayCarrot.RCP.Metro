using RayCarrot.RCP.Metro.Games.Settings;

namespace RayCarrot.RCP.Metro.Games.Components;

[GameComponentBase(SingleInstance = true)]
[GameFeature(nameof(Resources.GameHub_GameSettings), GenericIconKind.GameFeature_Settings)]
public class GameSettingsComponent : FactoryGameComponent<GameSettingsViewModel>
{
    public GameSettingsComponent(Func<GameInstallation, GameSettingsViewModel> objFactory) : base(objFactory) { }
}