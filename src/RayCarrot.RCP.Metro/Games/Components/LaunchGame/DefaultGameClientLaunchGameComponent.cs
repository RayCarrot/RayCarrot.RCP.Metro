using System.Diagnostics;
using RayCarrot.RCP.Metro.Games.Clients;

namespace RayCarrot.RCP.Metro.Games.Components;

public class DefaultGameClientLaunchGameComponent : LaunchGameComponent
{
    #region Logger

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    #endregion

    #region Public Properties

    public override bool SupportsLaunchArguments => false;

    #endregion

    #region Private Methods

    private IEnumerable<DuoGridItemViewModel> GetGameInfoItems(GameInstallation gameInstallation)
    {
        GameClientInstallation gameClientInstallation = GetRequiredGameClientInstallation();

        // Get the launch info
        FileSystemPath launchPath = gameClientInstallation.InstallLocation.FilePath;
        string launchArgs = GetLaunchArgs(gameClientInstallation);

        return new[]
        {
            new DuoGridItemViewModel(
                header: new ResourceLocString(nameof(Resources.GameInfo_LaunchPath)),
                text: launchPath.FullPath,
                minUserLevel: UserLevel.Technical),
            new DuoGridItemViewModel(
                header: new ResourceLocString(nameof(Resources.GameInfo_LaunchArgs)),
                text: launchArgs,
                minUserLevel: UserLevel.Technical)
        };
    }

    #endregion

    #region Protected Methods

    protected virtual string GetLaunchArgs(GameClientInstallation gameClientInstallation)
    {
        return $"\"{GameInstallation.InstallLocation}\"";
    }

    protected override async Task<bool> LaunchImplAsync()
    {
        GameClientInstallation gameClientInstallation = GetRequiredGameClientInstallation();
        FileSystemPath launchPath = gameClientInstallation.InstallLocation.FilePath;

        // Make sure the game client exists
        if (!launchPath.FileExists)
        {
            await Services.MessageUI.DisplayMessageAsync(Resources.GameClients_Launch_ClientNotFound, MessageType.Error);
            return false;
        }

        // Get the launch args
        string launchArgs = GetLaunchArgs(gameClientInstallation);

        Logger.Trace("The game {0} launch info has been retrieved as Path = {1}, Args = {2}",
            GameInstallation.FullId, launchPath, launchArgs);

        // Launch the game
        Process? process = await Services.File.LaunchFileAsync(launchPath, arguments: launchArgs);

        Logger.Info("The game {0} has been launched", GameInstallation.FullId);

        return process != null;
    }

    #endregion

    #region Public Methods

    public override void RegisterComponents(IGameComponentBuilder builder)
    {
        base.RegisterComponents(builder);

        builder.Register(new GameInfoComponent(GetGameInfoItems));
    }

    public override void CreateShortcut(FileSystemPath shortcutName, FileSystemPath destinationDirectory)
    {
        GameClientInstallation gameClientInstallation = GetRequiredGameClientInstallation();

        // Get the launch info
        FileSystemPath launchPath = gameClientInstallation.InstallLocation.FilePath;
        string launchArgs = GetLaunchArgs(gameClientInstallation);

        // Create the shortcut
        Services.File.CreateFileShortcut(shortcutName, destinationDirectory, launchPath, launchArgs);

        Logger.Trace("A shortcut was created for {0} under {1}", GameInstallation.FullId, destinationDirectory);
    }

    public override IEnumerable<JumpListItemViewModel> GetJumpListItems()
    {
        GameClientInstallation gameClientInstallation = GetRequiredGameClientInstallation();

        // Get the launch info
        FileSystemPath launchPath = gameClientInstallation.InstallLocation.FilePath;
        string launchArgs = GetLaunchArgs(gameClientInstallation);

        if (!launchPath.FileExists)
            return Enumerable.Empty<JumpListItemViewModel>();

        return new[]
        {
            new JumpListItemViewModel(
                gameInstallation: GameInstallation,
                name: GameInstallation.GetDisplayName(),
                iconSource: gameClientInstallation.InstallLocation.FilePath,
                launchPath: launchPath,
                workingDirectory: launchPath.Parent,
                launchArguments: launchArgs,
                id: GameInstallation.InstallationId)
        };
    }

    #endregion
}