using RayCarrot.RCP.Metro.Games.Clients;

namespace RayCarrot.RCP.Metro;

public class SupportedGameViewModel : BaseViewModel
{
    public SupportedGameViewModel(GameInstallation gameInstallation, GameClientInstallation gameClientInstallation)
    {
        GameInstallation = gameInstallation;
        GameClientInstallation = gameClientInstallation;
    }

    public GameInstallation GameInstallation { get; }
    public GameClientInstallation GameClientInstallation { get; }

    public bool UsesGameClient
    {
        get => Services.GameClients.GetAttachedGameClient(GameInstallation) == GameClientInstallation;
        set
        {
            Invoke();
            async void Invoke()
            {
                if (value)
                    await Services.GameClients.AttachGameClientAsync(GameInstallation, GameClientInstallation);
                else
                    await Services.GameClients.DetachGameClientAsync(GameInstallation);
            }
        }
    }

    public void RefreshUsesGameClient() => OnPropertyChanged(nameof(UsesGameClient));
}