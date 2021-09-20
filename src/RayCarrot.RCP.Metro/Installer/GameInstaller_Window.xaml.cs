using Microsoft.WindowsAPICodePack.Taskbar;
using System;
using System.Threading.Tasks;
using System.Windows;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// Interaction logic for GameInstaller.xaml
    /// </summary>
    public partial class GameInstaller_Window : WindowContentControl, IDialogWindowControl<GameInstaller_ViewModel, UserInputResult>
    {
        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        public GameInstaller_Window(Games game)
        {
            // Initialize components
            InitializeComponent();

            // Create the view model
            DataContext = new GameInstaller_ViewModel(game);

            ViewModel.InstallationComplete += VM_InstallationComplete;

            if (Services.Data.UI_ShowProgressOnTaskBar)
                ViewModel.StatusUpdated += VM_StatusUpdated;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// The view model
        /// </summary>
        public GameInstaller_ViewModel ViewModel => DataContext as GameInstaller_ViewModel;

        #endregion

        #region Protected Methods

        protected override void WindowAttached()
        {
            base.WindowAttached();

            WindowInstance.Icon = GenericIconKind.Window_Installer;
        }

        protected override async Task<bool> ClosingAsync()
        {
            if (!await base.ClosingAsync())
                return false;

            if (!ViewModel.InstallerRunning)
            {
                ViewModel.InstallationComplete -= VM_InstallationComplete;
                ViewModel.StatusUpdated -= VM_StatusUpdated;

                // Close window normally
                return true;
            }

            // Attempt to cancel the installation
            Task.Run(ViewModel.AttemptCancelAsync).WithoutAwait("Canceling game installer");

            return true;
        }

        protected override void Closed()
        {
            base.Closed();

            Application.Current.MainWindow?.SetTaskbarProgressState(TaskbarProgressBarState.NoProgress);
        }

        #endregion

        #region Public Methods

        public UserInputResult GetResult()
        {
            return new UserInputResult()
            {
                // TODO: Set if canceled
            };
        }

        #endregion

        #region Event Handlers

        private void VM_StatusUpdated(object sender, OperationProgressEventArgs e)
        {
            Dispatcher?.Invoke(() =>
            {
                var win = Application.Current.MainWindow;

                if (win == null)
                    return;

                // Set the progress
                win.SetTaskbarProgressValue(new Progress(e.Progress.TotalProgress.Percentage, 100, 0));

                // Set the state
                switch (e.State)
                {
                    case OperationState.None:
                        win.SetTaskbarProgressState(TaskbarProgressBarState.NoProgress);
                        break;

                    case OperationState.Running:
                        win.SetTaskbarProgressState(TaskbarProgressBarState.Normal);
                        break;

                    case OperationState.Paused:
                        win.SetTaskbarProgressState(TaskbarProgressBarState.Paused);
                        break;

                    case OperationState.Error:
                        win.SetTaskbarProgressState(TaskbarProgressBarState.Error);
                        break;

                    default:
                        win.SetTaskbarProgressState(TaskbarProgressBarState.NoProgress);
                        break;
                }
            });
        }

        private void VM_InstallationComplete(object sender, EventArgs e)
        {
            WindowInstance.Close();
        }

        #endregion
    }
}