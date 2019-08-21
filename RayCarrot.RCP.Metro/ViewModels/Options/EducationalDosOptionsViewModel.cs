using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using RayCarrot.Extensions;
using RayCarrot.UI;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// View model for the educational DOS games options
    /// </summary>
    public class EducationalDosOptionsViewModel : BaseRCPViewModel
    {
        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        public EducationalDosOptionsViewModel()
        {
            // Get the games
            Games = Data.EducationalDosBoxGames.Select(x => new EducationalDosBoxGameInfoViewModel(this, x, x.Name)).ToObservableCollection();

            // Refresh when the view model collection changes
            Games.CollectionChanged += async (s, e) =>
            {
                // NOTE: This gets hit twice times when reordering...

                // Clear the games
                Data.EducationalDosBoxGames.Clear();

                // Add the games
                Data.EducationalDosBoxGames.AddRange(Games.Select(x => new EducationalDosBoxGameInfo(x.GameInfo.ID, x.GameInfo.InstallDIr, x.GameInfo.LaunchName)
                {
                    LaunchMode = x.GameInfo.LaunchMode,
                    MountPath = x.GameInfo.MountPath,
                    Name = x.GameInfo.Name
                }));

                // Get the current launch mode
                var launchMode = Metro.Games.EducationalDos.GetInfo().LaunchMode;

                // Reset the game info with new install directory
                Data.Games[Metro.Games.EducationalDos] = new GameInfo(GameType.EducationalDosBox, Games.First().GameInfo.InstallDIr)
                {
                    LaunchMode = launchMode
                };

                // Refresh
                await App.OnRefreshRequiredAsync(new RefreshRequiredEventArgs(Metro.Games.EducationalDos, false, true, true, true));
            };

            // Create the commands
            AddGameCommand = new AsyncRelayCommand(AddGameAsync);
        }

        #endregion

        #region Commands

        public ICommand AddGameCommand { get; }

        #endregion

        #region Public Properties

        /// <summary>
        /// The educational games
        /// </summary>
        public ObservableCollection<EducationalDosBoxGameInfoViewModel> Games { get; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Adds a new educational game to the app
        /// </summary>
        /// <returns>The task</returns>
        public async Task AddGameAsync()
        {
            // Get the manager
            var manager = Metro.Games.EducationalDos.GetGameManager<EducationalDosBoxGameManager>();

            // Locate the new game
            var path = await manager.LocateAsync();

            if (path == null)
                return;

            // Add the game
            manager.AddEducationalDosBoxGameInfo(path.Value);

            // Get the new game info
            var newItem = Data.EducationalDosBoxGames.Last();

            // Create the view model
            var vm = new EducationalDosBoxGameInfoViewModel(this, newItem, newItem.Name);

            // Add the view model
            Games.Add(vm);

            // Edit the game
            await vm.EditGameAsync();
        }

        #endregion
    }
}