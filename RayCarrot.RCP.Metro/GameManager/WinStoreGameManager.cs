using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
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
        /// <param name="type">The game type</param>
        public WinStoreGameManager(Games game, GameType type = GameType.WinStore) : base(game, type)
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

        /// <summary>
        /// Gets the display name for the Fiesta Run edition
        /// </summary>
        /// <param name="edition">The edition to get the name for</param>
        /// <returns>The display name</returns>
        public string GetFiestaRunEditionDisplayName(FiestaRunEdition edition)
        {
            switch (edition)
            {
                case FiestaRunEdition.Default:
                    return Resources.FiestaRunVersion_Default;

                case FiestaRunEdition.Preload:
                    return Resources.FiestaRunVersion_Preload;

                case FiestaRunEdition.Win10:
                    return Resources.FiestaRunVersion_Win10;

                default:
                    throw new ArgumentOutOfRangeException(nameof(edition), edition, null);
            }
        }

        /// <summary>
        /// Gets the backup directories for a Windows Store game
        /// </summary>
        /// <param name="fullPackageName">The full package name</param>
        /// <returns>The backup directories</returns>
        public List<BackupDir> GetWinStoreBackupDirs(string fullPackageName)
        {
            return new List<BackupDir>()
            {
                new BackupDir()
                {
                    DirPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Packages", fullPackageName),
                    SearchOption = SearchOption.AllDirectories,
                    ID = "0"
                }
            };
        }

        /// <summary>
        /// Gets the full package name for a Windows Store game
        /// </summary>
        public string GetFullPackageName()
        {
            if (Game == Games.RaymanJungleRun)
                return "UbisoftEntertainment.RaymanJungleRun_dbgk1hhpxymar";

            if (Game == Games.RabbidsBigBang)
                return "UbisoftEntertainment.RabbidsBigBang_dbgk1hhpxymar";

            else if (Game == Games.RaymanFiestaRun)
                return GetFiestaRunFullPackageName(RCFRCP.Data.FiestaRunVersion);

            throw new ArgumentOutOfRangeException(nameof(Game), Game, "A package name can not be obtained from the specified game");
        }

        /// <summary>
        /// Gets the full package name for Rayman Fiesta Run based on edition
        /// </summary>
        /// <param name="edition">The edition</param>
        /// <returns></returns>
        public string GetFiestaRunFullPackageName(FiestaRunEdition edition)
        {
            switch (edition)
            {
                case FiestaRunEdition.Default:
                    return "Ubisoft.RaymanFiestaRun_ngz4m417e0mpw";

                case FiestaRunEdition.Preload:
                    return "UbisoftEntertainment.RaymanFiestaRunPreloadEdition_dbgk1hhpxymar";

                case FiestaRunEdition.Win10:
                    return "Ubisoft.RaymanFiestaRunWindows10Edition_ngz4m417e0mpw";

                default:
                    throw new ArgumentOutOfRangeException(nameof(RCFRCP.Data.FiestaRunVersion), RCFRCP.Data.FiestaRunVersion, null);
            }
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

        #endregion

        #region Public Overrides

        /// <summary>
        /// Locates the game
        /// </summary>
        /// <returns>Null if the game was not found. Otherwise a valid or empty path for the instal directory</returns>
        public override async Task<FileSystemPath?> LocateAsync()
        {
            bool found = false;

            if (Game == Games.RaymanFiestaRun)
            {
                foreach (FiestaRunEdition version in Enum.GetValues(typeof(FiestaRunEdition)))
                {
                    if (found)
                        break;

                    found = GetGamePackage(GetFiestaRunPackageName(version)) != null;

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
        public override Task<bool> IsValidAsync(FileSystemPath installDir)
        {
            // Make sure version is at least Windows 8
            if (AppViewModel.WindowsVersion < WindowsVersion.Win8)
                return Task.FromResult(false);

            return Task.FromResult(GetGamePackage() != null);
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

        /// <summary>
        /// Gets the backup infos for this game
        /// </summary>
        /// <returns>The backup infos</returns>
        public override Task<List<IBackupInfo>> GetBackupInfosAsync()
        {
            if (Game != Games.RaymanFiestaRun)
                return base.GetBackupInfosAsync();

            // Get every installed version
            var versions = FiestaRunEdition.Preload.GetValues().Where(x => GetGamePackage(GetFiestaRunPackageName(x)) != null);

            // Return a backup info for each version
            return Task.FromResult(versions.Select(x =>
            {
                var backupName = $"Rayman Fiesta Run ({x})";

                return new BaseBackupInfo(RCFRCP.App.GetCompressedBackupFile(backupName), RCFRCP.App.GetBackupDir(backupName), GetWinStoreBackupDirs(GetFiestaRunFullPackageName(x)), $"{Games.RaymanFiestaRun.GetDisplayName()} {GetFiestaRunEditionDisplayName(x)}") as IBackupInfo;
            }).ToList());
        }

        #endregion
    }
}