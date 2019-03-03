using System;
using System.ComponentModel;
using System.Threading.Tasks;
using RayCarrot.CarrotFramework;
using System.Windows;

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
            ConfigContentPresenter.Content = game.GetConfigContent(this);
            UtilitiesContentPresenter.Content = game.GetUtilitiesContent();

            Height = ConfigContentPresenter.Content != null ? 700 : 300;
            Width = 600;

            ChangePage(true);
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
        public GameConfigViewModel ConfigViewModel => (ConfigContentPresenter.Content as FrameworkElement)?.DataContext as GameConfigViewModel;

        #endregion

        #region Private Methods

        /// <summary>
        /// Change the current page
        /// </summary>
        /// <param name="config">True to change to config page, false to change to utilities</param>
        private void ChangePage(bool config)
        {
            ConfigButton.Visibility = config ? Visibility.Collapsed : Visibility.Visible;
            UtilitiesButton.Visibility = config && UtilitiesContentPresenter.Content != null ? Visibility.Visible : Visibility.Collapsed;

            ContentTabControl.SelectedIndex = config ? 0 : 1;
        }

        #endregion

        #region Event Handlers

        private async void GameOptions_OnLoadedAsync(object sender, RoutedEventArgs e)
        {
            RCFRCP.App.RefreshRequired += App_RefreshRequired;

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
                ConfigContentPresenter.Content = RCF.Data.CurrentUserLevel >= UserLevel.Technical ? ex.ToString() : null;
            }
        }

        private void App_RefreshRequired(object sender, EventArgs e)
        {
            Close();
        }

        private async void GameOptions_OnClosingAsync(object sender, CancelEventArgs e)
        {
            if (ForceClose || ConfigViewModel?.UnsavedChanges != true)
            {
                RCFRCP.App.RefreshRequired -= App_RefreshRequired;

                if (ConfigViewModel != null)
                    ConfigViewModel.OnSave = null;

                return;
            }

            e.Cancel = true;

            ChangePage(true);

            if (!await RCF.MessageUI.DisplayMessageAsync("Your configuration changes have not been saved. Do you want to exit and discard them?", "Confirm exit", MessageType.Question, true))
                return;

            ForceClose = true;
            _ = Task.Run(() => Dispatcher.Invoke(Close));
        }

        private void UtilitiesButton_OnClick(object sender, RoutedEventArgs e)
        {
            ChangePage(false);
        }

        private void ConfigButton_OnClick(object sender, RoutedEventArgs e)
        {
            ChangePage(true);
        }

        #endregion
    }
}