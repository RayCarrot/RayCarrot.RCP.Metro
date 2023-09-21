using System.Windows.Input;

namespace RayCarrot.RCP.Metro.ModLoader.Sources.GameBanana;

public class GameBananaModPanelFooterViewModel : ModPanelFooterViewModel
{
    public GameBananaModPanelFooterViewModel(long gameBananaId)
    {
        GameBananaId = gameBananaId;
        
        OpenInGameBananaCommand = new RelayCommand(OpenInGameBanana);
    }

    public ICommand OpenInGameBananaCommand { get; }

    public long GameBananaId { get; }

    public void OpenInGameBanana()
    {
        Services.App.OpenUrl($"https://gamebanana.com/mods/{GameBananaId}");
    }
}