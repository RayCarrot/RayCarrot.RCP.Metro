using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using RayCarrot.IO;
using NLog;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// The Rayman Fiesta Run (WinStore) game manager
    /// </summary>
    public sealed class GameManager_RaymanFiestaRun_WinStore : GameManager_WinStore
    {
        #region Logger

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        #endregion

        #region Public Override Properties

        /// <summary>
        /// The game
        /// </summary>
        public override Games Game => Games.RaymanFiestaRun;

        /// <summary>
        /// Gets the package name for the game
        /// </summary>
        public override string PackageName => GetFiestaRunPackageName(RCPServices.Data.FiestaRunVersion);

        /// <summary>
        /// Gets the full package name for the game
        /// </summary>
        public override string FullPackageName => GetFiestaRunFullPackageName(RCPServices.Data.FiestaRunVersion);

        /// <summary>
        /// Gets store ID for the game
        /// </summary>
        public override string StoreID => GetStoreID(RCPServices.Data.FiestaRunVersion);

        /// <summary>
        /// Gets the purchase links for the game
        /// </summary>
        public override IList<GamePurchaseLink> GetGamePurchaseLinks => SupportsWinStoreApps ? new GamePurchaseLink[]
        {
            // Only get the Preload edition URI as the other editions have been delisted.
            new GamePurchaseLink(Resources.GameDisplay_PurchaseWinStore, GetStorePageURI(GetStoreID(UserData_FiestaRunEdition.Preload)))
        } : new GamePurchaseLink[0];

        /// <summary>
        /// Gets the game finder item for this game
        /// </summary>
        public override GameFinder_GameItem GameFinderItem => new GameFinder_GameItem(() =>
        {
            // Make sure version is at least Windows 8
            if (!SupportsWinStoreApps)
                return null;

            // Check each version
            foreach (UserData_FiestaRunEdition version in Enum.GetValues(typeof(UserData_FiestaRunEdition)))
            {
                // Check if valid
                if (IsFiestaRunEditionValidAsync(version))
                {
                    // Return the install directory
                    return new GameFinder_FoundResult(GetPackageInstallDirectory(GetFiestaRunPackageName(version)), version);
                }
            }

            // Return an empty path if not found
            return null;
        }, (x, y) => RCPServices.Data.FiestaRunVersion = y.CastTo<UserData_FiestaRunEdition>());

        #endregion

        #region Protected Override Methods

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

            try
            {
                // Check each version to see if at least one is found
                foreach (UserData_FiestaRunEdition version in Enum.GetValues(typeof(UserData_FiestaRunEdition)))
                {
                    // Attempt to get the install directory
                    var dir = GetPackageInstallDirectory(GetFiestaRunPackageName(version));

                    // Make sure we got a directory
                    if (dir == null)
                    {
                        Logger.Info($"The {Game} was not found under Windows Store packages");
                        continue;
                    }

                    // Get the install directory
                    FileSystemPath installDir = dir;

                    // Make sure we got a valid directory
                    if (!await IsValidAsync(installDir, version))
                    {
                        Logger.Info($"The {Game} install directory was not valid");

                        continue;
                    }

                    // Set the version
                    RCPServices.Data.FiestaRunVersion = version;

                    // Return the directory
                    return installDir;
                }

                await Services.MessageUI.DisplayMessageAsync(Resources.LocateGame_InvalidWinStoreGame, Resources.LocateGame_InvalidWinStoreGameHeader, MessageType.Error);

                return null;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Getting Fiesta Run game install directory");

                await Services.MessageUI.DisplayMessageAsync(Resources.LocateGame_InvalidWinStoreGame, Resources.LocateGame_InvalidWinStoreGameHeader, MessageType.Error);

                return null;
            }
        }

        #endregion

        #region Public Override Methods

        /// <summary>
        /// Indicates if the game is valid
        /// </summary>
        /// <param name="installDir">The game install directory, if any</param>
        /// <param name="parameter">Optional game parameter</param>
        /// <returns>True if the game is valid, otherwise false</returns>
        [MethodImpl(MethodImplOptions.NoInlining)]
        public override Task<bool> IsValidAsync(FileSystemPath installDir, object parameter = null)
        {
            // Make sure version is at least Windows 8
            if (!SupportsWinStoreApps)
                return Task.FromResult(false);

            if (!((installDir + Game.GetGameInfo<GameInfo_RaymanFiestaRun>().GetFiestaRunFileName(parameter as UserData_FiestaRunEdition? ?? RCPServices.Data.FiestaRunVersion)).FileExists))
                return Task.FromResult(false);

            return Task.FromResult(true);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Indicates if the edition of Fiesta Run is valid based on edition
        /// </summary>
        /// <param name="edition">The edition to check</param>
        /// <returns>True if the game is valid, otherwise false</returns>
        [MethodImpl(MethodImplOptions.NoInlining)]
        public bool IsFiestaRunEditionValidAsync(UserData_FiestaRunEdition edition)
        {
            // Make sure version is at least Windows 8
            if (!SupportsWinStoreApps)
                return false;

            return GetGamePackage(GetFiestaRunPackageName(edition)) != null;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Gets the Store ID for the specified version
        /// </summary>
        /// <param name="version">The version to get the ID for</param>
        /// <returns>The ID</returns>
        public string GetStoreID(UserData_FiestaRunEdition version)
        {
            return version switch
            {
                UserData_FiestaRunEdition.Default => "9wzdncrdds0c",

                UserData_FiestaRunEdition.Preload => "9wzdncrdcw9b",

                UserData_FiestaRunEdition.Win10 => "9nblggh59m6b",

                _ => throw new ArgumentOutOfRangeException(nameof(version), version, null),
            };
        }

        /// <summary>
        /// Gets the display name for the Fiesta Run edition
        /// </summary>
        /// <param name="edition">The edition to get the name for</param>
        /// <returns>The display name</returns>
        public string GetFiestaRunEditionDisplayName(UserData_FiestaRunEdition edition)
        {
            return edition switch
            {
                UserData_FiestaRunEdition.Default => Resources.FiestaRunVersion_Default,
                UserData_FiestaRunEdition.Preload => Resources.FiestaRunVersion_Preload,
                UserData_FiestaRunEdition.Win10 => Resources.FiestaRunVersion_Win10,
                _ => throw new ArgumentOutOfRangeException(nameof(edition), edition, null)
            };
        }

        /// <summary>
        /// Gets the package name for Rayman Fiesta Run based on edition
        /// </summary>
        /// <param name="edition">The edition</param>
        /// <returns></returns>
        public string GetFiestaRunPackageName(UserData_FiestaRunEdition edition)
        {
            return edition switch
            {
                UserData_FiestaRunEdition.Default => "Ubisoft.RaymanFiestaRun",
                UserData_FiestaRunEdition.Preload => "UbisoftEntertainment.RaymanFiestaRunPreloadEdition",
                UserData_FiestaRunEdition.Win10 => "Ubisoft.RaymanFiestaRunWindows10Edition",
                _ => throw new ArgumentOutOfRangeException(nameof(RCPServices.Data.FiestaRunVersion), RCPServices.Data.FiestaRunVersion, null)
            };
        }

        /// <summary>
        /// Gets the full package name for Rayman Fiesta Run based on edition
        /// </summary>
        /// <param name="edition">The edition</param>
        /// <returns></returns>
        public string GetFiestaRunFullPackageName(UserData_FiestaRunEdition edition)
        {
            return edition switch
            {
                UserData_FiestaRunEdition.Default => "Ubisoft.RaymanFiestaRun_ngz4m417e0mpw",
                UserData_FiestaRunEdition.Preload => "UbisoftEntertainment.RaymanFiestaRunPreloadEdition_dbgk1hhpxymar",
                UserData_FiestaRunEdition.Win10 => "Ubisoft.RaymanFiestaRunWindows10Edition_ngz4m417e0mpw",
                _ => throw new ArgumentOutOfRangeException(nameof(RCPServices.Data.FiestaRunVersion), RCPServices.Data.FiestaRunVersion, null)
            };
        }

        #endregion
    }
}