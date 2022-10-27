#nullable disable
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using NLog;
using Windows.ApplicationModel;
using Windows.Management.Deployment;

namespace RayCarrot.RCP.Metro;
/*
    Sometimes inlining will have to be removed using this attribute:
    [MethodImpl(MethodImplOptions.NoInlining)]
    This is due to the WinRT references used only exist on modern versions of Windows.
     */

// TODO-14: Rename to Package since it doesn't have to be installed from the store

/// <summary>
/// Base for a WinStore Rayman Control Panel game
/// </summary>
public abstract class GameManager_WinStore : GameManager
{
    #region Logger

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    #endregion

    #region Public Override Properties

    /// <summary>
    /// Indicates if using <see cref="UserData_GameLaunchMode"/> is supported
    /// </summary>
    public override bool SupportsGameLaunchMode => false;

    /// <summary>
    /// Gets the additional overflow button items for the game
    /// </summary>
    public override IList<OverflowButtonItemViewModel> GetAdditionalOverflowButtonItems => new OverflowButtonItemViewModel[]
    {
        new OverflowButtonItemViewModel(Resources.GameDisplay_OpenInWinStore, GenericIconKind.GameDisplay_Microsoft, new AsyncRelayCommand(async () =>
        {
            // NOTE: We could use Launcher.LaunchURI here, but since we're targeting Windows 7 it is good to use as few of the WinRT APIs as possible to avoid any runtime errors. Launching a file as a process will work with URLs as well, although less information will be given in case of error (such as if no application is installed to handle the URI).
            (await Services.File.LaunchFileAsync(GetStorePageURI()))?.Dispose();
        })),
    };

    /// <summary>
    /// Gets the purchase links for the game
    /// </summary>
    public override IList<GamePurchaseLink> GetGamePurchaseLinks => SupportsWinStoreApps ? new GamePurchaseLink[]
    {
        new GamePurchaseLink(Resources.GameDisplay_PurchaseWinStore, GetStorePageURI())
    } : new GamePurchaseLink[0];

    /// <summary>
    /// Gets the game finder item for this game
    /// </summary>
    public override GameFinder_GameItem GameFinderItem => new GameFinder_GameItem(() =>
    {
        // Make sure version is at least Windows 8
        if (!SupportsWinStoreApps)
            return null;

        // Return the install directory, if found
        return new GameFinder_FoundResult(GetPackageInstallDirectory(), null);
    });

    #endregion

    #region Public Abstract Properties

    /// <summary>
    /// Gets the package name for the game
    /// </summary>
    public abstract string PackageName { get; }

    /// <summary>
    /// Gets the full package name for the game
    /// </summary>
    public abstract string FullPackageName { get; }

    /// <summary>
    /// Gets store ID for the game
    /// </summary>
    public abstract string StoreID { get; }

    #endregion

    #region Public Properties

    /// <summary>
    /// Gets the legacy launch path to use for launching the game. This method of launching should only be used when no other method is available. If the package is not found this method will launch a new Windows Explorer window instead. The entry point is defaulted to "!APP" and may not always be correct.
    /// </summary>
    public string LegacyLaunchPath => "shell:appsFolder\\" + $"{FullPackageName}!App";

    /// <summary>
    /// Indicates if Microsoft Store apps are supported on the current system
    /// </summary>
    public bool SupportsWinStoreApps => AppViewModel.WindowsVersion is >= WindowsVersion.Win8 or WindowsVersion.Unknown;

    #endregion

    #region Protected Override Methods

    /// <summary>
    /// The implementation for launching the game
    /// </summary>
    /// <param name="forceRunAsAdmin">Indicated if the game should be forced to run as admin</param>
    /// <returns>The launch result</returns>
    protected override async Task<GameLaunchResult> LaunchAsync(bool forceRunAsAdmin)
    {
        try
        {
            // Launch the first app entry for the package
            await LaunchFirstPackageEntryAsync();

            return new GameLaunchResult(null, true);
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Launching Windows Store application");
            await Services.MessageUI.DisplayExceptionMessageAsync(ex, String.Format(Resources.LaunchGame_WinStoreError, Game.GetGameDescriptor().DisplayName));

            return new GameLaunchResult(null, false);
        }
    }

    /// <summary>
    /// Indicates if the game is valid
    /// </summary>
    /// <param name="installDir">The game install directory, if any</param>
    /// <param name="parameter">Optional game parameter</param>
    /// <returns>True if the game is valid, otherwise false</returns>
    [MethodImpl(MethodImplOptions.NoInlining)]
    protected override async Task<bool> IsDirectoryValidAsync(FileSystemPath installDir, object parameter = null)
    {
        // Make sure version is at least Windows 8
        if (!SupportsWinStoreApps)
            return false;

        // Make sure the default game file is found
        if (!(await base.IsDirectoryValidAsync(installDir, parameter)))
            return false;

        return true;
    }

    #endregion

    #region Public Override Methods

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
                id: gameInstallation.Game.ToString())
        };
    }

    /// <summary>
    /// Locates the game
    /// </summary>
    /// <returns>Null if the game was not found. Otherwise a valid or empty path for the install directory</returns>
    public override async Task<FileSystemPath?> LocateAsync()
    {
        // Make sure version is at least Windows 8
        if (!SupportsWinStoreApps)
        {
            await Services.MessageUI.DisplayMessageAsync(Resources.LocateGame_WinStoreNotSupported, Resources.LocateGame_InvalidWinStoreGameHeader, MessageType.Error);

            return null;
        }

        FileSystemPath installDir;

        try
        {
            // Get the install directory
            var dir = GetPackageInstallDirectory();

            // Make sure we got a valid directory
            if (dir == null)
            {
                Logger.Info("The {0} was not found under Windows Store packages", Game);

                return null;
            }

            installDir = dir;
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Getting Windows Store game install directory");

            await Services.MessageUI.DisplayMessageAsync(Resources.LocateGame_InvalidWinStoreGame, Resources.LocateGame_InvalidWinStoreGameHeader, MessageType.Error);

            return null;
        }

        if (!await IsValidAsync(installDir))
        {
            Logger.Info("The {0} install directory was not valid", Game);

            await Services.MessageUI.DisplayMessageAsync(Resources.LocateGame_InvalidWinStoreGame, Resources.LocateGame_InvalidWinStoreGameHeader, MessageType.Error);

            return null;
        }

        return installDir;
    }

    /// <summary>
    /// Creates a shortcut to launch the game from
    /// </summary>
    /// <param name="shortcutName">The name of the shortcut</param>
    /// <param name="destinationDirectory">The destination directory for the shortcut</param>
    public override void CreateGameShortcut(FileSystemPath shortcutName, FileSystemPath destinationDirectory)
    {
        // Create the shortcut
        Services.File.CreateFileShortcut(shortcutName, destinationDirectory, LegacyLaunchPath);

        Logger.Trace("A shortcut was created for {0} under {1}", Game, destinationDirectory);
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Gets the game package install directory for a Windows Store game
    /// </summary>
    /// <paramref name="packageName">The name of the package or null to use default</paramref>
    /// <returns>The package install directory</returns>
    [MethodImpl(MethodImplOptions.NoInlining)]
    public string GetPackageInstallDirectory(string packageName = null)
    {
        return GetGamePackage(packageName)?.InstalledLocation.Path;
    }

    /// <summary>
    /// Gets the game package for a Windows Store game
    /// </summary>
    /// <paramref name="packageName">The name of the package or null to use default</paramref>
    /// <returns>The package or null if not found</returns>
    [MethodImpl(MethodImplOptions.NoInlining)]
    public Package GetGamePackage(string packageName = null)
    {
        // TODO-14: Why are we enumerating all the packages to find it?
        return new PackageManager().FindPackagesForUser(String.Empty).FirstOrDefault(x => x.Id.Name == (packageName ?? PackageName));
    }

    /// <summary>
    /// Launches the first package entry for a game
    /// </summary>
    /// <returns>The task</returns>
    [MethodImpl(MethodImplOptions.NoInlining)]
    public async Task LaunchFirstPackageEntryAsync()
    {
        await (await GetGamePackage().GetAppListEntriesAsync()).First().LaunchAsync();
    }

    /// <summary>
    /// Gets the URI to use when opening the game in the Store
    /// </summary>
    /// <param name="storeID">The Store ID for the item to open or null if current one should be used</param>
    /// <returns>The URI</returns>
    public string GetStorePageURI(string storeID = null)
    {
        // Documentation on the store URI scheme:
        // https://docs.microsoft.com/en-us/windows/uwp/launch-resume/launch-store-app

        return $"ms-windows-store://pdp/?ProductId={storeID ?? StoreID}";
    }

    #endregion

    #region Public Static Methods

    /// <summary>
    /// Gets the backup directories for a Windows Store game
    /// </summary>
    /// <param name="fullPackageName">The full package name</param>
    /// <returns>The backup directories</returns>
    public static GameBackups_Directory[] GetWinStoreBackupDirs(string fullPackageName)
    {
        return new GameBackups_Directory[]
        {
            new GameBackups_Directory(Environment.SpecialFolder.LocalApplicationData.GetFolderPath() + "Packages" + fullPackageName, SearchOption.AllDirectories, "*", "0", 0),
            new GameBackups_Directory(Environment.SpecialFolder.LocalApplicationData.GetFolderPath() + "Packages" + fullPackageName + "LocalState", SearchOption.TopDirectoryOnly, "*", "0", 1)
        };
    }

    #endregion
}