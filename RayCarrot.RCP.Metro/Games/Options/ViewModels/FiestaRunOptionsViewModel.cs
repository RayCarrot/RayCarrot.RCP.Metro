using System;
using System.Threading.Tasks;
using Nito.AsyncEx;
using RayCarrot.Logging;

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
            // Create properties
            AsyncLock = new AsyncLock();

            // Get the manager
            var manager = Games.RaymanFiestaRun.GetManager<RaymanFiestaRun_WinStore>(GameType.WinStore);

            // Get the install directories for each version
            DefaultInstallDir = manager.GetPackageInstallDirectory(manager.GetFiestaRunPackageName(FiestaRunEdition.Default));
            PreloadInstallDir = manager.GetPackageInstallDirectory(manager.GetFiestaRunPackageName(FiestaRunEdition.Preload));
            Win10InstallDir = manager.GetPackageInstallDirectory(manager.GetFiestaRunPackageName(FiestaRunEdition.Win10));

            // Get the current version
            _selectedFiestaRunVersion = Data.FiestaRunVersion;
        }

        #endregion

        #region Private Fields

        private FiestaRunEdition _selectedFiestaRunVersion;

        #endregion

        #region Private Properties

        /// <summary>
        /// The install directory for <see cref="FiestaRunEdition.Default"/>
        /// </summary>
        private string DefaultInstallDir { get; }

        /// <summary>
        /// The install directory for <see cref="FiestaRunEdition.Preload"/>
        /// </summary>
        private string PreloadInstallDir { get; }

        /// <summary>
        /// The install directory for <see cref="FiestaRunEdition.Win10"/>
        /// </summary>
        private string Win10InstallDir { get; }

        /// <summary>
        /// The async lock for <see cref="UpdateFiestaRunVersionAsync"/>
        /// </summary>
        private AsyncLock AsyncLock { get; }

        #endregion

        #region Public Properties

        /// <summary>
        /// Indicates if <see cref="FiestaRunEdition.Default"/> is available
        /// </summary>
        public bool IsFiestaRunDefaultAvailable => DefaultInstallDir != null;

        /// <summary>
        /// Indicates if <see cref="FiestaRunEdition.Preload"/> is available
        /// </summary>
        public bool IsFiestaRunPreloadAvailable => PreloadInstallDir != null;

        /// <summary>
        /// Indicates if <see cref="FiestaRunEdition.Win10"/> is available
        /// </summary>
        public bool IsFiestaRunWin10Available => Win10InstallDir != null;

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
            using (await AsyncLock.LockAsync())
            {
                try
                {
                    // Update the version
                    Data.FiestaRunVersion = SelectedFiestaRunVersion;

                    // Get the new game data
                    GameData gameData = new GameData(GameType.WinStore, SelectedFiestaRunVersion switch
                    {
                        FiestaRunEdition.Default => DefaultInstallDir,
                        FiestaRunEdition.Preload => PreloadInstallDir,
                        FiestaRunEdition.Win10 => Win10InstallDir,
                        _ => throw new ArgumentOutOfRangeException(nameof(SelectedFiestaRunVersion),
                            SelectedFiestaRunVersion, null)
                    });

                    // Update the game data
                    RCPServices.Data.Games[Games.RaymanFiestaRun] = gameData;

                    await RCPServices.App.OnRefreshRequiredAsync(new RefreshRequiredEventArgs(Games.RaymanFiestaRun, false, false, false, true));
                }
                catch (Exception ex)
                {
                    ex.HandleError("Updating Fiesta Run install directory");
                }
            }
        }

        #endregion
    }
}