using System;
using System.Windows;
using RayCarrot.Extensions;
using RayCarrot.UI;
using RayCarrot.Windows.Shell;

namespace RayCarrot.RCP.UI
{
    /// <summary>
    /// Base application view model
    /// </summary>
    /// <typeparam name="Page">The page enum type</typeparam>
    public class BaseRCPAppViewModel<Page> : BaseRCPAppViewModel
        where Page : Enum
    {
        #region Public Properties

        /// <summary>
        /// The currently selected page
        /// </summary>
        public Page SelectedPage { get; set; }

        #endregion
    }

    /// <summary>
    /// Base application view model
    /// </summary>
    public class BaseRCPAppViewModel : BaseViewModel
    {
        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        public BaseRCPAppViewModel()
        {
            // Flag that the startup has begun
            IsStartupRunning = true;

            // Check if the application is running as administrator
            try
            {
                IsRunningAsAdmin = WindowsHelpers.RunningAsAdmin;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error");
                IsRunningAsAdmin = false;
            }
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Indicates if the application is running as administrator
        /// </summary>
        public bool IsRunningAsAdmin { get; }

        /// <summary>
        /// Indicates if the application startup is running
        /// </summary>
        public bool IsStartupRunning { get; set; }

        #endregion
    }
}