using System.Diagnostics;

namespace RayCarrot.RCP.Metro;

public class SteamLaunchGameComponent : LaunchGameComponent
{
    #region Constructor

    public SteamLaunchGameComponent(string steamId)
    {
        SteamId = steamId;
    }

    #endregion

    #region Logger

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    #endregion

    #region Public Properties

    /// <summary>
    /// The Steam product id
    /// </summary>
    public string SteamId { get; }

    #endregion

    #region Protected Methods

    protected override async Task<bool> LaunchImplAsync()
    {
        Logger.Trace("The game {0} is launching with Steam ID {1}", GameInstallation.FullId, SteamId);

        // TODO-14: Does this return the Steam/game process or just explorer.exe?
        // Launch the game
        Process? process = await Services.File.LaunchFileAsync(SteamHelpers.GetStorePageURL(SteamId));

        Logger.Info("The game {0} has been launched", GameInstallation.FullId);

        return process != null;
    }

    #endregion

    #region Public Methods

    public override void CreateShortcut(FileSystemPath shortcutName, FileSystemPath destinationDirectory)
    {
        Services.File.CreateURLShortcut(shortcutName, destinationDirectory, SteamHelpers.GetGameLaunchURI(SteamId));

        Logger.Trace("An URL shortcut was created for {0} under {1}", GameInstallation.FullId, destinationDirectory);
    }

    public override IEnumerable<JumpListItemViewModel> GetJumpListItems() => new[]
    {
        new JumpListItemViewModel(
            gameInstallation: GameInstallation,
            name: GameInstallation.GetDisplayName(),
            iconSource: GameInstallation.InstallLocation + GameInstallation.GameDescriptor.DefaultFileName,
            launchPath: SteamHelpers.GetGameLaunchURI(SteamId),
            workingDirectory: null,
            launchArguments: null,
            id: GameInstallation.InstallationId)
    };

    #endregion
}