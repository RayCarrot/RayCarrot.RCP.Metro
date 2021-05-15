using RayCarrot.Common;
using RayCarrot.IO;
using RayCarrot.Logging;
using RayCarrot.UI;
using RayCarrot.WPF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Nito.AsyncEx;

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
            CanUninstall = gameInfo.CanBeUninstalled;
            PageLoadLock = new AsyncLock();

            // Create the page collection
            var pages = new List<GameOptions_BasePageViewModel>();

            // Add the options page
            pages.Add(new GameOptions_OptionsPageViewModel(Game));

            // Add the progression page
            var progressionViewModel = gameInfo.ProgressionViewModel;

            if (progressionViewModel != null)
                pages.Add(new GameOptions_ProgressionPageViewModel(progressionViewModel));

            // Add the config page
            var configViewModel = gameInfo.ConfigPageViewModel;

            if (configViewModel != null)
                pages.Add(configViewModel);

            // Add the emulator config page
            var emu = gameInfo.Emulator;
            var emuConfigViewModel = emu?.GameConfigViewModel;

            if (emuConfigViewModel != null)
                pages.Add(emuConfigViewModel);

            // Add the utilities page
            var utilities = App.GetUtilities(Game).Select(x => new UtilityViewModel(x)).ToArray();

            if (utilities.Any())
                pages.Add(new GameOptions_UtilitiesPageViewModel(utilities));

            Pages = pages.ToArray();
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// The available options pages
        /// </summary>
        public GameOptions_BasePageViewModel[] Pages { get; }

        /// <summary>
        /// The currently selected page
        /// </summary>
        public GameOptions_BasePageViewModel SelectedPage { get; set; }

        /// <summary>
        /// The game
        /// </summary>
        public Games Game { get; }

        /// <summary>
        /// Indicates if the game can be uninstalled
        /// </summary>
        public bool CanUninstall { get; }

        /// <summary>
        /// The display name
        /// </summary>
        public string DisplayName { get; }

        /// <summary>
        /// The icons source
        /// </summary>
        public string IconSource { get; }

        public AsyncLock PageLoadLock { get; }

        public bool IsLoading { get; set; }

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
        /// Loads the current page
        /// </summary>
        public async Task LoadCurrentPageAsync()
        {
            using (await PageLoadLock.LockAsync())
            {
                // Get the selected page
                var page = SelectedPage;

                // Ignore if already loaded
                if (page.IsLoaded)
                    return;

                try
                {
                    IsLoading = true;

                    // Load the page
                    await page.LoadPageAsync();

                    RL.Logger?.LogInformationSource($"Loaded {page.PageName} page");
                }
                catch (Exception ex)
                {
                    ex.HandleError("Loading page", page);

                    page.SetErrorState(ex);
                }
                finally
                {
                    // Set the page as loaded
                    page.IsLoaded = true;

                    IsLoading = false;
                }
            }
        }

        /// <summary>
        /// Disposes the view model
        /// </summary>
        public void Dispose()
        {
            // Dispose
            Pages?.DisposeAll();
        }

        #endregion
    }
}