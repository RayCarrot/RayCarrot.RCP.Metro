using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using MahApps.Metro.IconPacks;
using Nito.AsyncEx;
using RayCarrot.CarrotFramework.Abstractions;
using RayCarrot.Extensions;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// View model for a game category
    /// </summary>
    public class GameCategoryViewModel : BaseRCPViewModel, IDisposable
    {
        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="games">The games in this category</param>
        /// <param name="displayNameGenerator">The generator for getting the display name</param>
        /// <param name="iconKind">The category icon</param>
        public GameCategoryViewModel(IEnumerable<Games> games, Func<string> displayNameGenerator, PackIconMaterialKind iconKind)
        {
            // Set properties
            Games = games.ToArray();
            DisplayNameGenerator = displayNameGenerator;
            DisplayName = displayNameGenerator();
            IconKind = iconKind;
            
            // Create properties
            AsyncLock = new AsyncLock();
            InstalledGames = new ObservableCollection<GameDisplayViewModel>();
            NotInstalledGames = new ObservableCollection<GameDisplayViewModel>();

            // Enable collection synchronization
            BindingOperations.EnableCollectionSynchronization(InstalledGames, Application.Current);
            BindingOperations.EnableCollectionSynchronization(NotInstalledGames, Application.Current);

            // Subscribe to events
            RCFCore.Data.CultureChanged += Data_CultureChanged;
        }

        #endregion

        #region Private Properties

        /// <summary>
        /// The async lock to use for the game page
        /// </summary>
        private AsyncLock AsyncLock { get; }

        #endregion

        #region Protected Properties

        /// <summary>
        /// The generator for getting the display name
        /// </summary>
        protected Func<string> DisplayNameGenerator { get; }

        #endregion

        #region Public Properties

        /// <summary>
        /// The games in this category
        /// </summary>
        public Games[] Games { get; }

        /// <summary>
        /// The category display name
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// The category icon
        /// </summary>
        public PackIconMaterialKind IconKind { get; }

        /// <summary>
        /// The installed games in this category
        /// </summary>
        public ObservableCollection<GameDisplayViewModel> InstalledGames { get; }

        /// <summary>
        /// The not installed games in this category
        /// </summary>
        public ObservableCollection<GameDisplayViewModel> NotInstalledGames { get; }

        /// <summary>
        /// Indicates if there are any installed games in this category
        /// </summary>
        public bool AnyInstalledGames { get; set; }

        /// <summary>
        /// Indicates if there are any not installed games in this category
        /// </summary>
        public bool AnyNotInstalledGames { get; set; }

        #endregion

        #region Event Handlers

        private void Data_CultureChanged(object sender, PropertyChangedEventArgs<CultureInfo> e)
        {
            DisplayName = DisplayNameGenerator();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Refreshes the added game
        /// </summary>
        /// <returns>The task</returns>
        public async Task RefreshGameAsync(Games game)
        {
            RCFCore.Logger?.LogInformationSource($"The displayed game {game} in {DisplayName} is being refreshed...");

            using (await AsyncLock.LockAsync())
            {
                try
                {
                    // Make sure the game exists in this category
                    if (!Games.Contains(game))
                        throw new Exception("Only games which have been added to the category can be refreshed individually");

                    // Make sure the game has been added
                    if (!game.IsAdded())
                        throw new Exception("Only added games can be refreshed individually");

                    // Get the collection containing the game
                    var collection = InstalledGames.Any(x => x.Game == game) ? InstalledGames : NotInstalledGames;

                    // Get the game index
                    var index = collection.FindItemIndex(x => x.Game == game);

                    // Make sure we got a valid index
                    if (index == -1)
                    {
                        RCFCore.Logger?.LogWarningSource($"The displayed game {game} in {DisplayName} could not be refreshed due to not existing in either game collection");

                        return;
                    }

                    // Refresh the game
                    collection[index] = game.GetGameInfo().GetDisplayViewModel();
                }
                catch (Exception ex)
                {
                    ex.HandleCritical("Refreshing game", game);
                    throw;
                }
            }

            RCFCore.Logger?.LogInformationSource($"The displayed game {game} in {DisplayName} has been refreshed");
        }

        /// <summary>
        /// Refreshes the games
        /// </summary>
        /// <returns>The task</returns>
        public async Task RefreshAsync()
        {
            RCFCore.Logger?.LogInformationSource($"The displayed games in {DisplayName} are being refreshed...");

            using (await AsyncLock.LockAsync())
            {
                try
                {
                    InstalledGames.Clear();
                    NotInstalledGames.Clear();

                    AnyInstalledGames = false;
                    AnyNotInstalledGames = false;

                    // Enumerate each game
                    foreach (Games game in Games)
                    {
                        // Get the game info
                        var info = game.GetGameInfo();

                        // Check if it has been added
                        if (info.IsAdded)
                        {
                            // Add the game to the collection
                            InstalledGames.Add(info.GetDisplayViewModel());
                            AnyInstalledGames = true;
                        }
                        else
                        {
                            NotInstalledGames.Add(info.GetDisplayViewModel());
                            AnyNotInstalledGames = true;
                        }
                    }
                }
                catch (Exception ex)
                {
                    ex.HandleCritical($"Refreshing games in {DisplayName}");
                    throw;
                }
            }

            RCFCore.Logger?.LogInformationSource($"The displayed games in {DisplayName} have been refreshed");
        }

        public void Dispose()
        {
            RCFCore.Data.CultureChanged -= Data_CultureChanged;

            BindingOperations.DisableCollectionSynchronization(InstalledGames);
            BindingOperations.DisableCollectionSynchronization(NotInstalledGames);
        }

        #endregion
    }
}