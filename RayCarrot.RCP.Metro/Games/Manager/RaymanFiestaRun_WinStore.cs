using System;
using RayCarrot.Windows.Shell;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// The Rayman Fiesta Run (WinStore) game manager
    /// </summary>
    public sealed class RaymanFiestaRun_WinStore : RCPWinStoreGame
    {
        #region Public Overrides

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

        #endregion

        #region Private Methods

        /// <summary>
        /// Indicates if the edition of Fiesta Run is valid based on edition
        /// </summary>
        /// <param name="edition">The edition to check</param>
        /// <returns>True if the game is valid, otherwise false</returns>
        public bool IsFiestaRunEditionValidAsync(FiestaRunEdition edition)
        {
            // Make sure version is at least Windows 8
            if (AppViewModel.WindowsVersion < WindowsVersion.Win8)
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