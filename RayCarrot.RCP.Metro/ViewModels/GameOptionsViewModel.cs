using RayCarrot.UI;
using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using RayCarrot.IO;
using RayCarrot.Common;
using RayCarrot.Logging;
using RayCarrot.WPF;

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
            UninstallCommand = new AsyncRelayCommand(UninstallAsync);
            ShortcutCommand = new AsyncRelayCommand(CreateShortcutAsync);
            
            // Get the info
            var gameInfo = game.GetGameInfo();

            // Set properties
            Game = game;
            DisplayName = gameInfo.DisplayName;
            IconSource = gameInfo.IconSource;
            GameInfoItems = new ObservableCollection<DuoGridItemViewModel>();
            CanUninstall = gameInfo.CanBeUninstalled;

            // Enable collection synchronization
            BindingOperations.EnableCollectionSynchronization(GameInfoItems, this);

            // Refresh the game data
            RefreshGameInfo();

            // Refresh the game data on certain events
            Services.Data.CultureChanged += Data_CultureChanged;
            App.RefreshRequired += App_RefreshRequiredAsync;

            // Check if the launch mode can be changed
            CanChangeLaunchMode = Game.GetManager().SupportsGameLaunchMode;

            // Get the utilities view models
            Utilities = App.GetUtilities(Game).Select(x => new UtilityViewModel(x)).ToArray();

            // Get the UI content, if available
            ConfigContent = gameInfo.ConfigUI;
            OptionsContent = gameInfo.OptionsUI;
            ProgressionViewModel = gameInfo.ProgressionViewModel;
            EmulatorDisplayName = gameInfo.Emulator?.DisplayName;
            EmulatorConfigContent = gameInfo.Emulator?.GameConfigUI;

            HasProgressionContent = ProgressionViewModel != null;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// The game
        /// </summary>
        public Games Game { get; }

        /// <summary>
        /// The game data
        /// </summary>
        public GameData GameData => RCPServices.Data.Games.TryGetValue(Game);

        /// <summary>
        /// The game info items
        /// </summary>
        public ObservableCollection<DuoGridItemViewModel> GameInfoItems { get; }

        /// <summary>
        /// Indicates if the game can be uninstalled
        /// </summary>
        public bool CanUninstall { get; }

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
            get => GameData.LaunchMode;
            set
            {
                GameData.LaunchMode = value;
                _ = App.OnRefreshRequiredAsync(new RefreshRequiredEventArgs(Game, false, true, false, false));
            }
        }

        /// <summary>
        /// The utilities for the game
        /// </summary>
        public UtilityViewModel[] Utilities { get; }

        /// <summary>
        /// Indicates if the game has utilities content
        /// </summary>
        public bool HasUtilities => Utilities.Any();

        /// <summary>
        /// The config content for the game
        /// </summary>
        public object ConfigContent { get; set; }

        /// <summary>
        /// The game config view model
        /// </summary>
        public GameConfigViewModel ConfigViewModel => (ConfigContent as FrameworkElement)?.Dispatcher?.Invoke(() => (ConfigContent as FrameworkElement)?.DataContext as GameConfigViewModel);

        /// <summary>
        /// Indicates if the game has config content
        /// </summary>
        public bool HasConfigContent => ConfigContent != null;

        /// <summary>
        /// The emulator config content for the game
        /// </summary>
        public object EmulatorConfigContent { get; set; }

        /// <summary>
        /// Indicates if the game has emulator config content
        /// </summary>
        public bool HasEmulatorConfigContent => EmulatorConfigContent != null;

        /// <summary>
        /// The display name for the emulator, if available
        /// </summary>
        public LocalizedString EmulatorDisplayName { get; }

        /// <summary>
        /// The progression view model for the game
        /// </summary>
        public BaseProgressionViewModel ProgressionViewModel { get; }

        /// <summary>
        /// Indicates if the game has config content
        /// </summary>
        public bool HasProgressionContent { get; set; }

        /// <summary>
        /// Indicates if the game has options content
        /// </summary>
        public bool HasOptionsContent => OptionsContent != null || CanChangeLaunchMode;

        #endregion

        #region Commands

        /// <summary>
        /// The command for removing the game from the program
        /// </summary>
        public ICommand RemoveCommand { get; }

        /// <summary>
        /// The command for uninstalling the game
        /// </summary>
        public ICommand UninstallCommand { get; }

        /// <summary>
        /// The command for creating a shortcut to launch the game
        /// </summary>
        public ICommand ShortcutCommand { get; }

        #endregion

        #region Event Handlers

        private async Task App_RefreshRequiredAsync(object sender, RefreshRequiredEventArgs e)
        {
            if (e.GameInfoModified)
            {
                // Refresh the game info
                RefreshGameInfo();

                // Reload the config if needed
                if (ConfigViewModel.ReloadOnGameInfoChanged)
                    await ConfigViewModel.SetupAsync();
            }
        }

        private void Data_CultureChanged(object sender, PropertyChangedEventArgs<CultureInfo> e)
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
            GameInfoItems.AddRange(Game.GetManager().GetGameInfoItems);
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
            if (!await Services.MessageUI.DisplayMessageAsync(String.Format(CanUninstall ? Resources.RemoveInstalledGameQuestion : Resources.RemoveGameQuestion, DisplayName), Resources.RemoveGameQuestionHeader,  MessageType.Question, true))
                return;

            // Remove the game
            await RCPServices.App.RemoveGameAsync(Game, false);

            // Refresh
            await RCPServices.App.OnRefreshRequiredAsync(new RefreshRequiredEventArgs(Game, true, false, false, false));
        }

        /// <summary>
        /// Uninstalls the game
        /// </summary>
        /// <returns>The task</returns>
        public async Task UninstallAsync()
        {
            RL.Logger?.LogInformationSource($"{Game} is being uninstalled...");

            // Have user confirm
            if (!await Services.MessageUI.DisplayMessageAsync(String.Format(Resources.UninstallGameQuestion, DisplayName), Resources.UninstallGameQuestionHeader, MessageType.Question, true))
            {
                RL.Logger?.LogInformationSource($"The uninstallation was canceled");

                return;
            }

            try
            {
                // Delete the game directory
                RCPServices.File.DeleteDirectory(Game.GetInstallDir(false));

                RL.Logger?.LogInformationSource($"The game install directory was removed");

                // Get additional uninstall directories
                var dirs = Game.GetGameInfo().UninstallDirectories;

                if (dirs != null)
                {
                    // Delete additional directories
                    foreach (var dir in dirs)
                        RCPServices.File.DeleteDirectory(dir);

                    RL.Logger?.LogInformationSource($"The game additional directories were removed");
                }

                // Get additional uninstall files
                var files = Game.GetGameInfo().UninstallFiles;

                if (files != null)
                {
                    // Delete additional files
                    foreach (var file in files)
                        RCPServices.File.DeleteFile(file);

                    RL.Logger?.LogInformationSource($"The game additional files were removed");
                }
            }
            catch (Exception ex)
            {
                ex.HandleError("Uninstalling game", Game);
                await Services.MessageUI.DisplayExceptionMessageAsync(ex, String.Format(Resources.UninstallGameError, DisplayName), Resources.UninstallGameErrorHeader);

                return;
            }

            // Remove the game
            await RCPServices.App.RemoveGameAsync(Game, true);

            // Refresh
            await RCPServices.App.OnRefreshRequiredAsync(new RefreshRequiredEventArgs(Game, true, false, false, false));

            await Services.MessageUI.DisplaySuccessfulActionMessageAsync(String.Format(Resources.UninstallGameSuccess, DisplayName), Resources.UninstallGameSuccessHeader);
        }

        /// <summary>
        /// Creates a shortcut to launch the game
        /// </summary>
        /// <returns>The task</returns>
        public async Task CreateShortcutAsync()
        {
            try
            {
                var result = await Services.BrowseUI.BrowseDirectoryAsync(new DirectoryBrowserViewModel()
                {
                    DefaultDirectory = Environment.SpecialFolder.Desktop.GetFolderPath(),
                    Title = Resources.GameShortcut_BrowseHeader
                });

                if (result.CanceledByUser)
                    return;

                var shortcutName = String.Format(Resources.GameShortcut_ShortcutName, Game.GetGameInfo().DisplayName);

                Game.GetManager().CreateGameShortcut(shortcutName, result.SelectedDirectory);

                await Services.MessageUI.DisplaySuccessfulActionMessageAsync(Resources.GameShortcut_Success);
            }
            catch (Exception ex)
            {
                ex.HandleError("Creating game shortcut", Game);
                await Services.MessageUI.DisplayExceptionMessageAsync(ex, Resources.GameShortcut_Error, Resources.GameShortcut_ErrorHeader);
            }
        }

        /// <summary>
        /// Disposes the view model
        /// </summary>
        public void Dispose()
        {
            // Unsubscribe events
            Services.Data.CultureChanged -= Data_CultureChanged;
            App.RefreshRequired -= App_RefreshRequiredAsync;

            // Dispose
            ProgressionViewModel?.Dispose();

            // Disable collection synchronization
            BindingOperations.DisableCollectionSynchronization(GameInfoItems);
        }

        #endregion
    }
}