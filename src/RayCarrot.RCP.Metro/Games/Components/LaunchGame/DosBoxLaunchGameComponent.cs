using System.Diagnostics;
using System.Text;
using RayCarrot.RCP.Metro.Games.Clients;
using RayCarrot.RCP.Metro.Games.Clients.Data;
using RayCarrot.RCP.Metro.Games.Clients.DosBox;

namespace RayCarrot.RCP.Metro.Games.Components;

public class DosBoxLaunchGameComponent : LaunchGameComponent
{
    #region Constructor

    public DosBoxLaunchGameComponent(DosBoxGameClientDescriptor gameClientDescriptor)
    {
        GameClientDescriptor = gameClientDescriptor;
    }

    #endregion

    #region Logger

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    #endregion

    #region Public Properties

    public override bool SupportsLaunchArguments => true;
    public DosBoxGameClientDescriptor GameClientDescriptor { get; }

    #endregion

    #region Private Methods

    private IEnumerable<DuoGridItemViewModel> GetGameInfoItems(GameInstallation gameInstallation)
    {
        GameClientInstallation gameClientInstallation = GetRequiredGameClientInstallation();

        // Get the launch info
        FileSystemPath launchPath = gameClientInstallation.InstallLocation.FilePath;
        string launchArgs = GetDOSBoxLaunchArgs();

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

    private string GetDOSBoxLaunchArgs(string? gameLaunchArgs = null)
    {
        // Create a string builder for the argument
        StringBuilder str = new();

        // Add config files
        foreach (FileSystemPath configFile in GetConfigFilePaths())
        {
            if (configFile.FileExists)
                str.Append($"-conf \"{configFile.FullPath}\" ");
        }

        // Add launch commands
        DosBoxLaunchCommandsComponent launchCommandsComponent = GameInstallation.GetRequiredComponent<DosBoxLaunchCommandsComponent>();
        foreach (string cmd in launchCommandsComponent.GetLaunchCommands())
        {
            // Add quotes if there's a space in the command
            if (cmd.Contains(" "))
                str.Append($"-c \"{cmd}\" ");
            else
                str.Append($"-c {cmd} ");
        }

        // Don't show the console
        str.Append("-noconsole");

        // Return the argument
        return str.ToString();
    }

    private IReadOnlyList<FileSystemPath> GetConfigFilePaths()
    {
        // Get the game client installation
        GameClientInstallation gameClientInstallation = GetRequiredGameClientInstallation();

        List<FileSystemPath> configFilePaths = new();

        // Get config files from the data
        var dataPaths = gameClientInstallation.GetObject<DosBoxConfigFilePaths>(GameClientDataKey.DosBox_ConfigFilePaths);
        if (dataPaths != null)
            configFilePaths.AddRange(dataPaths.FilePaths);

        // Get config files from the components
        configFilePaths.AddRange(GameInstallation.GetComponents<DosBoxConfigFileComponent>().CreateObjects());

        return configFilePaths;
    }

    #endregion

    #region Protected Methods

    protected override Task<bool> LaunchImplAsync() => LaunchGameAsync(null);

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
        string launchArgs = GetDOSBoxLaunchArgs();

        // Create the shortcut
        Services.File.CreateFileShortcut(shortcutName, destinationDirectory, launchPath, launchArgs);

        Logger.Trace("A shortcut was created for {0} under {1}", GameInstallation.FullId, destinationDirectory);
    }

    public override IEnumerable<JumpListItemViewModel> GetJumpListItems()
    {
        GameClientInstallation gameClientInstallation = GetRequiredGameClientInstallation();

        // Get the launch info
        FileSystemPath launchPath = gameClientInstallation.InstallLocation.FilePath;
        string launchArgs = GetDOSBoxLaunchArgs();

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

    public async Task<bool> LaunchGameAsync(string? gameLaunchArgs)
    {
        GameClientInstallation gameClientInstallation = GetRequiredGameClientInstallation();
        FileSystemPath launchPath = gameClientInstallation.InstallLocation.FilePath;

        // Make sure the DOSBox exe exists
        if (!launchPath.FileExists)
        {
            await Services.MessageUI.DisplayMessageAsync(Resources.GameClients_Launch_ClientNotFound, MessageType.Error);
            return false;
        }

        // Make sure the mount path exists
        FileSystemPath mountPath = GameInstallation.GetValue<FileSystemPath>(GameDataKey.Client_DosBox_MountPath);
        if (GameInstallation.HasComponent<MsDosGameRequiresDiscComponent>() && !mountPath.Exists)
        {
            await Services.MessageUI.DisplayMessageAsync(Resources.LaunchGame_MountPathNotFound, MessageType.Error);
            return false;
        }

        // Get the launch args
        string launchArgs = GetDOSBoxLaunchArgs(gameLaunchArgs);

        Logger.Trace("The game {0} launch info has been retrieved as Path = {1}, Args = {2}",
            GameInstallation.FullId, launchPath, launchArgs);

        // Launch the game
        Process? process = await Services.File.LaunchFileAsync(launchPath, arguments: launchArgs);

        Logger.Info("The game {0} has been launched", GameInstallation.FullId);

        return process != null;
    }

    #endregion
}