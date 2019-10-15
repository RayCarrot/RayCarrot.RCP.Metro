using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Management.Deployment;
using Windows.Services.Store;
using Windows.System;
using MahApps.Metro.IconPacks;
using RayCarrot.CarrotFramework.Abstractions;
using RayCarrot.Extensions;
using RayCarrot.IO;
using RayCarrot.UI;
using RayCarrot.Windows.Shell;

namespace RayCarrot.RCP.Metro
{
    /*
        Sometimes inlining will have to be removed using this attribute:
        [MethodImpl(MethodImplOptions.NoInlining)]
        This is due to the WinRT references used only exist on modern versions of Windows
         */

    /// <summary>
    /// Base for a WinStore Rayman Control Panel game
    /// </summary>
    public abstract class RCPWinStoreGame : RCPGameManager
    {
        #region Public Override Properties

        /// <summary>
        /// The game type
        /// </summary>
        public override GameType Type => GameType.WinStore;

        /// <summary>
        /// The display name for the game type
        /// </summary>
        public override string GameTypeDisplayName => Resources.GameType_WinStore;

        /// <summary>
        /// Indicates if using <see cref="GameLaunchMode"/> is supported
        /// </summary>
        public override bool SupportsGameLaunchMode => false;

        /// <summary>
        /// Gets the additional overflow button items for the game
        /// </summary>
        public override IList<OverflowButtonItemViewModel> GetAdditionalOverflowButtonItems => new OverflowButtonItemViewModel[]
        {
            // TODO: Localize
            new OverflowButtonItemViewModel("Open in store", PackIconMaterialKind.Windows, new AsyncRelayCommand(async () =>
            {
                // NOTE: We could use Launcher.LaunchURI here, but since we're targeting Windows 7 it is good to use as few of the WinRT APIs as possible to avoid any runtime errors. Launching a file as a process will work with URLs as well, although less information will be given in case of error (such as if no application is installed to handle the URI).
                (await RCFRCP.File.LaunchFileAsync(GetStorePageURI()))?.Dispose();
            })),
        };

        /// <summary>
        /// Gets the info items for the game
        /// </summary>
        public override IList<DuoGridItemViewModel> GetGameInfoItems
        {
            get
            {
                // Get the package
                var package = GetGamePackage();

                // Return new items
                return new List<DuoGridItemViewModel>(base.GetGameInfoItems)
                {
                    new DuoGridItemViewModel(Resources.GameInfo_WinStoreDependencies, package.Dependencies.Select(x => x.Id.Name).JoinItems(", "), UserLevel.Technical),
                    new DuoGridItemViewModel(Resources.GameInfo_WinStoreFullName, package.Id.FullName, UserLevel.Advanced),
                    new DuoGridItemViewModel(Resources.GameInfo_WinStoreArchitecture, package.Id.Architecture.ToString(), UserLevel.Technical),
                    new DuoGridItemViewModel(Resources.GameInfo_WinStoreVersion, $"{package.Id.Version.Major}.{package.Id.Version.Minor}.{package.Id.Version.Build}.{package.Id.Version.Revision}", UserLevel.Technical),
                    new DuoGridItemViewModel(Resources.GameInfo_WinStoreInstallDate, package.InstalledDate.DateTime.ToString(CultureInfo.CurrentCulture), UserLevel.Advanced),
                };
            }
        }

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
                ex.HandleError("Launching Windows Store application");
                await RCFUI.MessageUI.DisplayExceptionMessageAsync(ex, String.Format(Resources.LaunchGame_WinStoreError, Game.GetGameInfo().DisplayName));

                return new GameLaunchResult(null, false);
            }
        }

        #endregion

        #region Public Override Methods

        /// <summary>
        /// Gets the install directory for the game
        /// </summary>
        /// <returns>The install directory</returns>
        public override FileSystemPath FindInstallDirectory()
        {
            try
            {
                return GetPackageInstallDirectory();
            }
            catch (Exception ex)
            {
                ex.HandleError("Getting Windows Store game install directory");
                return FileSystemPath.EmptyPath;
            }
        }

        /// <summary>
        /// Gets the available jump list items for this game
        /// </summary>
        /// <returns>The items</returns>
        public override IList<JumpListItemViewModel> GetJumpListItems()
        {
            return new JumpListItemViewModel[]
            {
                new JumpListItemViewModel(Game.GetGameInfo().DisplayName, LegacyLaunchPath, LegacyLaunchPath, null, Game.ToString())
            };
        }

        /// <summary>
        /// Locates the game
        /// </summary>
        /// <returns>Null if the game was not found. Otherwise a valid or empty path for the install directory</returns>
        public override async Task<FileSystemPath?> LocateAsync()
        {
            // Make sure version is at least Windows 8
            if (AppViewModel.WindowsVersion < WindowsVersion.Win8)
            {
                await RCFUI.MessageUI.DisplayMessageAsync(Resources.LocateGame_WinStoreNotSupported, Resources.LocateGame_InvalidWinStoreGameHeader, MessageType.Error);

                return null;
            }

            bool found = false;

            if (Game == Games.RaymanFiestaRun)
            {
                foreach (FiestaRunEdition version in Enum.GetValues(typeof(FiestaRunEdition)))
                {
                    if (found)
                        break;

                    // TODO: Move into fiesta run class
                    found = new RaymanFiestaRun_WinStore().IsFiestaRunEditionValidAsync(version);

                    if (found)
                        RCFRCP.Data.FiestaRunVersion = version;
                }
            }
            else
            {
                found = await IsValidAsync(FileSystemPath.EmptyPath);
            }

            if (!found)
            {
                RCFCore.Logger?.LogInformationSource($"The {Game} was not found under Windows Store packages");

                await RCFUI.MessageUI.DisplayMessageAsync(Resources.LocateGame_InvalidWinStoreGame, Resources.LocateGame_InvalidWinStoreGameHeader, MessageType.Error);

                return null;
            }

            return FileSystemPath.EmptyPath;
        }

        /// <summary>
        /// Indicates if the game is valid
        /// </summary>
        /// <param name="installDir">The game install directory, if any</param>
        /// <returns>True if the game is valid, otherwise false</returns>
        public override Task<bool> IsValidAsync(FileSystemPath installDir)
        {
            // Make sure version is at least Windows 8
            if (AppViewModel.WindowsVersion < WindowsVersion.Win8)
                return Task.FromResult(false);

            return Task.FromResult(GetGamePackage() != null);
        }

        /// <summary>
        /// Creates a shortcut to launch the game from
        /// </summary>
        /// <param name="shortcutName">The name of the shortcut</param>
        /// <param name="destinationDirectory">The destination directory for the shortcut</param>
        /// <returns>The task</returns>
        public override async Task CreateGameShortcut(FileSystemPath shortcutName, FileSystemPath destinationDirectory)
        {
            // Create the shortcut
            await RCFRCP.File.CreateFileShortcutAsync(shortcutName, destinationDirectory, LegacyLaunchPath);

            RCFCore.Logger?.LogTraceSource($"A shortcut was created for {Game} under {destinationDirectory}");
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Gets the game package install directory for a Windows Store game
        /// </summary>
        /// <returns>The package install directory</returns>
        public FileSystemPath GetPackageInstallDirectory()
        {
            return GetGamePackage()?.InstalledLocation.Path ?? FileSystemPath.EmptyPath;
        }

        /// <summary>
        /// Gets the game package for a Windows Store game
        /// </summary>
        /// <paramref name="packageName">The name of the package or null to use default</paramref>
        /// <returns>The package or null if not found</returns>
        public Package GetGamePackage(string packageName = null)
        {
            return new PackageManager().FindPackagesForUser(String.Empty).FindItem(x => x.Id.Name == (packageName ?? PackageName));
        }

        /// <summary>
        /// Launches the first package entry for a game
        /// </summary>
        /// <returns>The task</returns>
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
        public static List<BackupDir> GetWinStoreBackupDirs(string fullPackageName)
        {
            return new List<BackupDir>()
            {
                new BackupDir(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Packages", fullPackageName), SearchOption.AllDirectories, "*", "0", 0),
                new BackupDir(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Packages", fullPackageName, "LocalState"), SearchOption.TopDirectoryOnly, "*", "0", 1)
            };
        }

        #endregion
    }
}