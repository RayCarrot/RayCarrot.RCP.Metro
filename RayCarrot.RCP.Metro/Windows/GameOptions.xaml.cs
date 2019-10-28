using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using RayCarrot.CarrotFramework.Abstractions;
using System.Windows;
using Windows.Management.Deployment;
using RayCarrot.Extensions;
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
        public GameOptions(Games game)
        {
            // Set up UI
            InitializeComponent();
            
            // Create view model
            ViewModel = new GameOptionsViewModel(game);
            
            // Subscribe to events
            Closed += Window_Closed;

            RefreshTabs();

            // Set default height
            if (ViewModel.HasConfigContent || ViewModel.HasUtilities)
                Height = 700;
            else
                SizeToContent = SizeToContent.Height;
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

            WindowHelpers.ShowWindow(() => new GameOptions(game), WindowHelpers.ShowWindowFlags.DuplicatesAllowed, groupNames.ToArray());
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

        private void FrameworkElement_OnLoaded(object sender, RoutedEventArgs e)
        {
            // Set the data context manually due to not being inherited
            sender.CastTo<FrameworkElement>().DataContext = DataContext;
        }

        #endregion
    }
}