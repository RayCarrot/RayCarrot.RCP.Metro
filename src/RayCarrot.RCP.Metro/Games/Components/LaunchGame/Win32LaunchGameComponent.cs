using System.Diagnostics;
using RayCarrot.RCP.Metro.Games.Options;

namespace RayCarrot.RCP.Metro.Games.Components;

/// <summary>
/// Provides functionality to a launch a Win32 game. The platform doesn't have to be <see cref="GamePlatform.Win32"/>,
/// but a <see cref="Win32LaunchPathComponent"/> is required. Optionally a <see cref="LaunchArgumentsComponent"/> can
/// also be used to specify launch arguments.
/// </summary>
[RequiredGameComponents(typeof(Win32LaunchPathComponent))]
public class Win32LaunchGameComponent : LaunchGameComponent
{
    #region Logger

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    #endregion

    #region Public Properties

    public override bool SupportsLaunchArguments => true;

    #endregion

    #region Private Methods

    private IEnumerable<ActionItemViewModel> GetAdditionalLaunchActions(GameInstallation gameInstallation)
    {
        // Add run as admin option
        bool runAsAdmin = gameInstallation.GetValue(GameDataKey.Win32_RunAsAdmin, false);

        // Don't include run as admin option if set to always do so
        if (runAsAdmin)
            return Enumerable.Empty<ActionItemViewModel>();

        return new[]
        {
            new IconCommandItemViewModel(
                header: Resources.GameDisplay_RunAsAdmin,
                description: null,
                iconKind: GenericIconKind.GameAction_RunAsAdmin,
                command: new AsyncRelayCommand(async () =>
                {
                    bool success = await LaunchAsync(gameInstallation, true);

                    if (success)
                        await gameInstallation.GetComponents<OnGameLaunchedComponent>().InvokeAllAsync();
                }))
        };
    }

    private IEnumerable<DuoGridItemViewModel> GetGameInfoItems(GameInstallation gameInstallation)
    {
        // Get the launch info
        FileSystemPath launchPath = GameInstallation.GetRequiredComponent<Win32LaunchPathComponent>().CreateObject();
        string? launchArgs = GameInstallation.GetComponent<LaunchArgumentsComponent>()?.CreateObject();

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

    private async Task<bool> LaunchAsync(GameInstallation gameInstallation, bool asAdmin)
    {
        // Get the launch info
        FileSystemPath launchPath = GameInstallation.GetRequiredComponent<Win32LaunchPathComponent>().CreateObject();
        string? launchArgs = GameInstallation.GetComponent<LaunchArgumentsComponent>()?.CreateObject();

        Logger.Trace("The game {0} launch info has been retrieved as Path = {1}, Args = {2}",
            gameInstallation.FullId, launchPath, launchArgs);

        // Launch the game
        Process? process = await Services.File.LaunchFileAsync(launchPath, asAdmin, launchArgs);

        Logger.Info("The game {0} has been launched", gameInstallation.FullId);

        return process != null;
    }

    #endregion

    #region Protected Methods

    protected override Task<bool> LaunchImplAsync()
    {
        bool runAsAdmin = GameInstallation.GetValue(GameDataKey.Win32_RunAsAdmin, false);
        return LaunchAsync(GameInstallation, runAsAdmin);
    }

    #endregion

    #region Public Methods

    public override void RegisterComponents(IGameComponentBuilder builder)
    {
        base.RegisterComponents(builder);

        // Register components for providing the "run as admin" option
        builder.Register(new GameOptionsComponent(x => new Win32GameOptionsViewModel(x)));
        builder.Register(new AdditionalLaunchActionsComponent(GetAdditionalLaunchActions));

        // Register components for showing launch info
        builder.Register(new GameInfoComponent(GetGameInfoItems));
    }

    public override void CreateShortcut(FileSystemPath shortcutName, FileSystemPath destinationDirectory)
    {
        // Get the launch info
        FileSystemPath launchPath = GameInstallation.GetRequiredComponent<Win32LaunchPathComponent>().CreateObject();
        string? launchArgs = GameInstallation.GetComponent<LaunchArgumentsComponent>()?.CreateObject();

        // Create the shortcut
        Services.File.CreateFileShortcut(shortcutName, destinationDirectory, launchPath, launchArgs);

        Logger.Trace("A shortcut was created for {0} under {1}", GameInstallation.FullId, destinationDirectory);
    }

    public override IEnumerable<JumpListItemViewModel> GetJumpListItems()
    {
        // Get the launch info
        FileSystemPath launchPath = GameInstallation.GetRequiredComponent<Win32LaunchPathComponent>().CreateObject();
        string? launchArgs = GameInstallation.GetComponent<LaunchArgumentsComponent>()?.CreateObject();

        if (!launchPath.FileExists)
            return Enumerable.Empty<JumpListItemViewModel>();

        return new[]
        {
            new JumpListItemViewModel(
                gameInstallation: GameInstallation,
                name: GameInstallation.GetDisplayName(),
                iconSource: GameInstallation.GetRequiredComponent<Win32LaunchPathComponent>().CreateObject(),
                launchPath: launchPath,
                workingDirectory: launchPath.Parent,
                launchArguments: launchArgs,
                id: GameInstallation.InstallationId)
        };
    }

    #endregion
}