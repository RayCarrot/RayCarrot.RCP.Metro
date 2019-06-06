using System;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using RayCarrot.CarrotFramework;
using RayCarrot.WPF;
using System.Windows.Input;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// View model for the about page
    /// </summary>
    public class AboutPageViewModel : BaseRCPViewModel
    {
        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        public AboutPageViewModel()
        {
            // Create commands
            OpenUrlCommand = new RelayCommand(x => App.OpenUrl(x?.ToString()));
            ShowVersionHistoryCommand = new RelayCommand(ShowVersionHistory);
            CheckForUpdatesCommand = new AsyncRelayCommand(async () => await App.CheckForUpdatesAsync(true));
            UninstallCommand = new AsyncRelayCommand(UninstallAsync);
        }

        #endregion

        #region Commands

        public ICommand OpenUrlCommand { get; }

        public ICommand ShowVersionHistoryCommand { get; }

        public ICommand CheckForUpdatesCommand { get; }

        public ICommand UninstallCommand { get; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Shows the application version history
        /// </summary>
        public void ShowVersionHistory()
        {
            WindowHelpers.ShowWindow<AppNewsDialog>();
        }

        /// <summary>
        /// Runs the uninstaller
        /// </summary>
        /// <returns>The task</returns>
        public async Task UninstallAsync()
        {
            // Confirm
            if (!await RCF.MessageUI.DisplayMessageAsync(Resources.About_ConfirmUninstall, Resources.About_ConfirmUninstallHeader, MessageType.Question, true))
                return;

            // Run the uninstaller
            if (await RCFRCP.File.LaunchFileAsync(CommonPaths.UninstallFilePath, true, $"\"{Assembly.GetExecutingAssembly().Location}\"") == null)
            {
                string[] appDataLocations = 
                {
                    CommonPaths.UserDataBaseDir,
                    CommonPaths.RegistryBaseKey
                };

                await RCF.MessageUI.DisplayMessageAsync(String.Format(Resources.About_UninstallFailed, appDataLocations.JoinItems(Environment.NewLine)), MessageType.Error);

                return;
            }

            // Shut down the app
            Application.Current.Shutdown();
        }

        #endregion
    }
}