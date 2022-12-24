namespace RayCarrot.RCP.Metro.Games.Components;

public class DefaultToRunAsAdminOnGameAddedComponent : OnGameAddedComponent
{
    public DefaultToRunAsAdminOnGameAddedComponent() : base(DefaultToRunAsAdmin) { }

    private static void DefaultToRunAsAdmin(GameInstallation gameInstallation)
    {
        gameInstallation.SetValue(GameDataKey.Win32_RunAsAdmin, true);
    }
}