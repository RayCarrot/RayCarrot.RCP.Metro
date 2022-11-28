using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// A game descriptor for a MS-DOS program
/// </summary>
public abstract class MSDOSGameDescriptor :
    // TODO-14: Inherit directly from GameDescriptor instead?
    Win32GameDescriptor
{
    // TODO-14: Most of this should be removed in favor of the new emulator system

    #region Logger

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    #endregion

    #region Public Properties

    public override GamePlatform Platform => GamePlatform.MSDOS;
    public override bool SupportsGameLaunchMode => true;
    public override Emulator Emulator => new Emulator_DOSBox(); // TODO-14: Make this lazy if it's a property, otherwise method

    /// <summary>
    /// Gets the DosBox configuration file path for the auto config file
    /// </summary>
    /// <returns>The file path</returns>
    public FileSystemPath DosBoxConfigFile => AppFilePaths.UserDataBaseDir + "DosBox" + (LegacyGame + ".ini"); // TODO-14: Should be per installation

    /// <summary>
    /// Indicates if the game requires a disc to be mounted in order to play
    /// </summary>
    public virtual bool RequiresMounting => true; // TODO-14: Perhaps find better way to deal with this?

    /// <summary>
    /// The DOSBox file path
    /// </summary>
    public virtual FileSystemPath DOSBoxFilePath => Services.Data.Emu_DOSBox_Path;

    /// <summary>
    /// Optional additional config files
    /// </summary>
    public virtual IEnumerable<FileSystemPath> AdditionalConfigFiles => Enumerable.Empty<FileSystemPath>();

    /// <summary>
    /// The executable name for the game. This is independent of the <see cref="GameDescriptor.DefaultFileName"/> which is used to launch the game.
    /// </summary>
    public abstract string ExecutableName { get; }

    #endregion

    #region Protected Methods

    protected override FileSystemPath GetIconResourcePath(GameInstallation gameInstallation) => DOSBoxFilePath;

    protected override async Task<bool> VerifyCanLaunchAsync(GameInstallation gameInstallation)
    {
        // Make sure the DosBox executable exists
        if (!File.Exists(DOSBoxFilePath))
        {
            await Services.MessageUI.DisplayMessageAsync(Resources.LaunchGame_DosBoxNotFound, MessageType.Error);
            return false;
        }

        // Make sure the mount path exists
        FileSystemPath mountPath = gameInstallation.GetValue<FileSystemPath>(GameDataKey.DOSBoxMountPath);
        if (RequiresMounting && !mountPath.Exists)
        {
            await Services.MessageUI.DisplayMessageAsync(Resources.LaunchGame_MountPathNotFound, MessageType.Error);
            return false;
        }

        return true;
    }

    protected override FileSystemPath GetLaunchFilePath(GameInstallation gameInstallation) => DOSBoxFilePath;

    protected override string GetLaunchArgs(GameInstallation gameInstallation)
    {
        FileSystemPath mountPath = gameInstallation.GetValue<FileSystemPath>(GameDataKey.DOSBoxMountPath);
        return GetDosBoxArguments(mountPath, DefaultFileName, gameInstallation.InstallLocation);
    }

    /// <summary>
    /// Gets the DosBox launch arguments for the game
    /// </summary>
    /// <param name="mountPath">The disc/file to mount</param>
    /// <param name="launchName">The launch name</param>
    /// <param name="installDir">The game install directory</param>
    /// <returns>The launch arguments</returns>
    protected string GetDosBoxArguments(FileSystemPath mountPath, string launchName, FileSystemPath installDir)
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
        AddConfig(DosBoxConfigFile.FullPath);

        // Add additional config files
        foreach (var config in AdditionalConfigFiles)
            AddConfig(config);

        // Mount the disc if required
        if (RequiresMounting)
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

    public override Task PostGameAddAsync(GameInstallation gameInstallation)
    {
        // Create config file
        new Emulator_DOSBox_AutoConfigManager(DosBoxConfigFile).Create();

        return Task.CompletedTask;
    }

    public override Task PostGameRemovedAsync(GameInstallation gameInstallation)
    {
        try
        {
            // Remove the config file
            Services.File.DeleteFile(DosBoxConfigFile);
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Removing DosBox auto config file");
        }

        return Task.CompletedTask;
    }

    #endregion
}