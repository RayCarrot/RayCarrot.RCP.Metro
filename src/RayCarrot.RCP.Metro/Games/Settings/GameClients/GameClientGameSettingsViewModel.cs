using RayCarrot.RCP.Metro.Games.Clients;

namespace RayCarrot.RCP.Metro.Games.Settings;

public abstract class GameClientGameSettingsViewModel : GameSettingsViewModel
{
    protected GameClientGameSettingsViewModel(GameInstallation gameInstallation, GameClientInstallation gameClientInstallation) 
        : base(gameInstallation)
    {
        GameClientInstallation = gameClientInstallation;
    }

    public GameClientInstallation GameClientInstallation { get; }
}