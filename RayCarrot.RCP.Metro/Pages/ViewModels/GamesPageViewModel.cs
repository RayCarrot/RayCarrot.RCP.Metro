using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using MahApps.Metro.IconPacks;
using RayCarrot.CarrotFramework.Abstractions;
using RayCarrot.Extensions;
using RayCarrot.UI;

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

            GameCategories = new GameCategoryViewModel[]
            {
                new GameCategoryViewModel(games[GameCategory.Rayman], () => Resources.GamesPage_Category_Rayman, PackIconMaterialKind.GamepadVariant), 
                new GameCategoryViewModel(games[GameCategory.Rabbids], () => Resources.GamesPage_Category_Rabbids, PackIconMaterialKind.GamepadVariant), 
                //new GameCategoryViewModel(games[GameCategory.Demo], () => Resources.GamesPage_Category_Demos, PackIconMaterialKind.ShoppingMusic), 
                new GameCategoryViewModel(games[GameCategory.Other], () => Resources.GamesPage_Category_Other, PackIconMaterialKind.Buffer), 
                new GameCategoryViewModel(games[GameCategory.Fan], () => Resources.GamesPage_Category_Fan, PackIconMaterialKind.Earth), 
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

            // Refresh on culture changed
            RCFCore.Data.CultureChanged += async (s, e) => await Task.Run(async () => await RefreshAsync());

            // Refresh on startup
            Metro.App.Current.StartupComplete += async (s, e) => await RefreshAsync();
        }

        #endregion

        #region Commands

        public AsyncRelayCommand RunGameFinderCommand { get; }

        public AsyncRelayCommand RefreshGamesCommand { get; }

        #endregion

        #region Public Properties

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
            await GameCategories.FindItem(x => x.Games.Contains(game)).RefreshGameAsync(game);
        }

        /// <summary>
        /// Refreshes the games
        /// </summary>
        /// <returns>The task</returns>
        public async Task RefreshAsync()
        {
            try
            {
                RefreshingGames = true;

                // Refresh all categories
                foreach (var category in GameCategories)
                    await category.RefreshAsync();

                // Allow game finder to run only if there are games which have not been found
                // ReSharper disable once PossibleNullReferenceException
                await Application.Current.Dispatcher.InvokeAsync(() => RunGameFinderCommand.CanExecuteCommand = GameCategories.Any(x => x.AnyNotInstalledGames));
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
    }
}