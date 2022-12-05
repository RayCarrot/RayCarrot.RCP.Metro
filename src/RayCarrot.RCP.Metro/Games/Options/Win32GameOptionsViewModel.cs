using CommunityToolkit.Mvvm.Messaging;

namespace RayCarrot.RCP.Metro.Games.Options;

public class Win32GameOptionsViewModel : GameOptionsViewModel
{
    public Win32GameOptionsViewModel(GameInstallation gameInstallation) : base(gameInstallation)
    {

    }

    public bool RunAsAdmin
    {
        get => GameInstallation.GetValue(GameDataKey.Win32_RunAsAdmin, false);
        set
        {
            GameInstallation.SetValue(GameDataKey.Win32_RunAsAdmin, value);
            Services.Messenger.Send(new ModifiedGamesMessage(GameInstallation));
        }
    }
}