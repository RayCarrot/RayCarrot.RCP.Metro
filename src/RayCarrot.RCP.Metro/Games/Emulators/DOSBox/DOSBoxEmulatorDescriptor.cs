using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;

namespace RayCarrot.RCP.Metro;

public sealed class DOSBoxEmulatorDescriptor : EmulatorDescriptor
{
    #region Logger

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    #endregion

    #region Public Properties

    public override string EmulatorId => "DOSBox";
    public override GamePlatform[] SupportedPlatforms => new[] { GamePlatform.MSDOS };
    public override LocalizedString DisplayName => new ResourceLocString(nameof(Resources.GameType_DosBox));

    #endregion

    #region Private Methods

    private MSDOSGameDescriptor GetMsdosGameDescriptor(GameInstallation gameInstallation) =>
        gameInstallation.GameDescriptor as MSDOSGameDescriptor 
        ?? throw new InvalidOperationException($"The {nameof(DOSBoxEmulatorDescriptor)} can only be used with {nameof(MSDOSGameDescriptor)}");

    private string GetDOSBoxLaunchArgs(GameInstallation gameInstallation)
    {
        MSDOSGameDescriptor descriptor = GetMsdosGameDescriptor(gameInstallation);

        string? gameArgs = descriptor.GetLaunchArgs(gameInstallation);
        string launchName = gameArgs == null ? descriptor.DefaultFileName : $"{descriptor.DefaultFileName} {gameArgs}";

        FileSystemPath mountPath = gameInstallation.GetValue<FileSystemPath>(GameDataKey.DOSBoxMountPath);
        return GetDOSBoxLaunchArgs(
            mountPath: mountPath, 
            requiresMounting: descriptor.RequiresDisc, 
            launchName: launchName, 
            installDir: gameInstallation.InstallLocation, 
            dosBoxConfigFile: GetGameConfigFile(gameInstallation));
    }

    private static string GetDOSBoxLaunchArgs(
        FileSystemPath mountPath,
        bool requiresMounting, 
        string launchName, 
        FileSystemPath installDir,
        FileSystemPath dosBoxConfigFile)
    {
        // Create a string builder for the argument
        var str = new StringBuilder();

        // Helper method for adding an argument
        void AddArg(string arg)
        {
            str.Append($"{arg} ");
        }

        // Helper method for adding a config file to the argument
        void AddConfig(FileSystemPath configFile)
        {
            if (configFile.FileExists)
                AddArg($"-conf \"{configFile.FullPath}\"");
        }

        // Add the primary config file
        AddConfig(Services.Data.Emu_DOSBox_ConfigPath);

        // Add the RCP config file
        AddConfig(dosBoxConfigFile.FullPath);

        // Add additional config files
        //foreach (var config in additionalConfigFiles)
        //    AddConfig(config);

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

    public override GameOptionsDialog_EmulatorConfigPageViewModel GetGameConfigViewModel(GameInstallation gameInstallation, EmulatorInstallation emulatorInstallation) =>
        new Emulator_DOSBox_ConfigViewModel(gameInstallation, this);

    public override async Task<bool> LaunchGameAsync(GameInstallation gameInstallation, EmulatorInstallation emulatorInstallation, bool forceRunAsAdmin)
    {
        FileSystemPath launchPath = emulatorInstallation.InstallLocation;
        MSDOSGameDescriptor gameDescriptor = GetMsdosGameDescriptor(gameInstallation);

        // Make sure the DOSBox exe exists
        if (!launchPath.FileExists)
        {
            await Services.MessageUI.DisplayMessageAsync(Resources.LaunchGame_DosBoxNotFound, MessageType.Error);
            return false;
        }

        // Make sure the mount path exists
        FileSystemPath mountPath = gameInstallation.GetValue<FileSystemPath>(GameDataKey.DOSBoxMountPath);
        if (gameDescriptor.RequiresDisc && !mountPath.Exists)
        {
            await Services.MessageUI.DisplayMessageAsync(Resources.LaunchGame_MountPathNotFound, MessageType.Error);
            return false;
        }

        // Get the launch args
        string launchArgs = GetDOSBoxLaunchArgs(gameInstallation);

        Logger.Trace("The game {0} launch info has been retrieved as Path = {1}, Args = {2}",
            gameInstallation.FullId, launchPath, launchArgs);

        // Launch the game
        UserData_GameLaunchMode launchMode = gameInstallation.GetValue<UserData_GameLaunchMode>(GameDataKey.Win32LaunchMode);
        bool asAdmin = forceRunAsAdmin || launchMode == UserData_GameLaunchMode.AsAdmin;
        Process? process = await Services.File.LaunchFileAsync(launchPath, asAdmin, launchArgs);

        Logger.Info("The game {0} has been launched", gameInstallation.FullId);

        return process != null;
    }

    public override IEnumerable<DuoGridItemViewModel> GetGameInfoItems(GameInstallation gameInstallation, EmulatorInstallation emulatorInstallation)
    {
        // Get the launch info
        FileSystemPath launchPath = emulatorInstallation.InstallLocation;
        string launchArgs = GetDOSBoxLaunchArgs(gameInstallation);

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
        string launchArgs = GetDOSBoxLaunchArgs(gameInstallation);

        // Create the shortcut
        Services.File.CreateFileShortcut(shortcutName, destinationDirectory, launchPath, launchArgs);

        Logger.Trace("A shortcut was created for {0} under {1}", gameInstallation.FullId, destinationDirectory);
    }

    public override IEnumerable<JumpListItemViewModel> GetJumpListItems(GameInstallation gameInstallation, EmulatorInstallation emulatorInstallation)
    {
        // Get the launch info
        FileSystemPath launchPath = emulatorInstallation.InstallLocation;
        string launchArgs = GetDOSBoxLaunchArgs(gameInstallation);

        if (!launchPath.FileExists)
            return Enumerable.Empty<JumpListItemViewModel>();

        return new[]
        {
            new JumpListItemViewModel(
                name: gameInstallation.GameDescriptor.DisplayName,
                iconSource: emulatorInstallation.InstallLocation,
                launchPath: launchPath,
                workingDirectory: launchPath.Parent,
                launchArguments: launchArgs,
                // TODO-14: Use game ID instead
                id: gameInstallation.LegacyGame.ToString())
        };
    }

    public override Task OnEmulatorSelected(GameInstallation gameInstallation, EmulatorInstallation emulatorInstallation)
    {
        // Create config file
        new Emulator_DOSBox_AutoConfigManager(GetGameConfigFile(gameInstallation)).Create();

        return Task.CompletedTask;
    }

    public override Task OnEmulatorDeselected(GameInstallation gameInstallation, EmulatorInstallation emulatorInstallation)
    {
        try
        {
            // Remove the config file
            Services.File.DeleteFile(GetGameConfigFile(gameInstallation));
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Removing DosBox auto config file");
        }

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