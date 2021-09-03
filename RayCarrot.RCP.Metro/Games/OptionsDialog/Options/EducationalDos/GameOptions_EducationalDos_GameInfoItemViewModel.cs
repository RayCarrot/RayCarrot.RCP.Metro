using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using RayCarrot.Common;
using RayCarrot.Logging;
using RayCarrot.UI;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// View model for an education DOSBox game info item
    /// </summary>
    public class GameOptions_EducationalDos_GameInfoItemViewModel : BaseRCPViewModel
    {
        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="parentVM">The parent view model</param>
        /// <param name="gameData">The game info</param>
        /// <param name="name">The game name</param>
        public GameOptions_EducationalDos_GameInfoItemViewModel(GameOptions_EducationalDos_ViewModel parentVM, UserData_EducationalDosBoxGameData gameData, string name)
        {
            ParentVM = parentVM;
            GameData = gameData;
            Name = name;

            EditGameCommand = new AsyncRelayCommand(EditGameAsync);
            OpenLocationCommand = new AsyncRelayCommand(OpenLocationAsync);
            RemoveGameCommand = new AsyncRelayCommand(RemoveGameAsync);
        }

        #endregion

        #region Commands

        public ICommand EditGameCommand { get; }
        public ICommand OpenLocationCommand { get; }
        public ICommand RemoveGameCommand { get; }

        #endregion

        #region Public Properties

        /// <summary>
        /// The parent view model
        /// </summary>
        public GameOptions_EducationalDos_ViewModel ParentVM { get; }

        /// <summary>
        /// The game ID
        /// </summary>
        public UserData_EducationalDosBoxGameData GameData { get; }

        /// <summary>
        /// The game name
        /// </summary>
        public string Name { get; set; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Edits the game
        /// </summary>
        /// <returns>The task</returns>
        public async Task EditGameAsync()
        {
            RL.Logger?.LogInformationSource($"The educational game {GameData.Name} is being edited...");

            // Show the edit dialog and get the result
            var result = await RCPServices.UI.EditEducationalDosGameAsync(new EducationalDosGameEditViewModel(GameData)
            {
                Title = Resources.EducationalOptions_EditHeader
            });

            if (result.CanceledByUser)
                return;

            // Check if any games have the same launch mode
            if (Data.EducationalDosBoxGames.Any(x => x != GameData && x.LaunchMode != null && x.LaunchMode.Equals(result.LaunchMode, StringComparison.CurrentCultureIgnoreCase)))
                await Services.MessageUI.DisplayMessageAsync(Resources.LaunchModeConflict, Resources.LaunchModeConflictHeader, MessageType.Warning);

            // If the name is blank, add default name
            if (result.Name.IsNullOrWhiteSpace())
            {
                result.Name = GameData.InstallDir.Name;
                RL.Logger?.LogInformationSource($"The game name was blank and was defaulted to the installdir name");
            }

            // Update the view model
            Name = result.Name;

            // Update the game
            GameData.Name = result.Name;
            GameData.LaunchMode = result.LaunchMode;
            GameData.MountPath = result.MountPath;

            await Services.MessageUI.DisplaySuccessfulActionMessageAsync(Resources.EducationalOptions_EditSuccess);

            RL.Logger?.LogInformationSource($"The educational game {GameData.Name} has been edited");

            // Refresh
            await App.OnRefreshRequiredAsync(new RefreshRequiredEventArgs(Games.EducationalDos, false, true, true, true));
        }

        /// <summary>
        /// Opens the game location
        /// </summary>
        /// <returns>The task</returns>
        public async Task OpenLocationAsync()
        {
            // Get the install directory
            var instDir = GameData.InstallDir;

            // Select the file in Explorer if it exists
            if ((instDir + GameData.LaunchName).FileExists)
                instDir += GameData.LaunchName;

            // Open the location
            await RCPServices.File.OpenExplorerLocationAsync(instDir);

            RL.Logger?.LogTraceSource($"The educational game {GameData.Name} install location was opened");
        }

        /// <summary>
        /// Removes the game
        /// </summary>
        /// <returns>The task</returns>
        public async Task RemoveGameAsync()
        {
            RL.Logger?.LogInformationSource($"The educational game {GameData.Name} is being removed...");

            // Make sure it's not the last remaining game
            if (ParentVM.GameItems.Count <= 1)
            {
                RL.Logger?.LogInformationSource($"The educational game could not be removed due to being the last remaining game");

                await Services.MessageUI.DisplayMessageAsync(Resources.EducationalOptions_RemoveErrorLastOne, Resources.EducationalOptions_RemoveErrorLastOneHeader, MessageType.Error);
                return;
            }

            // Ask the user
            if (!await Services.MessageUI.DisplayMessageAsync(String.Format(Resources.RemoveGameQuestion, Name), Resources.RemoveGameQuestionHeader, MessageType.Question, true))
                return;

            // Check if the game is the default game
            var isDefault = ParentVM.GameItems.First() == this;

            // Remove the game
            ParentVM.GameItems.Remove(this);
            Data.EducationalDosBoxGames.Remove(GameData);

            RL.Logger?.LogInformationSource($"The educational game has been removed");

            // Refresh the default if this game was the default
            if (isDefault)
            {
                Games.EducationalDos.GetManager<GameManager_EducationalDOSBox>().RefreshDefault();
                RL.Logger?.LogInformationSource($"The educational game was the default and it has now been refreshed");
            }

            // Refresh
            await App.OnRefreshRequiredAsync(new RefreshRequiredEventArgs(Games.EducationalDos, false, true, true, true));
        }

        #endregion
    }
}