using System;
using System.ComponentModel;
using RayCarrot.CarrotFramework;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// Interaction logic for GameOptions.xaml
    /// </summary>
    public partial class GameOptions : BaseWindow
    {
        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="game">The game to show the options for</param>
        public GameOptions(Games game)
        {
            InitializeComponent();
            ViewModel = new GameOptionsViewModel(game);
            ConfigContentPresenter.Content = Type.GetType($"RayCarrot.RCP.Metro.{game}Config").CreateInstance(this);
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// The window view model
        /// </summary>
        public GameOptionsViewModel ViewModel
        {
            get => DataContext as GameOptionsViewModel;
            set => DataContext = value;
        }

        /// <summary>
        /// The game config view model
        /// </summary>
        public GameConfigViewModel ConfigViewModel => (ConfigContentPresenter.Content as FrameworkElement)?.DataContext as GameConfigViewModel;

        #endregion

        #region Event Handlers

        private async void GameOptions_OnLoadedAsync(object sender, RoutedEventArgs e)
        {
            RCFRCP.App.RefreshRequired += App_RefreshRequired;

            try
            {
                await ConfigViewModel.SetupAsync();
            }
            catch (Exception ex)
            {
                ex.HandleError("Set up game config view model");
                ConfigContentPresenter.Content = RCF.Data.CurrentUserLevel >= UserLevel.Technical ? ex.ToString() : null;
            }
        }

        private void App_RefreshRequired(object sender, EventArgs e)
        {
            Close();
        }

        private async void GameOptions_OnClosingAsync(object sender, CancelEventArgs e)
        {
            if (ConfigViewModel?.UnsavedChanges == true)
            {
                // TODO: Since this is async the window might close before it sets the cancel property
                if (!await RCF.MessageUI.DisplayMessageAsync("Your changes have not been saved. Do you want to exit and discard them?", "Confirm exit", MessageType.Question, true))
                {
                    e.Cancel = true;
                    return;
                }
            }

            RCFRCP.App.RefreshRequired -= App_RefreshRequired;
        }

        #endregion
    }

    /// <summary>
    /// View model for a game options dialog
    /// </summary>
    public class GameOptionsViewModel : BaseViewModel
    {
        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="game">The game to show the options for</param>
        public GameOptionsViewModel(Games game)
        {
            RemoveCommand = new AsyncRelayCommand(RemoveAsync);

            Game = game;
            DisplayName = game.GetDisplayName();
            IconSource = game.GetIconSource();

            var launchInfo = game.GetLaunchInfo();
            LaunchPath = launchInfo.Path;
            LaunchArguments = launchInfo.Args;

            var info = game.GetInfo();
            GameType = info.GameType;
            InstallDirectory = info.InstallDirectory;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// The game
        /// </summary>
        public Games Game { get; }

        /// <summary>
        /// The display name
        /// </summary>
        public string DisplayName { get; }

        /// <summary>
        /// The icons source
        /// </summary>
        public string IconSource { get; }

        /// <summary>
        /// The launch path
        /// </summary>
        public FileSystemPath LaunchPath { get; }

        /// <summary>
        /// The launch arguments
        /// </summary>
        public string LaunchArguments { get; }

        /// <summary>
        /// The game type
        /// </summary>
        public GameType GameType { get; }

        /// <summary>
        /// The install directory
        /// </summary>
        public FileSystemPath InstallDirectory { get; }

        #endregion

        #region Commands

        /// <summary>
        /// The command for removing the game from the program
        /// </summary>
        public ICommand RemoveCommand { get; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Removes the game from the program
        /// </summary>
        /// <returns>The task</returns>
        public async Task RemoveAsync()
        {
            // Ask the user
            if (!await RCF.MessageUI.DisplayMessageAsync($"Are you sure you want to remove {DisplayName} from the Rayman Control Panel? This will not remove the game from " +
                                                        $"your computer or any of its files, including the backups created using this program. Changes made using the utilities " +
                                                        $"may also remain.", "Confirm remove", MessageType.Question, true))
                return;

            // Remove the game
            RCFRCP.Data.Games.Remove(Game);

            // Refresh the games
            RCFRCP.App.OnRefreshRequired();
        }

        #endregion
    }

    /// <summary>
    /// View model for a game configuration
    /// </summary>
    public abstract class GameConfigViewModel : BaseViewModel
    {
        /// <summary>
        /// Indicates if there are any unsaved changes
        /// </summary>
        public bool UnsavedChanges { get; set; }

        /// <summary>
        /// Loads and sets up the current configuration properties
        /// </summary>
        /// <returns>The task</returns>
        public abstract Task SetupAsync();
    }
}