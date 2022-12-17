using System.Diagnostics;
using System.Text;

namespace RayCarrot.RCP.Metro.Games.Emulators.DosBox;

public sealed class DosBoxEmulatorDescriptor : EmulatorDescriptor
{
    #region Logger

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    #endregion

    #region Public Properties

    public override string EmulatorId => "DOSBox";
    public override GamePlatform[] SupportedPlatforms => new[] { GamePlatform.MsDos };
    public override LocalizedString DisplayName => new ResourceLocString(nameof(Resources.GameType_DosBox));
    public override EmulatorIconAsset Icon => EmulatorIconAsset.DosBox;

    #endregion

    #region Private Methods

    private MsDosGameDescriptor GetMsdosGameDescriptor(GameInstallation gameInstallation) =>
        gameInstallation.GameDescriptor as MsDosGameDescriptor 
        ?? throw new InvalidOperationException($"The {nameof(DosBoxEmulatorDescriptor)} can only be used with {nameof(MsDosGameDescriptor)}");

    private string GetDOSBoxLaunchArgs(GameInstallation gameInstallation, EmulatorInstallation emulatorInstallation, string? gameLaunchArgs = null)
    {
        MsDosGameDescriptor descriptor = GetMsdosGameDescriptor(gameInstallation);

        string? gameArgs = gameLaunchArgs ?? descriptor.GetLaunchArgs(gameInstallation);
        string launchName = gameArgs == null ? descriptor.DefaultFileName : $"{descriptor.DefaultFileName} {gameArgs}";

        FileSystemPath mountPath = gameInstallation.GetValue<FileSystemPath>(GameDataKey.Emu_DosBox_MountPath);
        return GetDOSBoxLaunchArgs(
            mountPath: mountPath, 
            requiresMounting: descriptor.RequiresDisc, 
            launchName: launchName, 
            installDir: gameInstallation.InstallLocation, 
            dosBoxConfigFiles: new[]
            {
                // Add the primary config file
                emulatorInstallation.GetValue<FileSystemPath>(EmulatorDataKey.DosBox_ConfigFilePath),

                // Add the RCP config file
                GetGameConfigFile(gameInstallation)
            });
    }

    private static string GetDOSBoxLaunchArgs(
        FileSystemPath mountPath,
        bool requiresMounting, 
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

        // Mount the disc if required
        if (requiresMounting)
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

    #region Public Methods

    public override EmulatorGameConfigViewModel GetGameConfigViewModel(GameInstallation gameInstallation, EmulatorInstallation emulatorInstallation) =>
        new DosBoxGameConfigViewModel(gameInstallation, this, GetMsdosGameDescriptor(gameInstallation));

    public override EmulatorOptionsViewModel GetEmulatorOptionsViewModel(EmulatorInstallation emulatorInstallation) =>
        new DosBoxEmulatorOptionsViewModel(emulatorInstallation, this);

    public override Task<bool> LaunchGameAsync(GameInstallation gameInstallation, EmulatorInstallation emulatorInstallation) =>
        LaunchGameAsync(gameInstallation, emulatorInstallation, null);

    public async Task<bool> LaunchGameAsync(GameInstallation gameInstallation, EmulatorInstallation emulatorInstallation, string? gameLaunchArgs)
    {
        FileSystemPath launchPath = emulatorInstallation.InstallLocation;
        MsDosGameDescriptor gameDescriptor = GetMsdosGameDescriptor(gameInstallation);

        // Make sure the DOSBox exe exists
        if (!launchPath.FileExists)
        {
            await Services.MessageUI.DisplayMessageAsync(Resources.LaunchGame_DosBoxNotFound, MessageType.Error);
            return false;
        }

        // Make sure the mount path exists
        FileSystemPath mountPath = gameInstallation.GetValue<FileSystemPath>(GameDataKey.Emu_DosBox_MountPath);
        if (gameDescriptor.RequiresDisc && !mountPath.Exists)
        {
            await Services.MessageUI.DisplayMessageAsync(Resources.LaunchGame_MountPathNotFound, MessageType.Error);
            return false;
        }

        // Get the launch args
        string launchArgs = GetDOSBoxLaunchArgs(gameInstallation, emulatorInstallation, gameLaunchArgs);

        Logger.Trace("The game {0} launch info has been retrieved as Path = {1}, Args = {2}",
            gameInstallation.FullId, launchPath, launchArgs);

        // Launch the game
        Process? process = await Services.File.LaunchFileAsync(launchPath, arguments: launchArgs);

        Logger.Info("The game {0} has been launched", gameInstallation.FullId);

        return process != null;
    }

    public override IEnumerable<DuoGridItemViewModel> GetGameInfoItems(GameInstallation gameInstallation, EmulatorInstallation emulatorInstallation)
    {
        // Get the launch info
        FileSystemPath launchPath = emulatorInstallation.InstallLocation;
        string launchArgs = GetDOSBoxLaunchArgs(gameInstallation, emulatorInstallation);

        return base.GetGameInfoItems(gameInstallation, emulatorInstallation).Concat(new[]
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

    public override void CreateGameShortcut(GameInstallation gameInstallation, EmulatorInstallation emulatorInstallation, FileSystemPath shortcutName, FileSystemPath destinationDirectory)
    {
        // Get the launch info
        FileSystemPath launchPath = emulatorInstallation.InstallLocation;
        string launchArgs = GetDOSBoxLaunchArgs(gameInstallation, emulatorInstallation);

        // Create the shortcut
        Services.File.CreateFileShortcut(shortcutName, destinationDirectory, launchPath, launchArgs);

        Logger.Trace("A shortcut was created for {0} under {1}", gameInstallation.FullId, destinationDirectory);
    }

    public override IEnumerable<JumpListItemViewModel> GetJumpListItems(GameInstallation gameInstallation, EmulatorInstallation emulatorInstallation)
    {
        // Get the launch info
        FileSystemPath launchPath = emulatorInstallation.InstallLocation;
        string launchArgs = GetDOSBoxLaunchArgs(gameInstallation, emulatorInstallation);

        if (!launchPath.FileExists)
            return Enumerable.Empty<JumpListItemViewModel>();

        // TODO-14: One for each game mode for edu games?
        return new[]
        {
            new JumpListItemViewModel(
                name: gameInstallation.GetDisplayName(),
                iconSource: emulatorInstallation.InstallLocation,
                launchPath: launchPath,
                workingDirectory: launchPath.Parent,
                launchArguments: launchArgs,
                // TODO-14: Use game ID instead
                id: gameInstallation.LegacyGame.ToString())
        };
    }

    public override Task OnEmulatorSelectedAsync(GameInstallation gameInstallation, EmulatorInstallation emulatorInstallation)
    {
        // Create config file
        new AutoConfigManager(GetGameConfigFile(gameInstallation)).Create(gameInstallation);

        return Task.CompletedTask;
    }

    // TODO-14: Should be per installation
    /// <summary>
    /// Gets the DosBox configuration file path for the auto config file
    /// </summary>
    /// <returns>The file path</returns>
    public FileSystemPath GetGameConfigFile(GameInstallation gameInstallation) =>
        AppFilePaths.UserDataBaseDir + "DosBox" + (gameInstallation.LegacyGame + ".ini");

    #endregion
}