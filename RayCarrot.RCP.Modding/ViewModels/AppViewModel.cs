using System;
using System.Windows;
using RayCarrot.UI;
using RayCarrot.Windows.Shell;

namespace RayCarrot.RCP.Modding
{
    /// <summary>
    /// The main application view model
    /// </summary>
    public class AppViewModel : BaseViewModel
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public AppViewModel()
        {
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

        /// <summary>
        /// Indicates if the application is running as administrator
        /// </summary>
        public bool IsRunningAsAdmin { get; set; }
    }
}