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

            // TODO: Ideally we wouldn't send this since it causes a lot of things to be refreshed, such
            //       as progression, patches etc. But we need it for now to refresh the collection of
            //       additional launch actions.
            Services.Messenger.Send(new ModifiedGamesMessage(GameInstallation));
        }
    }
}