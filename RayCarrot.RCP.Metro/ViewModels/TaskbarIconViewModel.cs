using System.Windows;
using System.Windows.Input;
using RayCarrot.CarrotFramework.Abstractions;
using RayCarrot.UI;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// View model for the main window task bar icon
    /// </summary>
    public class TaskbarIconViewModel : BaseRCPViewModel
    {
        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        public TaskbarIconViewModel()
        {
            MinimizeToTaskbarCommand = new RelayCommand(MinimizeToTaskbar);
            OpenFromTaskbarCommand = new RelayCommand(OpenFromTaskbar);
            CloseApplicationCommand = new RelayCommand(CloseApplication);
        }

        #endregion

        #region Public Properties

        public bool IsIconVisible { get; set; }

        #endregion

        #region Commands

        public ICommand MinimizeToTaskbarCommand { get; }

        public ICommand OpenFromTaskbarCommand { get; }

        public ICommand CloseApplicationCommand { get; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Minimizes the application to the task bar
        /// </summary>
        public void MinimizeToTaskbar()
        {
            foreach (Window window in Application.Current.Windows)
                window.Hide();

            IsIconVisible = true;

            RCFCore.Logger?.LogInformationSource("The program has been minimized to tray");
        }

        /// <summary>
        /// Opens the application from the task bar
        /// </summary>
        public void OpenFromTaskbar()
        {
            foreach (Window window in Application.Current.Windows)
                window.Show();

            IsIconVisible = false;

            RCFCore.Logger?.LogInformationSource("The program has been shown from the tray icon");
        }

        /// <summary>
        /// Closes the application
        /// </summary>
        public void CloseApplication()
        {
            RCFCore.Logger?.LogInformationSource("The program is being closed from the tray icon");

            Application.Current.Shutdown();
        }

        #endregion
    }
}