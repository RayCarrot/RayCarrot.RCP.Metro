namespace RayCarrot.RCP.Metro.Games.Components;

public class DefaultToRunAsAdminOnGameAddedComponent : OnGameAddedComponent
{
    public DefaultToRunAsAdminOnGameAddedComponent() : base(DefaultToRunAsAdmin) { }

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    private static void DefaultToRunAsAdmin(GameInstallation gameInstallation)
    {
        gameInstallation.SetValue(GameDataKey.Win32_RunAsAdmin, true);
        Logger.Info("The game {0} has been defaulted to run as admin", gameInstallation.FullId);
    }
}