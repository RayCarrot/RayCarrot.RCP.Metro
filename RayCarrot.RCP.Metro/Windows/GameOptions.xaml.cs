using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using RayCarrot.CarrotFramework.Abstractions;
using System.Windows;
using RayCarrot.UI;
using RayCarrot.WPF;

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
        /// <param name="page">The page to show</param>
        public GameOptions(Games game, GameOptionsPage page)
        {
            InitializeComponent();
            ViewModel = new GameOptionsViewModel(game);
            Closed += Window_Closed;

            ChangePage(page);
        }

        #endregion

        #region Private Properties

        /// <summary>
        /// Indicates if the window should be closed
        /// even though there are unsaved changes
        /// </summary>
        private bool ForceClose { get; set; }

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
        public GameConfigViewModel ConfigViewModel => (ViewModel.ConfigContent as FrameworkElement)?.DataContext as GameConfigViewModel;

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

        #endregion

        #region Public Static Methods

        /// <summary>
        /// Shows a new instance of this <see cref="Window"/>
        /// </summary>
        /// <param name="game">The game to show the options for</param>
        /// <param name="page">The page to show</param>
        public static void Show(Games game, GameOptionsPage page)
        {
            var groupNames = new List<string>
            {
                // The same game can only have one dialog opened at a time
                game.ToString()
            };

            // Add game specific group names
            groupNames.AddRange(game.GetGameInfo().DialogGroupNames);

            WindowHelpers.ShowWindow(() => new GameOptions(game, page), WindowHelpers.ShowWindowFlags.DuplicatesAllowed, groupNames.ToArray());
        }

        #endregion

        #region Event Handlers

        private async void GameOptions_OnLoadedAsync(object sender, RoutedEventArgs e)
        {
            RCFRCP.App.RefreshRequired += AppGameRefreshRequiredAsync;

            try
            {
                if (ConfigViewModel == null)
                    return;

                ConfigViewModel.OnSave = () =>
                {
                    if (RCFRCP.Data.CloseConfigOnSave)
                        Close();
                };

                await ConfigViewModel.SetupAsync();
            }
            catch (Exception ex)
            {
                ex.HandleError("Set up game config view model");
                ViewModel.ConfigContent = RCFCore.Data.CurrentUserLevel >= UserLevel.Technical ? ex.ToString() : null;
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            RCFRCP.App.RefreshRequired -= AppGameRefreshRequiredAsync;

            ViewModel?.Dispose();

            if (ConfigViewModel != null)
                ConfigViewModel.OnSave = null;
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
            if (ForceClose || ConfigViewModel?.UnsavedChanges != true)
                return;

            e.Cancel = true;

            ChangePage(GameOptionsPage.Config);

            if (!await RCFUI.MessageUI.DisplayMessageAsync(Metro.Resources.GameOptions_UnsavedChanges, Metro.Resources.GameOptions_UnsavedChangesHeader, MessageType.Question, true))
                return;

            ForceClose = true;

            // NOTE: Not the most elegant solution - but we can't call Close() from within a closing event
            _ = Task.Run(() => Dispatcher?.Invoke(Close));
        }

        #endregion
    }
}