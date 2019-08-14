using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using Nito.AsyncEx;
using RayCarrot.CarrotFramework.Abstractions;
using RayCarrot.UI;

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

            RCFRCP.App.GameRefreshRequired += async (s, e) => await RefreshAsync();
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
                        Application.Current.Dispatcher.Invoke(() => NotInstalledGames.Add(game.GetDisplayViewModel()));
                        continue;
                    }

                    // Check if it's valid
                    if (!game.GetGameManager().IsValid(game.GetInfo().InstallDirectory))
                    {
                        // Show message
                        await RCFUI.MessageUI.DisplayMessageAsync(String.Format(Resources.GameNotFound, game.GetDisplayName()), Resources.GameNotFoundHeader, MessageType.Error);

                        // Remove the game from app data
                        await RCFRCP.App.RemoveGameAsync(game, true);

                        Application.Current.Dispatcher.Invoke(() => NotInstalledGames.Add(game.GetDisplayViewModel()));

                        RCFCore.Logger?.LogInformationSource($"The game {game} has been removed due to not being valid");

                        continue;
                    }

                    // Add the game to the collection
                    Application.Current.Dispatcher.Invoke(() => InstalledGames.Add(game.GetDisplayViewModel()));
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