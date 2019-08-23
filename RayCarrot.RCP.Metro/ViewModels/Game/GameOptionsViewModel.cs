using RayCarrot.CarrotFramework.Abstractions;
using RayCarrot.UI;
using RayCarrot.Windows.Shell;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// View model for a game options dialog
    /// </summary>
    public class GameOptionsViewModel : BaseRCPViewModel, IDisposable
    {
        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="game">The game to show the options for</param>
        public GameOptionsViewModel(Games game)
        {
            // Create the commands
            RemoveCommand = new AsyncRelayCommand(RemoveAsync);
            ShortcutCommand = new AsyncRelayCommand(CreateShortcutAsync);

            // Set properties
            Game = game;
            DisplayName = game.GetDisplayName();
            IconSource = game.GetIconSource();
            GameInfoItems = new ObservableCollection<DuoGridItemViewModel>();

            // Refresh the game info
            RefreshGameInfo();

            // Refresh the game info on certain events
            RCFCore.Data.CultureChanged += Data_CultureChanged;
            App.RefreshRequired += App_RefreshRequired;

            // Check if the launch mode can be changed
            CanChangeLaunchMode = Game.GetGameManager().SupportsGameLaunchMode;

            // Get the utilities view models
            Utilities = App.GetUtilities(Game).Select(x => new RCPUtilityViewModel(x)).ToArray();

            // Get the options and config content, if available
            ConfigContent = game.GetConfigContent();
            OptionsContent = game.GetOptionsContent();
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// The game
        /// </summary>
        public Games Game { get; }

        /// <summary>
        /// The game info
        /// </summary>
        public GameInfo GameInfo => Game.GetInfo();

        /// <summary>
        /// The game info items
        /// </summary>
        public ObservableCollection<DuoGridItemViewModel> GameInfoItems { get; }

        /// <summary>
        /// The game options content
        /// </summary>
        public object OptionsContent { get; }

        /// <summary>
        /// The display name
        /// </summary>
        public string DisplayName { get; }

        /// <summary>
        /// The icons source
        /// </summary>
        public string IconSource { get; }

        /// <summary>
        /// Indicates if the launch mode can be changed
        /// </summary>
        public bool CanChangeLaunchMode { get; }

        /// <summary>
        /// The game's launch mode
        /// </summary>
        public GameLaunchMode LaunchMode
        {
            get => GameInfo.LaunchMode;
            set
            {
                GameInfo.LaunchMode = value;
                _ = App.OnRefreshRequiredAsync(new RefreshRequiredEventArgs(Game, false, true, false, false));
            }
        }

        /// <summary>
        /// The utilities for the game
        /// </summary>
        public RCPUtilityViewModel[] Utilities { get; }

        /// <summary>
        /// Indicates if the game has utilities content
        /// </summary>
        public bool HasUtilities => Utilities.Any();

        /// <summary>
        /// The config content for the game
        /// </summary>
        public object ConfigContent { get; set; }

        /// <summary>
        /// Indicates if the game has config content
        /// </summary>
        public bool HasConfigContent => ConfigContent != null;

        #endregion

        #region Commands

        /// <summary>
        /// The command for removing the game from the program
        /// </summary>
        public ICommand RemoveCommand { get; }

        /// <summary>
        /// The command for creating a shortcut to launch the game
        /// </summary>
        public ICommand ShortcutCommand { get; }

        #endregion

        #region Event Handlers

        private Task App_RefreshRequired(object sender, RefreshRequiredEventArgs e)
        {
            if (e.GameInfoModified && e.ModifiedGames.Contains(Game))
                RefreshGameInfo();

            return Task.CompletedTask;
        }

        private void Data_CultureChanged(object sender, PropertyChangedEventArgs<System.Globalization.CultureInfo> e)
        {
            RefreshGameInfo();
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Refreshes the game info
        /// </summary>
        private void RefreshGameInfo()
        {
            GameInfoItems.Clear();
            GameInfoItems.AddRange(Game.GetGameManager().GetGameInfoItems());
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Removes the game from the program
        /// </summary>
        /// <returns>The task</returns>
        public async Task RemoveAsync()
        {
            // Ask the user
            if (!await RCFUI.MessageUI.DisplayMessageAsync(String.Format(Resources.RemoveGameQuestion, DisplayName), Resources.RemoveGameQuestionHeader,  MessageType.Question, true))
                return;

            // Remove the game
            await RCFRCP.App.RemoveGameAsync(Game, false);

            // Refresh
            await RCFRCP.App.OnRefreshRequiredAsync(new RefreshRequiredEventArgs(Game, true, false, false, false));
        }

        /// <summary>
        /// Creates a shortcut to launch the game
        /// </summary>
        /// <returns>The task</returns>
        public async Task CreateShortcutAsync()
        {
            try
            {
                var result = await RCFUI.BrowseUI.BrowseDirectoryAsync(new DirectoryBrowserViewModel()
                {
                    DefaultDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                    Title = Resources.GameShortcut_BrowseHeader
                });

                if (result.CanceledByUser)
                    return;

                var gameInfo = Game.GetInfo();
                var shortcutName = String.Format(Resources.GameShortcut_ShortcutName, Game.GetDisplayName());

                if (gameInfo.GameType == GameType.Steam)
                {
                    WindowsHelpers.CreateURLShortcut(shortcutName, result.SelectedDirectory, $@"steam://rungameid/{Game.GetGameManager<SteamGameManager>().GetSteamID()}");

                    RCFCore.Logger?.LogTraceSource($"A shortcut was created for {Game} under {result.SelectedDirectory}");

                    await RCFUI.MessageUI.DisplaySuccessfulActionMessageAsync(Resources.GameShortcut_Success);
                }
                else
                {
                    var launchInfo = Game.GetGameManager().GetLaunchInfo();

                    await RCFRCP.File.CreateFileShortcutAsync(shortcutName, result.SelectedDirectory, launchInfo.Path, launchInfo.Args);
                }
            }
            catch (Exception ex)
            {
                ex.HandleError("Creating game shortcut", Game);
                await RCFUI.MessageUI.DisplayMessageAsync(Resources.GameShortcut_Error, Resources.GameShortcut_ErrorHeader, MessageType.Error);
            }
        }

        /// <summary>
        /// Disposes the view model
        /// </summary>
        public void Dispose()
        {
            RCFCore.Data.CultureChanged -= Data_CultureChanged;
            App.RefreshRequired -= App_RefreshRequired;
        }

        #endregion
    }
}