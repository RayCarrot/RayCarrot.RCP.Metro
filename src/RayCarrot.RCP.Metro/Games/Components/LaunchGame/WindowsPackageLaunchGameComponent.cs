using Windows.ApplicationModel;
using Windows.ApplicationModel.Core;

namespace RayCarrot.RCP.Metro;

[RequiredGameComponents(typeof(WindowsPackageComponent))]
public class WindowsPackageLaunchGameComponent : LaunchGameComponent
{
    #region Logger

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    #endregion

    #region Protected Methods

    protected override async Task<bool> LaunchImplAsync()
    {
        try
        {
            WindowsPackageComponent packageComponent = GameInstallation.GetRequiredComponent<WindowsPackageComponent>();
            Package? package = packageComponent.GetPackage();

            if (package == null)
            {
                Logger.Warn("The game {0} could not launch due to the package not being found", GameInstallation.FullId);
                return false;
            }

            // Launch the first app entry for the package
            AppListEntry mainEntry = (await package.GetAppListEntriesAsync()).First();
            bool success = await mainEntry.LaunchAsync();

            if (success)
                Logger.Info("The game {0} has been launched", GameInstallation.FullId);
            else
                Logger.Warn("The game {0} failed to launch", GameInstallation.FullId);
            
            return success;
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Launching Windows package application");
            await Services.MessageUI.DisplayExceptionMessageAsync(ex, String.Format(Resources.LaunchGame_WinStoreError, GameInstallation.GetDisplayName()));

            return false;
        }
    }

    #endregion

    #region Public Methods

    public override void CreateShortcut(FileSystemPath shortcutName, FileSystemPath destinationDirectory)
    {
        WindowsPackageComponent packageComponent = GameInstallation.GetRequiredComponent<WindowsPackageComponent>();
        
        // Create the shortcut
        Services.File.CreateFileShortcut(shortcutName, destinationDirectory, packageComponent.LegacyLaunchPath);

        Logger.Trace("A shortcut was created for {0} under {1}", GameInstallation.FullId, destinationDirectory);
    }

    public override IEnumerable<JumpListItemViewModel> GetJumpListItems()
    {
        WindowsPackageComponent packageComponent = GameInstallation.GetRequiredComponent<WindowsPackageComponent>();

        return new[]
        {
            new JumpListItemViewModel(
                gameInstallation: GameInstallation,
                name: GameInstallation.GetDisplayName(),
                iconSource: packageComponent.LegacyLaunchPath,
                launchPath: packageComponent.LegacyLaunchPath,
                workingDirectory: null,
                launchArguments: null,
                id: GameInstallation.InstallationId)
        };
    }

    #endregion
}