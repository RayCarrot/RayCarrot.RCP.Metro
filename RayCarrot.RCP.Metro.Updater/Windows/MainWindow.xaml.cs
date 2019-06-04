using System;
using System.ComponentModel;

namespace RayCarrot.RCP.Metro.Updater
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : BaseWindow
    {
        #region Constructor

        public MainWindow()
        {
            InitializeComponent();
            ViewModel = new UpdaterViewModel();
        }

        #endregion

        #region Protected Properties

        /// <summary>
        /// The view model
        /// </summary>
        protected UpdaterViewModel ViewModel
        {
            get => DataContext as UpdaterViewModel;
            set => DataContext = value;
        }

        #endregion

        #region Event Handlers

        private async void MainWindow_OnContentRenderedAsync(object sender, EventArgs e)
        {
            // Begin the update
            await ViewModel.UpdateAsync();
        }

        private void MainWindow_OnClosing(object sender, CancelEventArgs e)
        {
            // TODO: Allow operation to be canceled + add cancel button

            // Cancel closing
            e.Cancel = true;
        }

        #endregion
    }
}