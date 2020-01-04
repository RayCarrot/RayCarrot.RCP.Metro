using Nito.AsyncEx;
using RayCarrot.CarrotFramework.Abstractions;
using RayCarrot.UI;
using RayCarrot.UserData;
using RayCarrot.Windows.Shell;
using System;
using System.Threading.Tasks;
using System.Windows;

namespace RayCarrot.RCP.Core
{
    /// <summary>
    /// Base application view model
    /// </summary>
    /// <typeparam name="Page">The page enum type</typeparam>
    public abstract class BaseRCPAppViewModel<Page> : BaseRCPAppViewModel
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
    public abstract class BaseRCPAppViewModel : BaseViewModel
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
        protected BaseRCPAppViewModel()
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

        /// <summary>
        /// A flag indicating if an update check is in progress
        /// </summary>
        public bool CheckingForUpdates { get; set; }

        /// <summary>
        /// The current API version
        /// </summary>
        public Version CurrentAPIVersion => new Version(1, 1, 0, 0);

        #endregion

        #region Public Abstract Properties

        /// <summary>
        /// The current app version
        /// </summary>
        public abstract Version CurrentAppVersion { get; }

        /// <summary>
        /// Indicates if the current version is a beta version
        /// </summary>
        public abstract bool IsBeta { get; }

        /// <summary>
        /// The application base path to use for WPF related operations
        /// </summary>
        public abstract string WPFApplicationBasePath { get; }

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

        /// <summary>
        /// Checks for application updates
        /// </summary>
        /// <param name="isManualSearch">Indicates if this is a manual check, in which cause a message should be shown if no update is found</param>
        /// <returns>The task</returns>
        public async Task CheckForUpdatesAsync(bool isManualSearch)
        {
            if (CheckingForUpdates)
                return;

            try
            {
                CheckingForUpdates = true;

                // Check for updates
                var result = await RCFRCPC.UpdaterManager.CheckAsync(RCFRCPC.Data.ForceUpdate && isManualSearch, RCFRCPC.Data.GetBetaUpdates || IsBeta);

                // Check if there is an error
                if (result.ErrorMessage != null)
                {
                    await RCFUI.MessageUI.DisplayExceptionMessageAsync(result.Exception, result.ErrorMessage, Resources.Update_ErrorHeader);

                    RCFRCPC.Data.IsUpdateAvailable = false;

                    return;
                }

                // Check if no new updates were found
                if (!result.IsNewUpdateAvailable)
                {
                    if (isManualSearch)
                        await RCFUI.MessageUI.DisplayMessageAsync(String.Format(Resources.Update_LatestInstalled, CurrentAppVersion), Resources.Update_LatestInstalledHeader, MessageType.Information);

                    RCFRCPC.Data.IsUpdateAvailable = false;

                    return;
                }

                // Indicate that a new update is available
                RCFRCPC.Data.IsUpdateAvailable = true;

                // Run as new task to mark this operation as finished
                _ = Task.Run(async () =>
                {
                    try
                    {
                        if (await RCFUI.MessageUI.DisplayMessageAsync(!result.IsBetaUpdate ? String.Format(Resources.Update_UpdateAvailable, result.DisplayNews) : Resources.Update_BetaUpdateAvailable, Resources.Update_UpdateAvailableHeader, MessageType.Question, true))
                        {
                            // Launch the updater and run as admin if set to show under installed programs in under to update the Registry key
                            var succeeded = await RCFRCPC.UpdaterManager.UpdateAsync(result, false);
                        }
                    }
                    catch (Exception ex)
                    {
                        ex.HandleError("Updating RCP");
                        await RCFUI.MessageUI.DisplayMessageAsync(Resources.Update_Error, Resources.Update_ErrorHeader, MessageType.Error);
                    }
                });
            }
            finally
            {
                CheckingForUpdates = false;
            }
        }

        #endregion
    }
}