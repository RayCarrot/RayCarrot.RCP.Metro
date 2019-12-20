using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using RayCarrot.CarrotFramework.Abstractions;
using RayCarrot.Extensions;
using RayCarrot.IO;
using RayCarrot.RCP.Core;
using RayCarrot.UI;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// The Rayman Fiesta Run (WinStore) game manager
    /// </summary>
    public sealed class RaymanFiestaRun_WinStore : RCPWinStoreGame
    {
        #region Public Override Properties

        /// <summary>
        /// The game
        /// </summary>
        public override Games Game => Games.RaymanFiestaRun;

        /// <summary>
        /// Gets the package name for the game
        /// </summary>
        public override string PackageName => GetFiestaRunPackageName(RCFRCP.Data.FiestaRunVersion);

        /// <summary>
        /// Gets the full package name for the game
        /// </summary>
        public override string FullPackageName => GetFiestaRunFullPackageName(RCFRCP.Data.FiestaRunVersion);

        /// <summary>
        /// Gets store ID for the game
        /// </summary>
        public override string StoreID => GetStoreID(RCFRCP.Data.FiestaRunVersion);

        /// <summary>
        /// Gets the purchase links for the game
        /// </summary>
        public override IList<GamePurchaseLink> GetGamePurchaseLinks => SupportsWinStoreApps ? new GamePurchaseLink[]
        {
            // Only get the Preload edition URI as the other editions have been delisted.
            new GamePurchaseLink(Resources.GameDisplay_PurchaseWinStore, GetStorePageURI(GetStoreID(FiestaRunEdition.Preload)))
        } : new GamePurchaseLink[0];

        /// <summary>
        /// Gets the game finder item for this game
        /// </summary>
        public override GameFinderItem GameFinderItem => new GameFinderItem(() =>
        {
            // Make sure version is at least Windows 8
            if (!SupportsWinStoreApps)
                return null;

            // Check each version
            foreach (FiestaRunEdition version in Enum.GetValues(typeof(FiestaRunEdition)))
            {
                // Check if valid
                if (IsFiestaRunEditionValidAsync(version))
                {
                    // Return the install directory
                    return new FoundActionResult(GetPackageInstallDirectory(GetFiestaRunPackageName(version)), version);
                }
            }

            // Return an empty path if not found
            return null;
        }, (x, y) => RCFRCP.Data.FiestaRunVersion = y.CastTo<FiestaRunEdition>());

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
                await RCFUI.MessageUI.DisplayMessageAsync(Resources.LocateGame_WinStoreNotSupported, Resources.LocateGame_InvalidWinStoreGameHeader, MessageType.Error);

                return null;
            }

            try
            {
                // Check each version to see if at least one is found
                foreach (FiestaRunEdition version in Enum.GetValues(typeof(FiestaRunEdition)))
                {
                    // Attempt to get the install directory
                    var dir = GetPackageInstallDirectory(GetFiestaRunPackageName(version));

                    // Make sure we got a directory
                    if (dir == null)
                    {
                        RCFCore.Logger?.LogInformationSource($"The {Game} was not found under Windows Store packages");
                        continue;
                    }

                    // Get the install directory
                    FileSystemPath installDir = dir;

                    // Make sure we got a valid directory
                    if (!await IsValidAsync(installDir, version))
                    {
                        RCFCore.Logger?.LogInformationSource($"The {Game} install directory was not valid");

                        continue;
                    }

                    // Set the version
                    RCFRCP.Data.FiestaRunVersion = version;

                    // Return the directory
                    return installDir;
                }

                await RCFUI.MessageUI.DisplayMessageAsync(Resources.LocateGame_InvalidWinStoreGame, Resources.LocateGame_InvalidWinStoreGameHeader, MessageType.Error);

                return null;
            }
            catch (Exception ex)
            {
                ex.HandleError("Getting Fiesta Run game install directory");

                await RCFUI.MessageUI.DisplayMessageAsync(Resources.LocateGame_InvalidWinStoreGame, Resources.LocateGame_InvalidWinStoreGameHeader, MessageType.Error);

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

            if (!((installDir + Game.GetGameInfo<RaymanFiestaRun_Info>().GetFiestaRunFileName(parameter as FiestaRunEdition? ?? RCFRCP.Data.FiestaRunVersion)).FileExists))
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
        public bool IsFiestaRunEditionValidAsync(FiestaRunEdition edition)
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
        public string GetStoreID(FiestaRunEdition version)
        {
            return version switch
            {
                FiestaRunEdition.Default => "9wzdncrdds0c",

                FiestaRunEdition.Preload => "9wzdncrdcw9b",

                FiestaRunEdition.Win10 => "9nblggh59m6b",

                _ => throw new ArgumentOutOfRangeException(nameof(version), version, null),
            };
        }

        /// <summary>
        /// Gets the display name for the Fiesta Run edition
        /// </summary>
        /// <param name="edition">The edition to get the name for</param>
        /// <returns>The display name</returns>
        public string GetFiestaRunEditionDisplayName(FiestaRunEdition edition)
        {
            return edition switch
            {
                FiestaRunEdition.Default => Resources.FiestaRunVersion_Default,
                FiestaRunEdition.Preload => Resources.FiestaRunVersion_Preload,
                FiestaRunEdition.Win10 => Resources.FiestaRunVersion_Win10,
                _ => throw new ArgumentOutOfRangeException(nameof(edition), edition, null)
            };
        }

        /// <summary>
        /// Gets the package name for Rayman Fiesta Run based on edition
        /// </summary>
        /// <param name="edition">The edition</param>
        /// <returns></returns>
        public string GetFiestaRunPackageName(FiestaRunEdition edition)
        {
            return edition switch
            {
                FiestaRunEdition.Default => "Ubisoft.RaymanFiestaRun",
                FiestaRunEdition.Preload => "UbisoftEntertainment.RaymanFiestaRunPreloadEdition",
                FiestaRunEdition.Win10 => "Ubisoft.RaymanFiestaRunWindows10Edition",
                _ => throw new ArgumentOutOfRangeException(nameof(RCFRCP.Data.FiestaRunVersion), RCFRCP.Data.FiestaRunVersion, null)
            };
        }

        /// <summary>
        /// Gets the full package name for Rayman Fiesta Run based on edition
        /// </summary>
        /// <param name="edition">The edition</param>
        /// <returns></returns>
        public string GetFiestaRunFullPackageName(FiestaRunEdition edition)
        {
            return edition switch
            {
                FiestaRunEdition.Default => "Ubisoft.RaymanFiestaRun_ngz4m417e0mpw",
                FiestaRunEdition.Preload => "UbisoftEntertainment.RaymanFiestaRunPreloadEdition_dbgk1hhpxymar",
                FiestaRunEdition.Win10 => "Ubisoft.RaymanFiestaRunWindows10Edition_ngz4m417e0mpw",
                _ => throw new ArgumentOutOfRangeException(nameof(RCFRCP.Data.FiestaRunVersion), RCFRCP.Data.FiestaRunVersion, null)
            };
        }

        #endregion
    }
}