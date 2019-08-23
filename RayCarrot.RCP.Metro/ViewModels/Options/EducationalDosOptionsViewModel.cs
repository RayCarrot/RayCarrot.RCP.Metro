using RayCarrot.CarrotFramework.Abstractions;
using RayCarrot.Extensions;
using RayCarrot.UI;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using RayCarrot.IO;

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
            RCFCore.Logger?.LogInformationSource($"A new educational game is being added...");

            // Get the manager
            var manager = Games.EducationalDos.GetGameManager<EducationalDosBoxGameManager>();

            // Locate the new game
            var path = await manager.LocateAsync();

            if (path == null)
                return;

            // Get the game info
            var newItem = manager.GetNewEducationalDosBoxGameInfo(path.Value);

            // Check that if it shares the same ID as an existing game that the install directory is the same
            if (RCFRCP.Data.EducationalDosBoxGames.Any(x => x.ID == newItem.ID && x.InstallDir.CorrectPathCasing() != newItem.InstallDir.CorrectPathCasing()))
            {
                await RCFUI.MessageUI.DisplayMessageAsync(Resources.EducationalOptions_AddNewDuplicateIDError, Resources.EducationalOptions_AddNewDuplicateIDErrorHeader, MessageType.Error);

                return;
            }

            // Add the game to the list of educational games
            RCFRCP.Data.EducationalDosBoxGames.Add(newItem);

            // Create the view model
            var vm = new EducationalDosBoxGameInfoViewModel(this, newItem, newItem.Name);

            // Add the view model
            GameItems.Add(vm);

            RCFCore.Logger?.LogInformationSource($"A new educational game has been added with the ID {newItem.ID}");

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
            RCFCore.Logger?.LogInformationSource($"The educational game options are saving...");

            // Clear the games
            Data.EducationalDosBoxGames.Clear();

            // Add the games
            Data.EducationalDosBoxGames.AddRange(GameItems.Select(x => new EducationalDosBoxGameInfo(x.GameInfo.ID, x.GameInfo.InstallDir, x.GameInfo.LaunchName)
            {
                LaunchMode = x.GameInfo.LaunchMode,
                MountPath = x.GameInfo.MountPath,
                Name = x.GameInfo.Name
            }));

            // Refresh the default game
            Games.EducationalDos.GetGameManager<EducationalDosBoxGameManager>().RefreshDefault();

            // Refresh
            await App.OnRefreshRequiredAsync(new RefreshRequiredEventArgs(Games.EducationalDos, false, true, true, true));

            RCFCore.Logger?.LogInformationSource($"The educational game options have saved");
        }

        #endregion
    }
}