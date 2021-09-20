using System;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.WindowsAPICodePack.Taskbar;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// Interaction logic for Downloader.xaml
    /// </summary>
    public partial class Downloader : WindowContentControl, IDialogWindowControl<DownloaderViewModel, DownloaderResult>
    {
        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="viewModel">The view model</param>
        public Downloader(DownloaderViewModel viewModel)
        {
            InitializeComponent();

            ViewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));

            ViewModel.DownloadComplete += ViewModelDownloadComplete;
            Loaded += Downloader_OnLoadedAsync;

            if (Services.Data.UI_ShowProgressOnTaskBar)
                ViewModel.StatusUpdated += ViewModel_StatusUpdated;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// The view model
        /// </summary>
        public DownloaderViewModel ViewModel
        {
            get => DataContext as DownloaderViewModel;
            set => DataContext = value;
        }

        #endregion

        #region Protected Methods

        protected override void WindowAttached()
        {
            base.WindowAttached();

            WindowInstance.Icon = GenericIconKind.Window_Downloader;
        }

        protected override async Task<bool> ClosingAsync()
        {
            if (!await base.ClosingAsync())
                return false;

            if (!ViewModel.OperationRunning)
            {
                ViewModel.DownloadComplete -= ViewModelDownloadComplete;
                ViewModel.StatusUpdated -= ViewModel_StatusUpdated;

                // Close window normally
                return true;
            }
            
            // Attempt to cancel the installation (run async without awaiting, but still log any exceptions)
            Task.Run(ViewModel.AttemptCancelAsync).WithoutAwait(exceptionLogMessage: "Error canceling downloader");

            // Cancel the closing
            return false;
        }

        protected override void Closed()
        {
            base.Closed();

            Application.Current.MainWindow?.SetTaskbarProgressState(TaskbarProgressBarState.NoProgress);
        }

        #endregion

        #region Public Methods

        public DownloaderResult GetResult()
        {
            return new DownloaderResult(ViewModel.CurrentDownloadState);
        }

        #endregion

        #region Event Handlers

        private void ViewModel_StatusUpdated(object sender, ValueEventArgs<Progress> e)
        {
            Dispatcher?.Invoke(() =>
            {
                // Set the progress on the main window
                Application.Current.MainWindow?.SetTaskbarProgressValue(new Progress(e.Value.Percentage, 100, 0));
            });
        }

        private void ViewModelDownloadComplete(object sender, EventArgs e)
        {
            WindowInstance.Close();
        }

        private async void Downloader_OnLoadedAsync(object sender, RoutedEventArgs e)
        {
            Loaded -= Downloader_OnLoadedAsync;

            await ViewModel.StartAsync();
        }

        #endregion
    }
}