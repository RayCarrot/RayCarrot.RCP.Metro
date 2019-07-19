using System;
using System.ComponentModel;
using System.Windows.Shell;
using RayCarrot.CarrotFramework.Abstractions;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// Interaction logic for GameInstaller.xaml
    /// </summary>
    public partial class GameInstaller : BaseWindow
    {
        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        public GameInstaller(Games game)
        {
            // Initialize components
            InitializeComponent();

            // Set text properties
            Title = $"Install {game.GetDisplayName()}";
            TaskbarItemInfo = new TaskbarItemInfo();

            // Create the view model
            DataContext = new GameInstallerViewModel(game);

            VM.InstallationComplete += VM_InstallationComplete;

            if (RCFRCP.Data.ShowProgressOnTaskBar)
                VM.StatusUpdated += VM_StatusUpdated;
        }

        #endregion

        #region Private Properties

        /// <summary>
        /// The view model
        /// </summary>
        private GameInstallerViewModel VM => DataContext as GameInstallerViewModel;

        #endregion

        #region Event Handlers

        private async void GameInstaller_OnClosingAsync(object sender, CancelEventArgs e)
        {
            if (!VM.InstallerRunning)
            {
                VM.InstallationComplete -= VM_InstallationComplete;
                VM.StatusUpdated -= VM_StatusUpdated;

                // Close window normally
                return;
            }

            // Cancel the closing
            e.Cancel = true;

            // Attempt to cancel the installation
            await VM.AttemptCancelAsync();
        }

        private void VM_StatusUpdated(object sender, OperationProgressEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                // Set the progress
                TaskbarItemInfo.ProgressValue = e.Progress.TotalProgress.Percentage / 100;

                // Set the state
                switch (e.State)
                {
                    case OperationState.None:
                        TaskbarItemInfo.ProgressState = TaskbarItemProgressState.None;
                        break;

                    case OperationState.Running:
                        TaskbarItemInfo.ProgressState = TaskbarItemProgressState.Normal;
                        break;

                    case OperationState.Paused:
                        TaskbarItemInfo.ProgressState = TaskbarItemProgressState.Paused;
                        break;

                    case OperationState.Error:
                        TaskbarItemInfo.ProgressState = TaskbarItemProgressState.Error;
                        break;

                    default:
                        TaskbarItemInfo.ProgressState = TaskbarItemProgressState.None;
                        break;
                }
            });
        }

        private void VM_InstallationComplete(object sender, EventArgs e)
        {
            Close();
        }

        #endregion
    }
}