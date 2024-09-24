using System.Diagnostics;
using System.Text;
using RayCarrot.RCP.Metro.Games.Clients;
using RayCarrot.RCP.Metro.Games.Clients.Data;
using RayCarrot.RCP.Metro.Games.Clients.DosBox;
using RayCarrot.RCP.Metro.Games.Structure;

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
        GameClientInstallation gameClientInstallation = GetRequiredGameClientInstallation();

        string exeFileName = GameDescriptor.GetStructure<DirectoryProgramInstallationStructure>().FileSystem.GetLocalPath(ProgramPathType.PrimaryExe);
        string? gameArgs = gameLaunchArgs ?? GameInstallation.GetComponent<LaunchArgumentsComponent>()?.CreateObject();
        string launchName = gameArgs == null ? exeFileName : $"{exeFileName} {gameArgs}";

        FileSystemPath mountPath = GameInstallation.GetValue<FileSystemPath>(GameDataKey.Client_DosBox_MountPath);

        List<FileSystemPath> configFilePaths = new();

        // Get config files from the data
        var dataPaths = gameClientInstallation.GetObject<DosBoxConfigFilePaths>(GameClientDataKey.DosBox_ConfigFilePaths);
        if (dataPaths != null)
            configFilePaths.AddRange(dataPaths.FilePaths);

        // Get config files from the components
        configFilePaths.AddRange(GameInstallation.GetComponents<DosBoxConfigFileComponent>().CreateObjects());

        return GetDOSBoxLaunchArgs(
            mountPath: mountPath,
            launchName: launchName,
            installDir: GameInstallation.InstallLocation.Directory,
            dosBoxConfigFiles: configFilePaths);
    }

    private static string GetDOSBoxLaunchArgs(
        FileSystemPath mountPath,
        string launchName,
        FileSystemPath installDir,
        IEnumerable<FileSystemPath> dosBoxConfigFiles)
    {
        // Create a string builder for the argument
        var str = new StringBuilder();

        // Helper method for adding an argument
        void AddArg(string arg)
        {
            str.Append($"{arg} ");
        }

        // Add config files
        foreach (FileSystemPath configFile in dosBoxConfigFiles)
        {
            if (configFile.FileExists)
                AddArg($"-conf \"{configFile.FullPath}\"");
        }

        // Mount the disc if the path exists
        if (mountPath.Exists)
        {
            // The mounting differs if it's a physical disc vs. a disc image
            if (mountPath.IsDirectoryRoot)
                AddArg($"-c \"mount d {mountPath.FullPath} -t cdrom\"");
            else
                AddArg($"-c \"imgmount d '{mountPath.FullPath}' -t iso -fs iso\"");
        }

        // Mount the game install directory as the C drive
        AddArg($"-c \"MOUNT C '{installDir.FullPath}'\"");

        // Add commands to launch the game
        AddArg("-c C:");
        AddArg($"-c \"{launchName}\"");
        AddArg("-noconsole");
        AddArg("-c exit");

        // Return the argument
        return str.ToString().Trim();
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