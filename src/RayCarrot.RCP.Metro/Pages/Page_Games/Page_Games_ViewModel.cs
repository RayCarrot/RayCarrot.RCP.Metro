using Nito.AsyncEx;
using RayCarrot.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using NLog;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// View model for the games page
/// </summary>
public class Page_Games_ViewModel : BaseRCPViewModel, IDisposable
{
    #region Constructor

    /// <summary>
    /// Default constructor
    /// </summary>
    public Page_Games_ViewModel()
    {
        // Get categorized games
        var games = App.GetCategorizedGames;
            
        // Create properties
        RefreshingGames = false;
        AsyncLock = new AsyncLock();

        GameCategories = new Page_Games_CategoryViewModel[]
        {
            // Create the master category
            new Page_Games_CategoryViewModel(App.GetGames), 
                
            // Create the categories
            new Page_Games_CategoryViewModel(games[GameCategory.Rayman], new ResourceLocString(nameof(Resources.GamesPage_Category_Rayman)), GenericIconKind.Games_Rayman), 
            new Page_Games_CategoryViewModel(games[GameCategory.Rabbids], new ResourceLocString(nameof(Resources.GamesPage_Category_Rabbids)), GenericIconKind.Games_Rabbids), 
            new Page_Games_CategoryViewModel(games[GameCategory.Demo], new ResourceLocString(nameof(Resources.GamesPage_Category_Demos)), GenericIconKind.Games_Demos),
            new Page_Games_CategoryViewModel(games[GameCategory.Other], new ResourceLocString(nameof(Resources.GamesPage_Category_Other)), GenericIconKind.Games_Other), 
            new Page_Games_CategoryViewModel(games[GameCategory.Fan], new ResourceLocString(nameof(Resources.GamesPage_Category_Fan)), GenericIconKind.Games_FanGames),
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
        RefreshCategorizedVisibility();

        // Refresh on culture changed
        Services.InstanceData.CultureChanged += async (s, e) => await Task.Run(async () => await RefreshAsync());

        // Refresh on startup
        Metro.App.Current.StartupComplete += async (s, e) => await RefreshAsync();

        // Refresh visibility on setting change
        Data.PropertyChanged += Data_PropertyChanged;
    }

    #endregion

    #region Logger

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

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
    public Page_Games_CategoryViewModel[] GameCategories { get; }

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
        Logger.Info("The displayed game {0} is being refreshed...", game);

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
                    Logger.Trace("The displayed game {0} in {1} is being refreshed...", game, category.DisplayName);

                    // Get the collection containing the game
                    var collection = category.InstalledGames.Any(x => x.Game == game) ? category.InstalledGames : category.NotInstalledGames;

                    // Get the game index
                    var index = collection.FindItemIndex(x => x.Game == game);

                    // Make sure we got a valid index
                    if (index == -1)
                    {
                        Logger.Warn("The displayed game {0} in {1} could not be refreshed due to not existing in either game collection", game, category.DisplayName);

                        return;
                    }

                    // Refresh the game
                    Application.Current.Dispatcher.Invoke(() => collection[index] = displayVM);

                    Logger.Trace("The displayed game {0} in {1} has been refreshed", game, category.DisplayName);
                }
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex, "Refreshing game {0}", game);
                throw;
            }
        }

        Logger.Info("The displayed game {0} has been refreshed", game);
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
                var displayVMCache = new Dictionary<Games, Page_Games_GameViewModel>();

                Logger.Info("All displayed games are being refreshed...");

                // Refresh all categories
                foreach (var category in GameCategories)
                {
                    Logger.Info("The displayed games in {0} are being refreshed...", category.DisplayName.Value);

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
                            Page_Games_GameViewModel displayVM = displayVMCache.ContainsKey(game)
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
                        Logger.Fatal(ex, "Refreshing games in {0}", category.DisplayName);
                        throw;
                    }

                    Logger.Info("The displayed games in {0} have been refreshed with {1} installed and {2} not installed games", category.DisplayName, category.InstalledGames.Count, category.NotInstalledGames.Count);
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
                Logger.Fatal(ex, "Refreshing games");
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
    public void RefreshCategorizedVisibility()
    {
        lock (GameCategories)
        {
            try
            {
                // Get the master category
                var master = GameCategories.First(x => x.IsMaster);

                // Set the master category visibility
                master.IsVisible = false;

                // Set the categories visibility
                GameCategories.Where(x => !x.IsMaster).ForEach(x => x.IsVisible = Data.UI_CategorizeGames);

                // Set the selected index
                SelectedCategoryIndex = Data.UI_CategorizeGames ? GameCategories.FindItemIndex(x => !x.IsMaster) : GameCategories.FindItemIndex(x => x == master);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Refreshing game category visibility");

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
            await Services.MessageUI.DisplayMessageAsync(Resources.GameFinder_NoResults, Resources.GameFinder_ResultHeader, MessageType.Information);
    }

    public void Dispose()
    {
        GameCategories?.ForEach(x => x.Dispose());
    }

    #endregion

    #region Event Handlers

    private void Data_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(Data.UI_CategorizeGames))
            return;

        RefreshCategorizedVisibility();
    }

    #endregion
}