using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using RayCarrot.Extensions;
using RayCarrot.UI;
using DragDrop = GongSolutions.Wpf.DragDrop.DragDrop;

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
            GameItems = Data.EducationalDosBoxGames.Select(x => new EducationalDosBoxGameInfoViewModel(this, x, x.Name)).ToObservableCollection();

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
        public ObservableCollection<EducationalDosBoxGameInfoViewModel> GameItems { get; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Adds a new educational game to the app
        /// </summary>
        /// <returns>The task</returns>
        public async Task AddGameAsync()
        {
            // TODO: Log

            // Get the manager
            var manager = Games.EducationalDos.GetGameManager<EducationalDosBoxGameManager>();

            // Locate the new game
            var path = await manager.LocateAsync();

            if (path == null)
                return;

            // Add the game
            var newItem = manager.AddEducationalDosBoxGameInfo(path.Value);

            // Create the view model
            var vm = new EducationalDosBoxGameInfoViewModel(this, newItem, newItem.Name);

            // Add the view model
            GameItems.Add(vm);

            // Refresh
            await App.OnRefreshRequiredAsync(new RefreshRequiredEventArgs(Games.EducationalDos, false, true, true, true));

            // Edit the game
            await vm.EditGameAsync();
        }

        /// <summary>
        /// Saves the changes made from the game items
        /// </summary>
        /// <returns>The task</returns>
        public async Task SaveAsync()
        {
            // TODO: Log

            // Clear the games
            Data.EducationalDosBoxGames.Clear();

            // Add the games
            Data.EducationalDosBoxGames.AddRange(GameItems.Select(x => new EducationalDosBoxGameInfo(x.GameInfo.ID, x.GameInfo.InstallDIr, x.GameInfo.LaunchName)
            {
                LaunchMode = x.GameInfo.LaunchMode,
                MountPath = x.GameInfo.MountPath,
                Name = x.GameInfo.Name
            }));

            // Refresh the default game
            Games.EducationalDos.GetGameManager<EducationalDosBoxGameManager>().RefreshDefault();

            // Refresh
            await App.OnRefreshRequiredAsync(new RefreshRequiredEventArgs(Games.EducationalDos, false, true, true, true));
        }

        #endregion
    }
}