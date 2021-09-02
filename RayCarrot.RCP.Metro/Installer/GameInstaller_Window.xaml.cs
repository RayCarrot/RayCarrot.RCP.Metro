using System;
using System.ComponentModel;
using Microsoft.WindowsAPICodePack.Taskbar;
using RayCarrot.Windows.Shell;
using System.Threading.Tasks;
using RayCarrot.Common;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// Interaction logic for GameInstaller.xaml
    /// </summary>
    public partial class GameInstaller_Window : BaseWindow
    {
        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        public GameInstaller_Window(Games game)
        {
            // Initialize components
            InitializeComponent();

            // Set text properties
            Title = $"Install {game.GetGameInfo().DisplayName}";

            // Create the view model
            DataContext = new GameInstaller_ViewModel(game);

            VM.InstallationComplete += VM_InstallationComplete;

            if (RCPServices.Data.ShowProgressOnTaskBar)
                VM.StatusUpdated += VM_StatusUpdated;
        }

        #endregion

        #region Private Properties

        /// <summary>
        /// The view model
        /// </summary>
        private GameInstaller_ViewModel VM => DataContext as GameInstaller_ViewModel;

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
            await Task.Run(VM.AttemptCancelAsync);
        }

        private void VM_StatusUpdated(object sender, OperationProgressEventArgs e)
        {
            Dispatcher?.Invoke(() =>
            {
                // Set the progress
                this.SetTaskbarProgressValue(new Progress(e.Progress.TotalProgress.Percentage, 100, 0));

                // Set the state
                switch (e.State)
                {
                    case OperationState.None:
                        this.SetTaskbarProgressState(TaskbarProgressBarState.NoProgress);
                        break;

                    case OperationState.Running:
                        this.SetTaskbarProgressState(TaskbarProgressBarState.Normal);
                        break;

                    case OperationState.Paused:
                        this.SetTaskbarProgressState(TaskbarProgressBarState.Paused);
                        break;

                    case OperationState.Error:
                        this.SetTaskbarProgressState(TaskbarProgressBarState.Error);
                        break;

                    default:
                        this.SetTaskbarProgressState(TaskbarProgressBarState.NoProgress);
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