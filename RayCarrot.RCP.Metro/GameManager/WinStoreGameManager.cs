using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Management.Deployment;
using RayCarrot.CarrotFramework.Abstractions;
using RayCarrot.Extensions;
using RayCarrot.IO;
using RayCarrot.UI;
using RayCarrot.Windows.Shell;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// The game manager for a <see cref="GameType.WinStore"/> game
    /// </summary>
    public class WinStoreGameManager : BaseGameManager
    {
        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="game">The game to manage</param>
        public WinStoreGameManager(Games game) : base(game, GameType.WinStore)
        {

        }

        #endregion

        #region Protected Overrides Properties

        /// <summary>
        /// The display name for the game type
        /// </summary>
        public override string GameTypeDisplayName => Resources.GameType_WinStore;

        /// <summary>
        /// Indicates if using <see cref="GameLaunchMode"/> is supported
        /// </summary>
        public override bool SupportsGameLaunchMode => false;

        #endregion

        #region Public Methods

        /// <summary>
        /// Gets the package name for Rayman Fiesta Run based on edition
        /// </summary>
        /// <param name="edition">The edition</param>
        /// <returns></returns>
        public string GetFiestaRunPackageName(FiestaRunEdition edition)
        {
            switch (edition)
            {
                case FiestaRunEdition.Default:
                    return "Ubisoft.RaymanFiestaRun";

                case FiestaRunEdition.Preload:
                    return "UbisoftEntertainment.RaymanFiestaRunPreloadEdition";

                case FiestaRunEdition.Win10:
                    return "Ubisoft.RaymanFiestaRunWindows10Edition";

                default:
                    throw new ArgumentOutOfRangeException(nameof(RCFRCP.Data.FiestaRunVersion), RCFRCP.Data.FiestaRunVersion, null);
            }
        }

        /// <summary>
        /// Gets the game package for a Windows Store game
        /// </summary>
        /// <paramref name="packageName">The name of the package or null to use default</paramref>
        /// <returns>The package or null if not found</returns>
        [MethodImpl(MethodImplOptions.NoInlining)]
        public object GetGamePackage(string packageName = null)
        {
            return new PackageManager().FindPackagesForUser(String.Empty).FindItem(x => x.Id.Name == (packageName ?? GetPackageName()));
        }

        /// <summary>
        /// Gets the game package install directory for a Windows Store game
        /// </summary>
        /// <returns>The package install directory</returns>
        [MethodImpl(MethodImplOptions.NoInlining)]
        public FileSystemPath GetPackageInstallDirectory()
        {
            return (GetGamePackage() as Package)?.InstalledLocation.Path ?? FileSystemPath.EmptyPath;
        }

        /// <summary>
        /// Launches the first package entry for a game
        /// </summary>
        /// <returns>The task</returns>
        [MethodImpl(MethodImplOptions.NoInlining)]
        public async Task LaunchFirstPackageEntryAsync()
        {
            await (await GetGamePackage().CastTo<Package>().GetAppListEntriesAsync()).First().LaunchAsync();
        }
        /// <summary>
        /// Gets the package name for a Windows Store game
        /// </summary>
        /// <returns>The package name</returns>
        public string GetPackageName()
        {
            if (Game == Games.RaymanJungleRun)
                return "UbisoftEntertainment.RaymanJungleRun";

            if (Game == Games.RabbidsBigBang)
                return "UbisoftEntertainment.RabbidsBigBang";

            else if (Game == Games.RaymanFiestaRun)
                return GetFiestaRunPackageName(RCFRCP.Data.FiestaRunVersion);

            throw new ArgumentOutOfRangeException(nameof(Game), Game, "A package name can not be obtained from the specified game");
        }

        #endregion

        #region Protected Overrides

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
                await RCFUI.MessageUI.DisplayMessageAsync(String.Format(Resources.LaunchGame_WinStoreError, Game.GetDisplayName()), MessageType.Error);

                return new GameLaunchResult(null, false);
            }
        }

        /// <summary>
        /// Locates the game
        /// </summary>
        /// <returns>Null if the game was not found. Otherwise a valid or empty path for the instal directory</returns>
        protected override async Task<FileSystemPath?> LocateAsync()
        {
            // Helper method for finding and adding a Windows Store app
            bool FindWinStoreApp() =>
                // Check if the game is installed
                IsValid(FileSystemPath.EmptyPath);

            bool found = false;

            if (Game == Games.RaymanFiestaRun)
            {
                foreach (FiestaRunEdition version in Enum.GetValues(typeof(FiestaRunEdition)))
                {
                    if (found)
                        break;

                    RCFRCP.Data.FiestaRunVersion = version;
                    found = FindWinStoreApp();
                }
            }
            else
            {
                found = FindWinStoreApp();
            }

            if (!found)
            {
                RCFCore.Logger?.LogInformationSource($"The {Game} was not found under Windows Store packages");

                await RCFUI.MessageUI.DisplayMessageAsync(Resources.LocateGame_InvalidWinStoreGame, Resources.LocateGame_InvalidWinStoreGameHeader, MessageType.Error);

                return null;
            }

            return FileSystemPath.EmptyPath;
        }

        #endregion

        #region Public Overrides

        /// <summary>
        /// Gets the launch info for the game
        /// </summary>
        /// <returns>The launch info</returns>
        public override GameLaunchInfo GetLaunchInfo()
        {
            // NOTE: This method of launching a WinStore game should only be used when no other method is available. If the package is not found this method will launch a new Windows Explorer window instead. The entry point ("!APP") may not always be correct (that is the default used for most packages with a single entry point).

            return new GameLaunchInfo("shell:appsFolder\\" + $"{Game.GetLaunchName()}!App", null);
        }

        /// <summary>
        /// Indicates if the game is valid
        /// </summary>
        /// <param name="installDir">The game install directory, if any</param>
        /// <returns>True if the game is valid, otherwise false</returns>
        [MethodImpl(MethodImplOptions.NoInlining)]
        public override bool IsValid(FileSystemPath installDir)
        {
            // Make sure version is at least Windows 8
            if (AppViewModel.WindowsVersion < WindowsVersion.Win8)
                return false;

            return GetGamePackage() != null;
        }

        /// <summary>
        /// Gets the install directory for the game
        /// </summary>
        /// <returns>The install directory</returns>
        public override FileSystemPath GetInstallDirectory()
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
        /// Gets the info items for the specified game
        /// </summary>
        /// <returns>The info items</returns>
        [MethodImpl(MethodImplOptions.NoInlining)]
        public override IEnumerable<DuoGridItemViewModel> GetGameInfoItems()
        {
            // Return from base
            foreach (var item in base.GetGameInfoItems())
                yield return item;

            // Get the package
            if (Game.GetGameManager<WinStoreGameManager>().GetGamePackage() is Package package)
            {
                // Return new items
                yield return new DuoGridItemViewModel(Resources.GameInfo_WinStoreDependencies, package.Dependencies.Select(x => x.Id.Name).JoinItems(", "), UserLevel.Technical);
                yield return new DuoGridItemViewModel(Resources.GameInfo_WinStoreFullName, package.Id.FullName, UserLevel.Advanced);
                yield return new DuoGridItemViewModel(Resources.GameInfo_WinStoreArchitecture, package.Id.Architecture.ToString(), UserLevel.Technical);
                yield return new DuoGridItemViewModel(Resources.GameInfo_WinStoreVersion, $"{package.Id.Version.Major}.{package.Id.Version.Minor}.{package.Id.Version.Build}.{package.Id.Version.Revision}", UserLevel.Technical);
                yield return new DuoGridItemViewModel(Resources.GameInfo_WinStoreInstallDate, package.InstalledDate.DateTime.ToString(CultureInfo.CurrentCulture), UserLevel.Advanced);
            }
            else
            {
                RCFCore.Logger?.LogErrorSource("Game options WinStore package is null");
            }
        }

        /// <summary>
        /// Gets the icon resource path for the game based on its launch information
        /// </summary>
        /// <returns>The icon resource path</returns>
        public override string GetIconResourcePath() => GetLaunchInfo().Path;

        #endregion
    }
}