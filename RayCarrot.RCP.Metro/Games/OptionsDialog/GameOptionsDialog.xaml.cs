using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

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
            ViewModel = new GameOptionsDialog_ViewModel(game);

            // Subscribe to events
            Loaded += GameOptions_OnLoadedAsync;
            Closing += GameOptions_OnClosingAsync;
            Closed += Window_Closed;

            // Set default height
            if (ViewModel.Pages.Length > 1)
                Height = 700;
            else
                SizeToContent = SizeToContent.Height;

            // Set default width
            if (ViewModel.Pages.Length >= 4)
                Width = 700;
            else
                Width = 625;
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
        public GameOptionsDialog_ViewModel ViewModel
        {
            get => DataContext as GameOptionsDialog_ViewModel;
            set => DataContext = value;
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

            AppWindowsManager.ShowWindow(() => new GameOptionsDialog(game), AppWindowsManager.ShowWindowFlags.DuplicatesAllowed, groupNames.ToArray());
        }

        #endregion

        #region Event Handlers

        private void GameOptions_OnLoadedAsync(object sender, RoutedEventArgs e)
        {
            Services.App.RefreshRequired += AppGameRefreshRequiredAsync;

            foreach (var page in ViewModel.Pages)
                page.Saved += Page_Saved;
        }

        private void Page_Saved(object sender, EventArgs e)
        {
            if (Services.Data.App_CloseConfigOnSave)
                Close();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            Services.App.RefreshRequired -= AppGameRefreshRequiredAsync;

            foreach (var page in ViewModel.Pages)
                page.Saved -= Page_Saved;

            ViewModel?.Dispose();
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
            if (ForceClose)
                return;

            if (ViewModel.IsLoading)
            {
                e.Cancel = true;
                return;
            }

            var unsavedPage = ViewModel.Pages.FirstOrDefault(x => x.UnsavedChanges);

            if (unsavedPage == null)
                return;

            e.Cancel = true;

            ViewModel.SelectedPage = unsavedPage;

            // Don't show a dialog if the close button is disabled
            if (!IsCloseButtonEnabled)
                return;

            if (!await Services.MessageUI.DisplayMessageAsync(Metro.Resources.GameOptions_UnsavedChanges, Metro.Resources.GameOptions_UnsavedChangesHeader, MessageType.Question, true))
                return;

            ForceClose = true;

            // NOTE: Not the most elegant solution - but we can't call Close() from within a closing event
            _ = Task.Run(() => Dispatcher?.Invoke(Close));
        }

        private async void PagesTabControl_OnSelectionChanged(object sender, SelectionChangedEventArgs e) => await ViewModel.LoadCurrentPageAsync();

        private void CancelButton_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        #endregion
    }
}