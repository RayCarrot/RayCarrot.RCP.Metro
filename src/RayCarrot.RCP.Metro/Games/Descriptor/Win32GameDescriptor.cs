using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using NLog;

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
    public override bool SupportsGameLaunchMode => true;

    #endregion

    #region Protected Methods

    protected override async Task<GameLaunchResult> LaunchAsync(GameInstallation gameInstallation, bool forceRunAsAdmin)
    {
        // Get the launch info
        GameLaunchInfo launchInfo = GetLaunchInfo(gameInstallation);

        Logger.Trace("The game {0} launch info has been retrieved as Path = {1}, Args = {2}",
            Id, launchInfo.Path, launchInfo.Args);

        // Launch the game
        UserData_GameLaunchMode launchMode = gameInstallation.GetValue<UserData_GameLaunchMode>(GameDataKey.Win32LaunchMode);
        bool asAdmin = forceRunAsAdmin || launchMode == UserData_GameLaunchMode.AsAdmin;
        Process? process = await Services.File.LaunchFileAsync(launchInfo.Path, asAdmin, launchInfo.Args);

        Logger.Info("The game {0} has been launched", Id);

        return new GameLaunchResult(process, process != null);
    }

    /// <summary>
    /// Gets the launch info for the game
    /// </summary>
    /// <param name="gameInstallation">The game installation to get the launch info for</param>
    /// <returns>The launch info</returns>
    protected virtual GameLaunchInfo GetLaunchInfo(GameInstallation gameInstallation) // TODO-14: Don't override this. Instead GetPath and GetArgs.
    {
        string? launchArgs = GetLaunchArgs(gameInstallation);
        return new GameLaunchInfo(gameInstallation.InstallLocation + gameInstallation.GameDescriptor.DefaultFileName, launchArgs);
    }

    /// <summary>
    /// Gets the launch arguments for the game
    /// </summary>
    protected virtual string? GetLaunchArgs(GameInstallation gameInstallation) => null;

    /// <summary>
    /// Gets the icon resource path for the game based on its launch information
    /// </summary>
    /// <param name="gameInstallation">The game installation to get the icon for</param>
    /// <returns>The icon resource path</returns>
    protected virtual FileSystemPath GetIconResourcePath(GameInstallation gameInstallation) => GetLaunchInfo(gameInstallation).Path;

    #endregion

    #region Public Methods

    public override IEnumerable<DuoGridItemViewModel> GetGameInfoItems(GameInstallation gameInstallation)
    {
        // Get the launch info
        GameLaunchInfo launchInfo = GetLaunchInfo(gameInstallation);

        return base.GetGameInfoItems(gameInstallation).Concat(new[]
        {
            new DuoGridItemViewModel(
                header: new ResourceLocString(nameof(Resources.GameInfo_LaunchPath)),
                text: launchInfo.Path.FullPath,
                minUserLevel: UserLevel.Technical),
            new DuoGridItemViewModel(
                header: new ResourceLocString(nameof(Resources.GameInfo_LaunchArgs)),
                text: launchInfo.Args,
                minUserLevel: UserLevel.Technical)
        });
    }

    public override void CreateGameShortcut(GameInstallation gameInstallation, FileSystemPath shortcutName, FileSystemPath destinationDirectory)
    {
        // Get the launch info
        GameLaunchInfo launchInfo = GetLaunchInfo(gameInstallation);

        // Create the shortcut
        Services.File.CreateFileShortcut(shortcutName, destinationDirectory, launchInfo.Path, launchInfo.Args);

        Logger.Trace("A shortcut was created for {0} under {1}", Id, destinationDirectory);
    }

    public override IEnumerable<JumpListItemViewModel> GetJumpListItems(GameInstallation gameInstallation)
    {
        GameLaunchInfo launchInfo = GetLaunchInfo(gameInstallation);

        if (!launchInfo.Path.FileExists)
            return Enumerable.Empty<JumpListItemViewModel>();

        return new[]
        {
            new JumpListItemViewModel(
                name: gameInstallation.GameDescriptor.DisplayName,
                iconSource: GetIconResourcePath(gameInstallation),
                launchPath: launchInfo.Path,
                workingDirectory: launchInfo.Path.Parent,
                launchArguments: launchInfo.Args,
                // TODO-14: Use game ID instead
                id: gameInstallation.LegacyGame.ToString())
        };
    }

    public override async Task<FileSystemPath?> LocateAsync()
    {
        // Have user browse for directory
        DirectoryBrowserResult result = await Services.BrowseUI.BrowseDirectoryAsync(new DirectoryBrowserViewModel()
        {
            Title = Resources.LocateGame_BrowserHeader,
            DefaultDirectory = Environment.SpecialFolder.ProgramFilesX86.GetFolderPath(),
            MultiSelection = false
        });

        // Make sure the user did not cancel
        if (result.CanceledByUser)
            return null;

        // Make sure the selected directory exists
        if (!result.SelectedDirectory.DirectoryExists)
            return null;

        // Make sure the directory is valid
        if (!await IsValidAsync(result.SelectedDirectory))
        {
            Logger.Info("The selected install directory for {0} is not valid", Id);

            await Services.MessageUI.DisplayMessageAsync(Resources.LocateGame_InvalidLocation,
                Resources.LocateGame_InvalidLocationHeader, MessageType.Error);

            return null;
        }

        // Return the valid directory
        return result.SelectedDirectory;
    }

    #endregion

    #region Data Types

    /// <summary>
    /// Contains general launch information for a game
    /// </summary>
    protected record GameLaunchInfo(FileSystemPath Path, string? Args);

    #endregion
}