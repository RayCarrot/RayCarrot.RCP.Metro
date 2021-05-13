using RayCarrot.WPF;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using RayCarrot.Logging;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// Interaction logic for GameOptions.xaml
    /// </summary>
    public partial class GameOptionsDialog : BaseWindow
    {
        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="game">The game to show the options for</param>
        public GameOptionsDialog(Games game)
        {
            // Set up UI
            InitializeComponent();
            
            // Create view model
            ViewModel = new GameOptionsViewModel(game);

            LoadedPages = new HashSet<GameOptionsPage>();

            // Subscribe to events
            Closed += Window_Closed;

            RefreshTabs();

            // Set default height
            if (ViewModel.HasConfigContent || ViewModel.HasUtilities || ViewModel.HasProgressionContent)
                Height = 700;
            else
                SizeToContent = SizeToContent.Height;

            // Set default width
            if (ViewModel.HasUtilities && ViewModel.HasConfigContent && ViewModel.HasOptionsContent && ViewModel.HasProgressionContent)
                Width = 650;
            else
                Width = 600;
        }

        #endregion

        #region Private Properties

        /// <summary>
        /// Indicates if the window should be closed
        /// even though there are unsaved changes
        /// </summary>
        private bool ForceClose { get; set; }

        private HashSet<GameOptionsPage> LoadedPages { get; }

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

        #endregion

        #region Private Methods

        /// <summary>
        /// Change the current page
        /// </summary>
        /// <param name="page">The page to change to</param>
        private void ChangePage(GameOptionsPage page)
        {
            ContentTabControl.SelectedIndex = (int)page;
        }

        /// <summary>
        /// Refreshes which page is to be shown
        /// </summary>
        private void RefreshTabs()
        {
            if (ViewModel.HasConfigContent)
                ChangePage(GameOptionsPage.Config);
            else if (ViewModel.HasOptionsContent)
                ChangePage(GameOptionsPage.Options);
            else if (ViewModel.HasUtilities)
                ChangePage(GameOptionsPage.Utilities);
            else if (ViewModel.HasEmulatorConfigContent)
                ChangePage(GameOptionsPage.EmulatorConfig);
            else
                ChangePage(GameOptionsPage.Information);
        }

        #endregion

        #region Public Static Methods

        /// <summary>
        /// Shows a new instance of this <see cref="Window"/>
        /// </summary>
        /// <param name="game">The game to show the options for</param>
        public static void Show(Games game)
        {
            var groupNames = new List<string>
            {
                // The same game can only have one dialog opened at a time
                game.ToString()
            };

            // Add game specific group names
            groupNames.AddRange(game.GetGameInfo().DialogGroupNames);

            WindowHelpers.ShowWindow(() => new GameOptionsDialog(game), WindowHelpers.ShowWindowFlags.DuplicatesAllowed, groupNames.ToArray());
        }

        #endregion

        #region Event Handlers

        private async void GameOptions_OnLoadedAsync(object sender, RoutedEventArgs e)
        {
            RCPServices.App.RefreshRequired += AppGameRefreshRequiredAsync;

            try
            {
                foreach (var config in ViewModel.GetGameConfigViewModels)
                {
                    if (config != null)
                    {
                        config.OnSave = () =>
                        {
                            if (RCPServices.Data.CloseConfigOnSave)
                                Close();
                        };

                        await config.SetupAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                ex.HandleError("Set up game config view model");
                ViewModel.ConfigContent = Services.Data.CurrentUserLevel >= UserLevel.Technical ? ex.ToString() : null;
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            RCPServices.App.RefreshRequired -= AppGameRefreshRequiredAsync;

            ViewModel?.Dispose();

            if (ViewModel != null)
            {
                foreach (var config in ViewModel.GetGameConfigViewModels)
                {
                    if (config != null)
                        config.OnSave = null;
                }
            }
        }

        private Task AppGameRefreshRequiredAsync(object sender, RefreshRequiredEventArgs e)
        {
            if (e.GameCollectionModified && e.ModifiedGames.Contains(ViewModel.Game))
            {
                ForceClose = true;
                Close();

                return Task.CompletedTask;
            }

            return Task.CompletedTask;
        }

        private async void GameOptions_OnClosingAsync(object sender, CancelEventArgs e)
        {
            if (ForceClose || ViewModel?.GetGameConfigViewModels.Any(x => x?.UnsavedChanges == true) != true)
                return;

            e.Cancel = true;

            ChangePage(GameOptionsPage.Config);

            if (!await Services.MessageUI.DisplayMessageAsync(Metro.Resources.GameOptions_UnsavedChanges, Metro.Resources.GameOptions_UnsavedChangesHeader, MessageType.Question, true))
                return;

            ForceClose = true;

            // NOTE: Not the most elegant solution - but we can't call Close() from within a closing event
            _ = Task.Run(() => Dispatcher?.Invoke(Close));
        }

        private async void ContentTabControl_OnSelectionChangedAsync(object sender, SelectionChangedEventArgs e)
        {
            var page = (GameOptionsPage)ContentTabControl.SelectedIndex;

            if (LoadedPages.Contains(page))
                return;

            LoadedPages.Add(page);

            if (page == GameOptionsPage.Progression)
            {
                try
                {
                    // Make sure we have a progression view model
                    if (ViewModel.ProgressionViewModel != null)
                    {
                        // Load the progression data
                        await ViewModel.ProgressionViewModel.LoadDataAsync();

                        // Refresh if we have progression content
                        ViewModel.HasProgressionContent =
                            ViewModel.ProgressionViewModel.ProgressionSlots?.Any() == true;

                        RL.Logger?.LogInformationSource($"Loaded game progression");
                    }
                }
                catch (Exception ex)
                {
                    ex.HandleError("Set up game progression view model");
                    ProgressionTab.Content =
                        Services.Data.CurrentUserLevel >= UserLevel.Technical ? ex.ToString() : null;
                }
            }
        }

        #endregion

        #region Enums

        /// <summary>
        /// The available game options pages
        /// </summary>
        public enum GameOptionsPage
        {
            /// <summary>
            /// The game information
            /// </summary>
            Information,

            /// <summary>
            /// The game progress information
            /// </summary>
            Progression,

            /// <summary>
            /// The primary game options
            /// </summary>
            Options,

            /// <summary>
            /// The game configuration
            /// </summary>
            Config,

            /// <summary>
            /// The emulator configuration
            /// </summary>
            EmulatorConfig,

            /// <summary>
            /// The game utilities
            /// </summary>
            Utilities
        }

        #endregion
    }
}