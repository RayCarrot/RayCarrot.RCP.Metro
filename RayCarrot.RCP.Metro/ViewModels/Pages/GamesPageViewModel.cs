using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using Nito.AsyncEx;
using RayCarrot.CarrotFramework.Abstractions;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// View model for the games page
    /// </summary>
    public class GamesPageViewModel : BaseRCPViewModel
    {
        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        public GamesPageViewModel()
        {
            AsyncLock = new AsyncLock();
            InstalledGames = new ObservableCollection<GameDisplayViewModel>();
            NotInstalledGames = new ObservableCollection<GameDisplayViewModel>();

            BindingOperations.EnableCollectionSynchronization(InstalledGames, Application.Current);
            BindingOperations.EnableCollectionSynchronization(NotInstalledGames, Application.Current);

            App.RefreshRequired += async (s, e) =>
            {
                if (e.LaunchInfoModified || e.GameCollectionModified)
                    await RefreshAsync();
            };
            RCFCore.Data.CultureChanged += async (s, e) => await RefreshAsync();
        }

        #endregion

        #region Private Properties

        /// <summary>
        /// The async lock to use for the game page
        /// </summary>
        private AsyncLock AsyncLock { get; }

        #endregion

        #region Public Properties

        /// <summary>
        /// The installed games
        /// </summary>
        public ObservableCollection<GameDisplayViewModel> InstalledGames { get; }

        /// <summary>
        /// The not installed games
        /// </summary>
        public ObservableCollection<GameDisplayViewModel> NotInstalledGames { get; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Refreshes the games
        /// </summary>
        /// <returns>The task</returns>
        public async Task RefreshAsync()
        {
            RCFCore.Logger?.LogInformationSource($"The displayed games are being refreshed...");

            using (await AsyncLock.LockAsync())
            {
                InstalledGames.Clear();
                NotInstalledGames.Clear();

                // Enumerate each game
                foreach (Games game in RCFRCP.App.GetGames)
                {
                    // Check if it has been added
                    if (!game.IsAdded())
                    {
                        NotInstalledGames.Add(game.GetDisplayViewModel());
                        continue;
                    }

                    // Add the game to the collection
                    InstalledGames.Add(game.GetDisplayViewModel());
                }

                // Notify the UI
                OnPropertyChanged(nameof(InstalledGames));
                OnPropertyChanged(nameof(NotInstalledGames));
            }

            RCFCore.Logger?.LogInformationSource($"The displayed games have been refreshed");
        }

        #endregion
    }
}