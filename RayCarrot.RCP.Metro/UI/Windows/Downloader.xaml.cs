﻿using RayCarrot.Common;
using RayCarrot.Windows.Shell;
using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// Interaction logic for Downloader.xaml
    /// </summary>
    public partial class Downloader : BaseWindow
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

            if (RCPServices.Data.ShowProgressOnTaskBar)
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

        #region Event Handlers

        private void ViewModel_StatusUpdated(object sender, ValueEventArgs<Progress> e)
        {
            Dispatcher?.Invoke(() =>
            {
                // Set the progress
                this.SetTaskbarProgressValue(new Progress(e.Value.Percentage, 100, 0));
            });
        }

        private void ViewModelDownloadComplete(object sender, EventArgs e)
        {
            Close();
        }

        private async void Downloader_OnLoadedAsync(object sender, RoutedEventArgs e)
        {
            await ViewModel.StartAsync();
        }

        private async void Downloader_OnClosingAsync(object sender, CancelEventArgs e)
        {
            if (!ViewModel.OperationRunning)
            {
                ViewModel.DownloadComplete -= ViewModelDownloadComplete;
                ViewModel.StatusUpdated -= ViewModel_StatusUpdated;

                // Close window normally
                return;
            }

            // Cancel the closing
            e.Cancel = true;

            // Attempt to cancel the installation
            await Task.Run(ViewModel.AttemptCancelAsync);
        }

        #endregion
    }
}