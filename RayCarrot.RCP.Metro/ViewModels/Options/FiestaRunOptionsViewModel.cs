using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using RayCarrot.CarrotFramework.Abstractions;
using RayCarrot.IO;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// View model for the Rayman Fiesta Run options
    /// </summary>
    public class FiestaRunOptionsViewModel : BaseRCPViewModel
    {
        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        public FiestaRunOptionsViewModel()
        {
            // Get the manager
            var manager = Games.RaymanFiestaRun.GetManager<RaymanFiestaRun_WinStore>(GameType.WinStore);

            // Get available versions
            IsFiestaRunDefaultAvailable = manager.GetGamePackage(manager.GetFiestaRunPackageName(FiestaRunEdition.Default)) != null;
            IsFiestaRunPreloadAvailable = manager.GetGamePackage(manager.GetFiestaRunPackageName(FiestaRunEdition.Preload)) != null;
            IsFiestaRunWin10Available = manager.GetGamePackage(manager.GetFiestaRunPackageName(FiestaRunEdition.Win10)) != null;

            // Get the current version
            _selectedFiestaRunVersion = Data.FiestaRunVersion;
        }

        #endregion

        #region Private Fields

        private FiestaRunEdition _selectedFiestaRunVersion;

        #endregion

        #region Public Properties

        /// <summary>
        /// Indicates if <see cref="FiestaRunEdition.Default"/> is available
        /// </summary>
        public bool IsFiestaRunDefaultAvailable { get; }

        /// <summary>
        /// Indicates if <see cref="FiestaRunEdition.Preload"/> is available
        /// </summary>
        public bool IsFiestaRunPreloadAvailable { get; }

        /// <summary>
        /// Indicates if <see cref="FiestaRunEdition.Win10"/> is available
        /// </summary>
        public bool IsFiestaRunWin10Available { get; }

        /// <summary>
        /// The selected Fiesta Run version
        /// </summary>
        public FiestaRunEdition SelectedFiestaRunVersion
        {
            get => _selectedFiestaRunVersion;
            set
            {
                // Update value
                _selectedFiestaRunVersion = value;

                // Update data
                Task.Run(UpdateFiestaRunVersionAsync);
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Updates the current Fiesta Run version based on the selected one
        /// </summary>
        /// <returns>The task</returns>
        public async Task UpdateFiestaRunVersionAsync()
        {
            // Update the install directory and game data
            try
            {
                // Keep track of previous version
                var previousVersion = Data.FiestaRunVersion;

                // Update the version
                Data.FiestaRunVersion = SelectedFiestaRunVersion;

                // Attempt to get the new install directory
                var installDir = Games.RaymanFiestaRun.GetManager<RCPWinStoreGame>().GetPackageInstallDirectory();

                // Make sure the new install directory is valid
                if (installDir == null || !installDir.Value.DirectoryExists)
                {
                    // Revert back the Fiesta Run version
                    Data.FiestaRunVersion = previousVersion;

                    // TODO: Show error message

                    return;
                }

                // Get the new game data
                GameData gameData = new GameData(GameType.WinStore, installDir.Value);

                // Update the game data
                RCFRCP.Data.Games[Games.RaymanFiestaRun] = gameData;

                await RCFRCP.App.OnRefreshRequiredAsync(new RefreshRequiredEventArgs(Games.RaymanFiestaRun, false, false, false, true));
            }
            catch (Exception ex)
            {
                ex.HandleError("Updating Fiesta Run install directory");
            }
        }

        #endregion
    }
}