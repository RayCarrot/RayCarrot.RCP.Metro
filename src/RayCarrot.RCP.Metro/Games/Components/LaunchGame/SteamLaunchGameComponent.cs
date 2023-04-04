using System.Diagnostics;
using RayCarrot.RCP.Metro.Games.Structure;

namespace RayCarrot.RCP.Metro.Games.Components;

[RequiredGameComponents(typeof(SteamGameClientComponent))]
public class SteamLaunchGameComponent : LaunchGameComponent
{
    #region Logger

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    #endregion

    #region Private Methods

    private string GetSteamId() => GameInstallation.GetRequiredComponent<SteamGameClientComponent>().SteamId;

    #endregion

    #region Protected Methods

    protected override async Task<bool> LaunchImplAsync()
    {
        // Get the steam id
        string steamId = GetSteamId();

        Logger.Trace("The game {0} is launching with Steam ID {1}", GameInstallation.FullId, steamId);

        // Launch the game. The process this returns is Steam.exe.
        Process? process = await Services.File.LaunchFileAsync(SteamHelpers.GetGameLaunchURI(steamId));

        Logger.Info("The game {0} has been launched", GameInstallation.FullId);

        return process != null;
    }

    #endregion

    #region Public Methods

    public override void CreateShortcut(FileSystemPath shortcutName, FileSystemPath destinationDirectory)
    {
        Services.File.CreateURLShortcut(shortcutName, destinationDirectory, SteamHelpers.GetGameLaunchURI(GetSteamId()));

        Logger.Trace("An URL shortcut was created for {0} under {1}", GameInstallation.FullId, destinationDirectory);
    }

    public override IEnumerable<JumpListItemViewModel> GetJumpListItems() => new[]
    {
        new JumpListItemViewModel(
            gameInstallation: GameInstallation,
            name: GameInstallation.GetDisplayName(),
            iconSource: GameDescriptor.GetStructure<DirectoryProgramInstallationStructure>().GetAbsolutePath(GameInstallation, GameInstallationPathType.PrimaryExe),
            launchPath: SteamHelpers.GetGameLaunchURI(GetSteamId()),
            workingDirectory: null,
            launchArguments: null,
            id: GameInstallation.InstallationId)
    };

    #endregion
}