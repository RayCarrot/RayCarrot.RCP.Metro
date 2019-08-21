using System.Threading.Tasks;
using System.Windows.Input;
using RayCarrot.Extensions;
using RayCarrot.UI;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// View model for an education DOSBox game info item
    /// </summary>
    public class EducationalDosBoxGameInfoViewModel : BaseRCPViewModel
    {
        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="parentVM">The parent view model</param>
        /// <param name="gameInfo">The game info</param>
        /// <param name="name">The game name</param>
        public EducationalDosBoxGameInfoViewModel(EducationalDosOptionsViewModel parentVM, EducationalDosBoxGameInfo gameInfo, string name)
        {
            ParentVM = parentVM;
            GameInfo = gameInfo;
            Name = name;

            EditGameCommand = new AsyncRelayCommand(EditGameAsync);
            RemoveGameCommand = new AsyncRelayCommand(RemoveGameAsync);
        }

        #endregion

        #region Commands

        public ICommand EditGameCommand { get; }

        public ICommand RemoveGameCommand { get; }

        #endregion

        #region Public Properties

        /// <summary>
        /// The parent view model
        /// </summary>
        public EducationalDosOptionsViewModel ParentVM { get; }

        /// <summary>
        /// The game ID
        /// </summary>
        public EducationalDosBoxGameInfo GameInfo { get; }

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
            // Show the edit dialog and get the result
            var result = await RCFRCP.UI.EditEducationalDosGameAsync(new EducationalDosGameEditViewModel(GameInfo)
            {
                // TODO: Localize
                Title = "Edit game"
            });

            if (result.CanceledByUser)
                return;

            // If the name is blank, add default name
            if (result.Name.IsNullOrWhiteSpace())
                result.Name = GameInfo.InstallDIr.Name;

            // Update the view model
            Name = result.Name;

            // Update the game
            GameInfo.Name = result.Name;
            GameInfo.LaunchMode = result.LaunchMode;
            GameInfo.MountPath = result.MountPath;

            // Refresh
            await App.OnRefreshRequiredAsync(new RefreshRequiredEventArgs(Games.EducationalDos, false, true, true, true));
        }

        /// <summary>
        /// Removes the game
        /// </summary>
        /// <returns>The task</returns>
        public async Task RemoveGameAsync()
        {
            // TODO: Localize

            // Make sure it's not the last remaining game
            if (ParentVM.Games.Count <= 1)
            {
                await RCFUI.MessageUI.DisplayMessageAsync("The has to be at least one game available to launch", "Error removing game", MessageType.Error);
                return;
            }

            // Have user confirm
            if (!await RCFUI.MessageUI.DisplayMessageAsync("", "", MessageType.Question, true))
            {
                return;
            }

            // Remove the game
            ParentVM.Games.Remove(this);
        }

        #endregion
    }
}