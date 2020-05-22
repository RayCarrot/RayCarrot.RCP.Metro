using MahApps.Metro.IconPacks;
using Nito.AsyncEx;
using RayCarrot.CarrotFramework.Abstractions;
using RayCarrot.Extensions;
using RayCarrot.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using RayCarrot.Logging;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// View model for the games page
    /// </summary>
    public class GamesPageViewModel : BaseRCPViewModel, IDisposable
    {
        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        public GamesPageViewModel()
        {
            // Get categorized games
            var games = App.GetCategorizedGames;
            
            // Create properties
            RefreshingGames = false;
            AsyncLock = new AsyncLock();

            GameCategories = new GameCategoryViewModel[]
            {
                // Create the master category
                new GameCategoryViewModel(App.GetGames), 
                
                // Create the categories
                new GameCategoryViewModel(games[GameCategory.Rayman], new LocalizedString(() => Resources.GamesPage_Category_Rayman), PackIconMaterialKind.GamepadVariantOutline), 
                new GameCategoryViewModel(games[GameCategory.Rabbids], new LocalizedString(() => Resources.GamesPage_Category_Rabbids), PackIconMaterialKind.GamepadVariantOutline), 
                new GameCategoryViewModel(games[GameCategory.Demo], new LocalizedString(() => Resources.GamesPage_Category_Demos), PackIconMaterialKind.ShoppingMusic),
                new GameCategoryViewModel(games[GameCategory.Other], new LocalizedString(() => Resources.GamesPage_Category_Other), PackIconMaterialKind.Buffer), 
                new GameCategoryViewModel(games[GameCategory.Fan], new LocalizedString(() => Resources.GamesPage_Category_Fan), PackIconMaterialKind.Earth),
            };

            // Create commands
            RunGameFinderCommand = new AsyncRelayCommand(RunGameFinderAsync);
            RefreshGamesCommand = new AsyncRelayCommand(RefreshAsync);

            // Refresh on app refresh
            App.RefreshRequired += async (s, e) =>
            {
                if (e.LaunchInfoModified && e.ModifiedGames?.Any() == true)
                    foreach (Games game in e.ModifiedGames)
                        await Task.Run(async () => await RefreshGameAsync(game));

                else if (e.LaunchInfoModified || e.GameCollectionModified)
                    await Task.Run(async () => await RefreshAsync());
            };

            // Refresh category visibility
            _ = Task.Run(async () => await RefreshCategorizedVisibilityAsync());

            // Refresh on culture changed
            RCFCore.Data.CultureChanged += async (s, e) => await Task.Run(async () => await RefreshAsync());

            // Refresh on startup
            Metro.App.Current.StartupComplete += async (s, e) => await RefreshAsync();

            // Refresh visibility on setting change
            Data.PropertyChanged += Data_PropertyChangedAsync;
        }

        #endregion

        #region Commands

        public AsyncRelayCommand RunGameFinderCommand { get; }

        public AsyncRelayCommand RefreshGamesCommand { get; }

        #endregion

        #region Private Properties

        /// <summary>
        /// The async lock to use for the game page
        /// </summary>
        private AsyncLock AsyncLock { get; }

        #endregion

        #region Public Properties

        /// <summary>
        /// The selected game category index
        /// </summary>
        public int SelectedCategoryIndex { get; set; }

        /// <summary>
        /// The games category view models
        /// </summary>
        public GameCategoryViewModel[] GameCategories { get; }

        /// <summary>
        /// Indicates if the games are being refreshed
        /// </summary>
        public bool RefreshingGames { get; set; }

        #endregion
    
        #region Public Methods

        /// <summary>
        /// Refreshes the added game
        /// </summary>
        /// <returns>The task</returns>
        public async Task RefreshGameAsync(Games game)
        {
            RL.Logger?.LogInformationSource($"The displayed game {game} is being refreshed...");

            using (await AsyncLock.LockAsync())
            {
                try
                {
                    // Make sure the game has been added
                    if (!game.IsAdded())
                        throw new Exception("Only added games can be refreshed individually");

                    if (Application.Current.Dispatcher == null)
                        throw new Exception("Dispatcher can not be NULL");

                    // Get the display view model
                    var displayVM = game.GetGameInfo().GetDisplayViewModel();

                    // Refresh the game in every category it's available in
                    foreach (var category in GameCategories.Where(x => x.Games.Contains(game)))
                    {
                        RL.Logger?.LogTraceSource($"The displayed game {game} in {category.DisplayName} is being refreshed...");

                        // Get the collection containing the game
                        var collection = category.InstalledGames.Any(x => x.Game == game) ? category.InstalledGames : category.NotInstalledGames;

                        // Get the game index
                        var index = collection.FindItemIndex(x => x.Game == game);

                        // Make sure we got a valid index
                        if (index == -1)
                        {
                            RL.Logger?.LogWarningSource($"The displayed game {game} in {category.DisplayName} could not be refreshed due to not existing in either game collection");

                            return;
                        }

                        // Refresh the game
                        Application.Current.Dispatcher.Invoke(() => collection[index] = displayVM);

                        RL.Logger?.LogTraceSource($"The displayed game {game} in {category.DisplayName} has been refreshed");
                    }
                }
                catch (Exception ex)
                {
                    ex.HandleCritical("Refreshing game", game);
                    throw;
                }
            }

            RL.Logger?.LogInformationSource($"The displayed game {game} has been refreshed");
        }

        /// <summary>
        /// Refreshes the games
        /// </summary>
        /// <returns>The task</returns>
        public async Task RefreshAsync()
        {
            using (await AsyncLock.LockAsync())
            {
                try
                {
                    RefreshingGames = true;

                    if (Application.Current.Dispatcher == null)
                        throw new Exception("Dispatcher can not be NULL");

                    // Cache the game view models
                    var displayVMCache = new Dictionary<Games, GameDisplayViewModel>();

                    RL.Logger?.LogInformationSource($"All displayed games are being refreshed...");

                    // Refresh all categories
                    foreach (var category in GameCategories)
                    {
                        RL.Logger?.LogInformationSource($"The displayed games in {category.DisplayName.Value} are being refreshed...");

                        try
                        {
                            // Clear collections
                            Application.Current.Dispatcher.Invoke(() => category.InstalledGames.Clear());
                            Application.Current.Dispatcher.Invoke(() => category.NotInstalledGames.Clear());
                            
                            category.AnyInstalledGames = false;
                            category.AnyNotInstalledGames = false;

                            // Enumerate each game
                            foreach (Games game in category.Games)
                            {
                                // Get the game info
                                var info = game.GetGameInfo();

                                // If cached, reuse the view model, otherwise create new and add to cache
                                GameDisplayViewModel displayVM = displayVMCache.ContainsKey(game)
                                    ? displayVMCache[game]
                                    : displayVMCache[game] = info.GetDisplayViewModel();

                                // Check if it has been added
                                if (info.IsAdded)
                                {
                                    // Add the game to the collection
                                    Application.Current.Dispatcher.Invoke(() => category.InstalledGames.Add(displayVM));
                                    category.AnyInstalledGames = true;
                                }
                                else
                                {
                                    Application.Current.Dispatcher.Invoke(() => category.NotInstalledGames.Add(displayVM));
                                    category.AnyNotInstalledGames = true;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            ex.HandleCritical($"Refreshing games in {category.DisplayName}");
                            throw;
                        }

                        RL.Logger?.LogInformationSource($"The displayed games in {category.DisplayName} have been refreshed with {category.InstalledGames.Count} installed and {category.NotInstalledGames.Count} not installed games");
                    }

                    // Allow game finder to run only if there are games which have not been found
                    // ReSharper disable once PossibleNullReferenceException
                    await Application.Current.Dispatcher.InvokeAsync(() =>
                    {
                        RunGameFinderCommand.CanExecuteCommand = GameCategories.Any(x => x.AnyNotInstalledGames);

                        //// NOTE: This is a hacky solution to a weird WPF issue where an item can get duplicated in the view
                        //foreach (var c in GameCategories)
                        //{
                        //    CollectionViewSource.GetDefaultView(c.InstalledGames).Refresh();
                        //    CollectionViewSource.GetDefaultView(c.NotInstalledGames).Refresh();
                        //}
                    });
                }
                catch (Exception ex)
                {
                    ex.HandleCritical("Refreshing games");
                    throw;
                }
                finally
                {
                    RefreshingGames = false;
                }
            }
        }

        /// <summary>
        /// Refreshes the visibility of the categories based on if the games should be categorized
        /// </summary>
        /// <returns>The task</returns>
        public async Task RefreshCategorizedVisibilityAsync()
        {
            using (await AsyncLock.LockAsync())
            {
                try
                {
                    // Get the master category
                    var master = GameCategories.FindItem(x => x.IsMaster);

                    // Set the master category visibility
                    master.IsVisible = false;

                    // Set the categories visibility
                    GameCategories.Where(x => !x.IsMaster).ForEach(x => x.IsVisible = Data.CategorizeGames);

                    // Set the selected index
                    SelectedCategoryIndex = Data.CategorizeGames ? GameCategories.FindItemIndex(x => !x.IsMaster) : GameCategories.FindItemIndex(x => x == master);
                }
                catch (Exception ex)
                {
                    ex.HandleError("Refreshing game category visibility");

                    throw;
                }
            }
        }

        /// <summary>
        /// Runs the game finder
        /// </summary>
        /// <returns>The task</returns>
        public async Task RunGameFinderAsync()
        {
            // Make sure the game finder is not running
            if (App.IsGameFinderRunning)
                return;

            // Run the game finder
            var result = await Task.Run(App.RunGameFinderAsync);

            // Check the result
            if (!result)
                await RCFUI.MessageUI.DisplayMessageAsync(Resources.GameFinder_NoResults, Resources.GameFinder_ResultHeader, MessageType.Information);
        }

        public void Dispose()
        {
            GameCategories?.ForEach(x => x.Dispose());
        }

        #endregion

        #region Event Handlers

        private async void Data_PropertyChangedAsync(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != nameof(Data.CategorizeGames))
                return;

            await RefreshCategorizedVisibilityAsync();
        }

        #endregion
    }
}