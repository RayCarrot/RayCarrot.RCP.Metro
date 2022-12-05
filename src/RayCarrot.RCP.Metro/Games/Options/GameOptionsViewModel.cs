namespace RayCarrot.RCP.Metro.Games.Options;

public abstract class GameOptionsViewModel : BaseViewModel
{
    protected GameOptionsViewModel(GameInstallation gameInstallation)
    {
        GameInstallation = gameInstallation;
    }

    public GameInstallation GameInstallation { get; }
}