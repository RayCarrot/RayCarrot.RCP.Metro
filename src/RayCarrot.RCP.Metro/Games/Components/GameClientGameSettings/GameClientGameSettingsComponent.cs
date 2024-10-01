using RayCarrot.RCP.Metro.Games.Settings;

namespace RayCarrot.RCP.Metro.Games.Components;

[GameComponentBase(SingleInstance = true)]
public class GameClientGameSettingsComponent : FactoryGameComponent<GameSettingsViewModel>
{
    public GameClientGameSettingsComponent(Func<GameInstallation, GameSettingsViewModel> objFactory) : base(objFactory) { }
}