#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NLog;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// Base for a Win32 Rayman Control Panel game
/// </summary>
public abstract class GameManager_Win32 : GameManager
{
    #region Logger

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    #endregion

    #region Public Override Properties

    /// <summary>
    /// The game type
    /// </summary>
    public override GameType Type => GameType.Win32;

    /// <summary>
    /// The display name for the game type
    /// </summary>
    public override LocalizedString GameTypeDisplayName => new ResourceLocString(nameof(Resources.GameType_Desktop));

    /// <summary>
    /// Indicates if using <see cref="UserData_GameLaunchMode"/> is supported
    /// </summary>
    public override bool SupportsGameLaunchMode => true;

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

    #endregion

    #region Protected Virtual Properties

    /// <summary>
    /// Gets the icon resource path for the game based on its launch information
    /// </summary>
    /// <returns>The icon resource path</returns>
    protected virtual FileSystemPath IconResourcePath => GetLaunchInfo(Game.GetInstallation()).Path;

    #endregion

    #region Public Virtual Properties

    /// <summary>
    /// Gets the launch arguments for the game
    /// </summary>
    public virtual string GetLaunchArgs => null;

    #endregion

    #region Protected Override Methods

    /// <summary>
    /// The implementation for launching the game
    /// </summary>
    /// <param name="forceRunAsAdmin">Indicated if the game should be forced to run as admin</param>
    /// <returns>The launch result</returns>
    protected override async Task<GameLaunchResult> LaunchAsync(bool forceRunAsAdmin)
    {
        // Get the launch info
        GameLaunchInfo launchInfo = GetLaunchInfo(Game.GetInstallation());

        Logger.Trace("The game {0} launch info has been retrieved as Path = {1}, Args = {2}", Game, launchInfo.Path, launchInfo.Args);

        // Launch the game
        var launchMode = Game.GetInstallation().GetValue<UserData_GameLaunchMode>(GameDataKey.Win32LaunchMode);
        var process = await Services.File.LaunchFileAsync(launchInfo.Path, forceRunAsAdmin || launchMode == UserData_GameLaunchMode.AsAdmin, launchInfo.Args);

        Logger.Info("The game {0} has been launched", Game);

        return new GameLaunchResult(process, process != null);
    }

    #endregion

    #region Public Override Methods

    /// <summary>
    /// Locates the game
    /// </summary>
    /// <returns>Null if the game was not found. Otherwise a valid or empty path for the install directory</returns>
    public override async Task<FileSystemPath?> LocateAsync()
    {
        // Have user browse for directory
        var result = await Services.BrowseUI.BrowseDirectoryAsync(new DirectoryBrowserViewModel()
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
            Logger.Info("The selected install directory for {0} is not valid", Game);

            await Services.MessageUI.DisplayMessageAsync(Resources.LocateGame_InvalidLocation,
                Resources.LocateGame_InvalidLocationHeader, MessageType.Error);

            return null;
        }

        // Return the valid directory
        return result.SelectedDirectory;
    }

    public override IEnumerable<JumpListItemViewModel> GetJumpListItems(GameInstallation gameInstallation)
    {
        GameLaunchInfo launchInfo = GetLaunchInfo(gameInstallation);

        if (launchInfo.Path.FileExists)
        {
            return new[]
            {
                new JumpListItemViewModel(
                    name: gameInstallation.GameInfo.DisplayName, 
                    iconSource: IconResourcePath, 
                    launchPath: launchInfo.Path, 
                    workingDirectory: launchInfo.Path.Parent, 
                    launchArguments: launchInfo.Args, 
                    // TODO-14: Use game ID instead
                    id: gameInstallation.Game.ToString())
            };
        }
        else
        {
            return Enumerable.Empty<JumpListItemViewModel>();
        }
    }

    /// <summary>
    /// Creates a shortcut to launch the game from
    /// </summary>
    /// <param name="shortcutName">The name of the shortcut</param>
    /// <param name="destinationDirectory">The destination directory for the shortcut</param>
    public override void CreateGameShortcut(FileSystemPath shortcutName, FileSystemPath destinationDirectory)
    {
        // Get the launch info
        var launchInfo = GetLaunchInfo(Game.GetInstallation());

        // Create the shortcut
        Services.File.CreateFileShortcut(shortcutName, destinationDirectory, launchInfo.Path, launchInfo.Args);

        Logger.Trace("A shortcut was created for {0} under {1}", Game, destinationDirectory);
    }

    #endregion

    #region Public Virtual Methods

    /// <summary>
    /// Gets the launch info for the game
    /// </summary>
    /// <param name="gameInstallation">The game installation to get the launch info for</param>
    /// <returns>The launch info</returns>
    public virtual GameLaunchInfo GetLaunchInfo(GameInstallation gameInstallation)
    {
        return new GameLaunchInfo(gameInstallation.InstallLocation + gameInstallation.GameInfo.DefaultFileName, GetLaunchArgs);
    }

    #endregion

    #region Data Types

    /// <summary>
    /// Contains general launch information for a game
    /// </summary>
    public record GameLaunchInfo(FileSystemPath Path, string Args);

    #endregion
}