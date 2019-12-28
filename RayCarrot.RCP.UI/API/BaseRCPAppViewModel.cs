using Nito.AsyncEx;
using RayCarrot.CarrotFramework.Abstractions;
using RayCarrot.UI;
using RayCarrot.UserData;
using RayCarrot.Windows.Shell;
using System;
using System.Threading.Tasks;
using System.Windows;

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
        #region Static Constructor

        static BaseRCPAppViewModel()
        {
            WindowsVersion = WindowsHelpers.GetCurrentWindowsVersion();
        }

        #endregion

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

            // Create properties
            SaveUserDataAsyncLock = new AsyncLock();
        }

        #endregion

        #region Private Properties

        /// <summary>
        /// An async lock for the <see cref="SaveUserDataAsync"/> method
        /// </summary>
        private AsyncLock SaveUserDataAsyncLock { get; }

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

        #region Public Static Properties

        /// <summary>
        /// The Windows version the program is running on
        /// </summary>
        public static WindowsVersion WindowsVersion { get; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Saves all user data for the application
        /// </summary>
        public async Task SaveUserDataAsync()
        {
            // Lock the saving of user data
            using (await SaveUserDataAsyncLock.LockAsync())
            {
                // Run it as a new task
                await Task.Run(async () =>
                {
                    // Save all user data
                    try
                    {
                        // Save all user data
                        await RCFData.UserDataCollection.SaveAllAsync();

                        RCFCore.Logger?.LogInformationSource($"The application user data was saved");
                    }
                    catch (Exception ex)
                    {
                        ex.HandleCritical("Saving user data");
                    }
                });
            }
        }

        #endregion
    }
}