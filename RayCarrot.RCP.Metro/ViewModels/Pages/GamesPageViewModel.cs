using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using Nito.AsyncEx;
using RayCarrot.CarrotFramework.Abstractions;
using RayCarrot.Extensions;

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
            // Create properties
            AsyncLock = new AsyncLock();
            InstalledGames = new ObservableCollection<KeyValuePair<Games, GameDisplayViewModel>>();
            NotInstalledGames = new ObservableCollection<KeyValuePair<Games, GameDisplayViewModel>>();

            // Enable collection synchronization
            BindingOperations.EnableCollectionSynchronization(InstalledGames, Application.Current);
            BindingOperations.EnableCollectionSynchronization(NotInstalledGames, Application.Current);

            // Refresh on app refresh
            App.RefreshRequired += async (s, e) =>
            {
                if (e.LaunchInfoModified && e.ModifiedGames?.Any() == true)
                    foreach (Games game in e.ModifiedGames)
                        await RefreshGameAsync(game);

                else if (e.LaunchInfoModified || e.GameCollectionModified)
                    await RefreshAsync();
            };

            // Refresh on culture changed
            RCFCore.Data.CultureChanged += async (s, e) => await RefreshAsync();

            // Refresh on startup
            Metro.App.Current.StartupComplete += async (s, e) => await RefreshAsync();
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
        public ObservableCollection<KeyValuePair<Games, GameDisplayViewModel>> InstalledGames { get; }

        /// <summary>
        /// The not installed games
        /// </summary>
        public ObservableCollection<KeyValuePair<Games, GameDisplayViewModel>> NotInstalledGames { get; }

        /// <summary>
        /// Indicates if there are any installed games
        /// </summary>
        public bool AnyInstalledGames { get; set; }

        /// <summary>
        /// Indicates if there are any not installed games
        /// </summary>
        public bool AnyNotInstalledGames { get; set; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Refreshes the added game
        /// </summary>
        /// <returns>The task</returns>
        public async Task RefreshGameAsync(Games game)
        {
            RCFCore.Logger?.LogInformationSource($"The displayed game {game} is being refreshed...");

            using (await AsyncLock.LockAsync())
            {
                await Task.Run(() =>
                {
                    // Make sure the game has been added
                    if (!game.IsAdded())
                        throw new Exception("Only added games can be refreshed individually");

                    // Get the collection containing the game
                    var collection = InstalledGames.Any(x => x.Key == game) ? InstalledGames : NotInstalledGames;

                    // Refresh the game
                    collection[collection.FindItemIndex(x => x.Key == game)] = new KeyValuePair<Games, GameDisplayViewModel>(game, game.GetDisplayViewModel());
                });
            }

            RCFCore.Logger?.LogInformationSource($"The displayed game {game} has been refreshed");
        }

        /// <summary>
        /// Refreshes the games
        /// </summary>
        /// <returns>The task</returns>
        public async Task RefreshAsync()
        {
            RCFCore.Logger?.LogInformationSource($"The displayed games are being refreshed...");

            using (await AsyncLock.LockAsync())
            {
                await Task.Run(() =>
                {
                    try
                    {
                        InstalledGames.Clear();
                        NotInstalledGames.Clear();

                        AnyInstalledGames = false;
                        AnyNotInstalledGames = false;

                        // Enumerate each game
                        foreach (Games game in RCFRCP.App.GetGames)
                        {
                            // Check if it has been added
                            if (game.IsAdded())
                            {
                                // Add the game to the collection
                                InstalledGames.Add(new KeyValuePair<Games, GameDisplayViewModel>(game, game.GetDisplayViewModel()));
                                AnyInstalledGames = true;
                            }
                            else
                            {
                                NotInstalledGames.Add(new KeyValuePair<Games, GameDisplayViewModel>(game, game.GetDisplayViewModel()));
                                AnyNotInstalledGames = true;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        ex.HandleCritical("Refreshing games");
                        throw;
                    }
                });
            }

            RCFCore.Logger?.LogInformationSource($"The displayed games have been refreshed");
        }

        #endregion
    }
}