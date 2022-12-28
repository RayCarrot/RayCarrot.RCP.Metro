namespace RayCarrot.RCP.Metro.Games.Clients;

public abstract class GameClientOptionsViewModel : BaseViewModel
{
    protected GameClientOptionsViewModel(GameClientInstallation gameClientInstallation)
    {
        GameClientInstallation = gameClientInstallation;
    }

    public GameClientInstallation GameClientInstallation { get; }
}