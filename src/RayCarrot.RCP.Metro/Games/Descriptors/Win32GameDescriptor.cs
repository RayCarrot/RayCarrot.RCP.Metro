using System.Diagnostics;
using RayCarrot.RCP.Metro.Games.Components;
using RayCarrot.RCP.Metro.Games.Options;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// A game descriptor for a Win32 game
/// </summary>
public abstract class Win32GameDescriptor : GameDescriptor
{
    #region Logger

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    #endregion

    #region Public Properties

    public override GamePlatform Platform => GamePlatform.Win32;

    #endregion

    #region Private Methods

    private async Task<bool> LaunchAsync(GameInstallation gameInstallation, bool asAdmin)
    {
        // Get the launch info
        FileSystemPath launchPath = GetLaunchFilePath(gameInstallation);
        string? launchArgs = GetLaunchArgs(gameInstallation);

        Logger.Trace("The game {0} launch info has been retrieved as Path = {1}, Args = {2}",
            GameId, launchPath, launchArgs);

        // Launch the game
        Process? process = await Services.File.LaunchFileAsync(launchPath, asAdmin, launchArgs);

        Logger.Info("The game {0} has been launched", GameId);

        return process != null;
    }

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
                        await PostLaunchAsync();
                }))
        };
    }

    #endregion

    #region Protected Methods

    protected override void RegisterComponents(DescriptorComponentBuilder builder)
    {
        base.RegisterComponents(builder);

        builder.Register(new GameOptionsComponent(x => new Win32GameOptionsViewModel(x)));
        builder.Register(new AdditionalLaunchActionsComponent(GetAdditionalLaunchActions));
    }

    protected override Task<bool> LaunchAsync(GameInstallation gameInstallation)
    {
        bool runAsAdmin = gameInstallation.GetValue(GameDataKey.Win32_RunAsAdmin, false);
        return LaunchAsync(gameInstallation, runAsAdmin);
    }

    /// <summary>
    /// Gets the launch file path for the game
    /// </summary>
    /// <param name="gameInstallation">The game installation to get the launch file path for</param>
    /// <returns>The launch file path</returns>
    protected virtual FileSystemPath GetLaunchFilePath(GameInstallation gameInstallation) =>
        gameInstallation.InstallLocation + gameInstallation.GameDescriptor.DefaultFileName;

    /// <summary>
    /// Gets the launch arguments for the game
    /// </summary>
    protected virtual string? GetLaunchArgs(GameInstallation gameInstallation) => null;

    /// <summary>
    /// Gets the icon resource path for the game based on its launch information
    /// </summary>
    /// <param name="gameInstallation">The game installation to get the icon for</param>
    /// <returns>The icon resource path</returns>
    protected virtual FileSystemPath GetIconResourcePath(GameInstallation gameInstallation) => GetLaunchFilePath(gameInstallation);

    #endregion

    #region Public Methods

    public override IEnumerable<GameAddAction> GetAddActions() => new GameAddAction[]
    {
        new LocateGameAddAction(this),
    };

    public override IEnumerable<DuoGridItemViewModel> GetGameInfoItems(GameInstallation gameInstallation)
    {
        // Get the launch info
        FileSystemPath launchPath = GetLaunchFilePath(gameInstallation);
        string? launchArgs = GetLaunchArgs(gameInstallation);

        return base.GetGameInfoItems(gameInstallation).Concat(new[]
        {
            new DuoGridItemViewModel(
                header: new ResourceLocString(nameof(Resources.GameInfo_LaunchPath)),
                text: launchPath.FullPath,
                minUserLevel: UserLevel.Technical),
            new DuoGridItemViewModel(
                header: new ResourceLocString(nameof(Resources.GameInfo_LaunchArgs)),
                text: launchArgs,
                minUserLevel: UserLevel.Technical)
        });
    }

    public override void CreateGameShortcut(GameInstallation gameInstallation, FileSystemPath shortcutName, FileSystemPath destinationDirectory)
    {
        // Get the launch info
        FileSystemPath launchPath = GetLaunchFilePath(gameInstallation);
        string? launchArgs = GetLaunchArgs(gameInstallation);

        // Create the shortcut
        Services.File.CreateFileShortcut(shortcutName, destinationDirectory, launchPath, launchArgs);

        Logger.Trace("A shortcut was created for {0} under {1}", GameId, destinationDirectory);
    }

    public override IEnumerable<JumpListItemViewModel> GetJumpListItems(GameInstallation gameInstallation)
    {
        // Get the launch info
        FileSystemPath launchPath = GetLaunchFilePath(gameInstallation);
        string? launchArgs = GetLaunchArgs(gameInstallation);

        if (!launchPath.FileExists)
            return Enumerable.Empty<JumpListItemViewModel>();

        return new[]
        {
            new JumpListItemViewModel(
                name: gameInstallation.GetDisplayName(),
                iconSource: GetIconResourcePath(gameInstallation),
                launchPath: launchPath,
                workingDirectory: launchPath.Parent,
                launchArguments: launchArgs,
                // TODO-14: Use game ID instead
                id: gameInstallation.LegacyGame.ToString())
        };
    }

    #endregion
}