using RayCarrot.UI;
using System;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Input;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// View model for the about page
    /// </summary>
    public class Page_About_ViewModel : BaseRCPViewModel
    {
        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        public Page_About_ViewModel()
        {
            // Create commands
            OpenUrlCommand = new RelayCommand(x => App.OpenUrl(x?.ToString()));
            ShowVersionHistoryCommand = new RelayCommand(ShowVersionHistory);
            CheckForUpdatesCommand = new AsyncRelayCommand(async () => await App.CheckForUpdatesAsync(true));
            UninstallCommand = new AsyncRelayCommand(UninstallAsync);

            // Refresh the update badge property based on if new update is available
            Data.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(Data.IsUpdateAvailable))
                    OnPropertyChanged(nameof(UpdateBadge));
            };
        }

        #endregion

        #region Commands

        public ICommand OpenUrlCommand { get; }

        public ICommand ShowVersionHistoryCommand { get; }

        public ICommand CheckForUpdatesCommand { get; }

        public ICommand UninstallCommand { get; }

        #endregion

        #region Public Properties

        /// <summary>
        /// The update badge, indicating if new updates are available
        /// </summary>
        public string UpdateBadge => Data.IsUpdateAvailable ? "1" : null;

        #endregion

        #region Public Methods

        /// <summary>
        /// Shows the application version history
        /// </summary>
        public void ShowVersionHistory()
        {
            AppWindowsManager.ShowWindow<AppNewsDialog>();
        }

        /// <summary>
        /// Runs the uninstaller
        /// </summary>
        /// <returns>The task</returns>
        public async Task UninstallAsync()
        {
            // Confirm
            if (!await Services.MessageUI.DisplayMessageAsync(Resources.About_ConfirmUninstall, Resources.About_ConfirmUninstallHeader, MessageType.Question, true))
                return;

            // Run the uninstaller
            if (await RCPServices.File.LaunchFileAsync(AppFilePaths.UninstallFilePath, true, $"\"{Assembly.GetEntryAssembly()?.Location}\"") == null)
            {
                string[] appDataLocations = 
                {
                    AppFilePaths.UserDataBaseDir,
                    AppFilePaths.RegistryBaseKey
                };

                await Services.MessageUI.DisplayMessageAsync(String.Format(Resources.About_UninstallFailed, appDataLocations.JoinItems(Environment.NewLine)), MessageType.Error);

                return;
            }

            // Shut down the app
            await Metro.App.Current.ShutdownRCFAppAsync(true);
        }

        #endregion
    }
}