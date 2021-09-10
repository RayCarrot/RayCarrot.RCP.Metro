﻿using Nito.AsyncEx;
using RayCarrot.IO;
using NLog;
using RayCarrot.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// View model for a game options dialog
    /// </summary>
    public class GameOptionsDialog_ViewModel : BaseRCPViewModel, IDisposable
    {
        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="game">The game to show the options for</param>
        public GameOptionsDialog_ViewModel(Games game)
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
            var pages = new List<GameOptionsDialog_BasePageViewModel>();

            // Add the options page
            pages.Add(new GameOptionsDialog_OptionsPageViewModel(Game));

            // Add the progression page
            var progressionViewModel = gameInfo.ProgressionViewModel;

            if (progressionViewModel != null)
                pages.Add(new GameOptionsDialog_ProgressionPageViewModel(progressionViewModel));

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
                pages.Add(new GameOptionsDialog_UtilitiesPageViewModel(utilities));

            Pages = pages.ToArray();

            SelectedPage = Pages.FirstOrDefault();
        }

        #endregion

        #region Logger

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        #endregion

        #region Public Properties

        /// <summary>
        /// The available options pages
        /// </summary>
        public GameOptionsDialog_BasePageViewModel[] Pages { get; }

        /// <summary>
        /// The currently selected page
        /// </summary>
        public GameOptionsDialog_BasePageViewModel SelectedPage { get; set; }

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
            await Services.App.RemoveGameAsync(Game, false);

            // Refresh
            await Services.App.OnRefreshRequiredAsync(new RefreshRequiredEventArgs(Game, RefreshFlags.GameCollection));
        }

        /// <summary>
        /// Uninstalls the game
        /// </summary>
        /// <returns>The task</returns>
        public async Task UninstallAsync()
        {
            Logger.Info("{0} is being uninstalled...", Game);

            // Have user confirm
            if (!await Services.MessageUI.DisplayMessageAsync(String.Format(Resources.UninstallGameQuestion, DisplayName), Resources.UninstallGameQuestionHeader, MessageType.Question, true))
            {
                Logger.Info("The uninstallation was canceled");

                return;
            }

            try
            {
                // Delete the game directory
                Services.File.DeleteDirectory(Game.GetInstallDir(false));

                Logger.Info("The game install directory was removed");

                // Get additional uninstall directories
                var dirs = Game.GetGameInfo().UninstallDirectories;

                if (dirs != null)
                {
                    // Delete additional directories
                    foreach (var dir in dirs)
                        Services.File.DeleteDirectory(dir);

                    Logger.Info("The game additional directories were removed");
                }

                // Get additional uninstall files
                var files = Game.GetGameInfo().UninstallFiles;

                if (files != null)
                {
                    // Delete additional files
                    foreach (var file in files)
                        Services.File.DeleteFile(file);

                    Logger.Info("The game additional files were removed");
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Uninstalling game {0}", Game);
                await Services.MessageUI.DisplayExceptionMessageAsync(ex, String.Format(Resources.UninstallGameError, DisplayName), Resources.UninstallGameErrorHeader);

                return;
            }

            // Remove the game
            await Services.App.RemoveGameAsync(Game, true);

            // Refresh
            await Services.App.OnRefreshRequiredAsync(new RefreshRequiredEventArgs(Game, RefreshFlags.GameCollection));

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
                Logger.Error(ex, "Creating game shortcut {0}", Game);
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

                    Logger.Info("Loaded {0} page", page.PageName);
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "Loading page {0}", page);

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