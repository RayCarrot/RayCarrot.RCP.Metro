using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using NLog;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Core;
using Windows.Management.Deployment;

namespace RayCarrot.RCP.Metro;

// TODO-14: We might need to disable inlining some places here so that this doesn't crash on Windows 7 where WinRT isn't supported
// TODO-14: Fix logs and comments due to rename from WinStore

/// <summary>
/// A game descriptor for a Windows package
/// </summary>
public abstract class WindowsPackageGameDescriptor : GameDescriptor
{
    #region Logger

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    #endregion

    #region Private Properties

    /// <summary>
    /// Gets the legacy launch path to use for launching the game. This method of launching should only be used when no
    /// other method is available. If the package is not found this method will launch a new Windows Explorer window
    /// instead. The entry point is defaulted to "!APP" and may not always be correct.
    /// </summary>
    private string LegacyLaunchPath => "shell:appsFolder\\" + $"{FullPackageName}!App";

    #endregion

    #region Public Properties

    /// <summary>
    /// Indicates if th Windows runtime is supported on the current system
    /// </summary>
    public static bool SupportsWinRT => AppViewModel.WindowsVersion is >= WindowsVersion.Win8 or WindowsVersion.Unknown;

    public override GamePlatform Platform => GamePlatform.WindowsPackage;
    public override bool SupportsGameLaunchMode => false;
    public override bool AllowPatching => false;

    public abstract string PackageName { get; }
    public abstract string FullPackageName { get; }

    #endregion

    #region Private Methods

    /// <summary>
    /// Gets the game package
    /// </summary>
    /// <returns>The package or null if not found</returns>
    private Package? GetPackage()
    {
        return new PackageManager().FindPackagesForUser(String.Empty).FirstOrDefault(x => x.Id.Name == PackageName);
    }

    #endregion

    #region Protected Methods

    protected override async Task<GameLaunchResult> LaunchAsync(GameInstallation gameInstallation, bool forceRunAsAdmin)
    {
        try
        {
            Package? package = GetPackage();

            if (package == null)
            {
                // TODO-14: Handle? Only log?
                return new GameLaunchResult(null, false);
            }

            // Launch the first app entry for the package
            AppListEntry mainEntry = (await package.GetAppListEntriesAsync()).First();
            await mainEntry.LaunchAsync();

            return new GameLaunchResult(null, true);
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Launching Windows Store application");
            await Services.MessageUI.DisplayExceptionMessageAsync(ex, String.Format(Resources.LaunchGame_WinStoreError, DisplayName));

            return new GameLaunchResult(null, false);
        }
    }

    protected override bool IsGameLocationValid(FileSystemPath installLocation)
    {
        // Make sure version is at least Windows 8
        if (!SupportsWinRT)
            return false;

        // Make sure the default game file is found
        return base.IsGameLocationValid(installLocation);
    }

    #endregion

    #region Public Methods

    public override IEnumerable<GameAddAction> GetAddActions() => new GameAddAction[]
    {
        new FindWindowsPackageGameAddActions(this)
    };

    public override GameFinder_GameItem GetGameFinderItem() => new(() =>
    {
        // Make sure version is at least Windows 8
        if (!SupportsWinRT)
            return null;

        // Return the install directory, if found
        return new GameFinder_FoundResult(GetPackageInstallDirectory());
    });

    public override IEnumerable<DuoGridItemViewModel> GetGameInfoItems(GameInstallation gameInstallation)
    {
        // Get the package
        Package? package = GetPackage();

        if (package == null)
            return base.GetGameInfoItems(gameInstallation);

        return base.GetGameInfoItems(gameInstallation).Concat(new[]
        {
            new DuoGridItemViewModel(
                header: new ResourceLocString(nameof(Resources.GameInfo_WinStoreDependencies)),
                text: package.Dependencies.Select(x => x.Id.Name).JoinItems(", "),
                minUserLevel: UserLevel.Technical),
            new DuoGridItemViewModel(
                header: new ResourceLocString(nameof(Resources.GameInfo_WinStoreFullName)),
                text: package.Id.FullName,
                minUserLevel: UserLevel.Advanced),
            new DuoGridItemViewModel(
                header: new ResourceLocString(nameof(Resources.GameInfo_WinStoreArchitecture)),
                text: package.Id.Architecture.ToString(),
                minUserLevel: UserLevel.Technical),
            new DuoGridItemViewModel(
                header: new ResourceLocString(nameof(Resources.GameInfo_WinStoreVersion)),
                text: $"{package.Id.Version.Major}.{package.Id.Version.Minor}.{package.Id.Version.Build}.{package.Id.Version.Revision}",
                minUserLevel: UserLevel.Technical),
            new DuoGridItemViewModel(
                header: new ResourceLocString(nameof(Resources.GameInfo_WinStoreInstallDate)),
                text: new GeneratedLocString(() => package.InstalledDate.DateTime.ToString(CultureInfo.CurrentCulture)),
                minUserLevel: UserLevel.Advanced),
        });
    }

    public override void CreateGameShortcut(GameInstallation gameInstallation, FileSystemPath shortcutName, FileSystemPath destinationDirectory)
    {
        // Create the shortcut
        Services.File.CreateFileShortcut(shortcutName, destinationDirectory, LegacyLaunchPath);

        Logger.Trace("A shortcut was created for {0} under {1}", Id, destinationDirectory);
    }

    public override IEnumerable<JumpListItemViewModel> GetJumpListItems(GameInstallation gameInstallation)
    {
        return new[]
        {
            new JumpListItemViewModel(
                name: gameInstallation.GameDescriptor.DisplayName,
                iconSource: LegacyLaunchPath,
                launchPath: LegacyLaunchPath,
                workingDirectory: null,
                launchArguments: null, 
                // TODO-14: Use game ID instead
                id: gameInstallation.LegacyGame.ToString())
        };
    }

    /// <summary>
    /// Gets the package install directory
    /// </summary>
    /// <returns>The package install directory</returns>
    public string? GetPackageInstallDirectory()
    {
        return GetPackage()?.InstalledLocation.Path;
    }

    public GameBackups_Directory[] GetBackupDirectories() => new GameBackups_Directory[]
    {
        new(Environment.SpecialFolder.LocalApplicationData.GetFolderPath() + "Packages" + FullPackageName, SearchOption.AllDirectories, "*", "0", 0),
        new(GetLocalAppDataDirectory(), SearchOption.TopDirectoryOnly, "*", "0", 1)
    };

    public FileSystemPath GetLocalAppDataDirectory() => 
        Environment.SpecialFolder.LocalApplicationData.GetFolderPath() + "Packages" + FullPackageName + "LocalState";

    #endregion
}