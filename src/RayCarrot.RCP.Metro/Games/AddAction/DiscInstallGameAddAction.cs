using System.Threading.Tasks;

namespace RayCarrot.RCP.Metro;

public class DiscInstallGameAddAction : GameAddAction
{
    public DiscInstallGameAddAction(GameDescriptor gameDescriptor, GameInstallerInfo installerInfo)
    {
        GameDescriptor = gameDescriptor;
        InstallerInfo = installerInfo;
    }

    public override LocalizedString Header => "Install from disc"; // TODO-UPDATE: Localize
    public override GenericIconKind Icon => GenericIconKind.GameAdd_DiscInstall;
    public override bool IsAvailable => true;

    public GameDescriptor GameDescriptor { get; }
    public GameInstallerInfo InstallerInfo { get; }

    public override async Task<GameInstallation?> AddGameAsync()
    {
        // Show and run the installer
        GameInstallerResult result = await Services.UI.InstallGameAsync(GameDescriptor, InstallerInfo);
        return result.GameInstallation;
    }
}