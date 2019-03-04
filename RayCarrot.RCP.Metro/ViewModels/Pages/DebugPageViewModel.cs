using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;
using RayCarrot.CarrotFramework;
using RayCarrot.Windows.Registry;
using RayCarrot.WPF;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// View model for the debug page
    /// </summary>
    public class DebugPageViewModel : BaseRCPViewModel
    {
        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        public DebugPageViewModel()
        {
            ShowDialogCommand = new AsyncRelayCommand(ShowDialogAsync);
            ShowLogCommand = new RelayCommand(ShowLog);
            OpenAppDataCommand = new AsyncRelayCommand(OpenAppDataAsync);
            UbiIniLinkCommand = new AsyncRelayCommand(SetupUbiIniLinkAsync);
            OpenPrimaryUbiIniCommand = new AsyncRelayCommand(OpenPrimaryUbiIniAsync);
            OpenSecondaryUbiIniCommand = new AsyncRelayCommand(OpenSecondaryUbiIniAsync);

            // Show log viewer if a debugger is attached
            if (_firstConstruction && Debugger.IsAttached)
            {
                ShowLog();
                _firstConstruction = false;
            }
        }

        #endregion

        #region Private Static Properties

        /// <summary>
        /// Indicates if this is the first time the class has been constructed
        /// </summary>
        private static bool _firstConstruction = true;

        #endregion

        #region Public Properties

        /// <summary>
        /// The selected dialog type
        /// </summary>
        public DebugDialogTypes SelectedDialog { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// Shows the selected dialog
        /// </summary>
        /// <returns></returns>
        public async Task ShowDialogAsync()
        {
            switch (SelectedDialog)
            {
                case DebugDialogTypes.GameTypeSelection:
                    await new GameTypeSelectionDialog(new GameTypeSelectionViewModel()
                    {
                        Title = "Debug",
                        AllowSteam = true,
                        AllowWin32 = true,
                        AllowDosBox = true,
                        AllowWinStore = true
                    }).ShowDialogAsync();
                    break;

                case DebugDialogTypes.RegistryKey:
                    await RCFWinReg.RegistryBrowseUIManager.BrowseRegistryKeyAsync(new RegistryBrowserViewModel()
                    {
                        Title = "Debug",
                        BrowseValue = false
                    });
                    break;

                case DebugDialogTypes.RegistryKeyValue:
                    await RCFWinReg.RegistryBrowseUIManager.BrowseRegistryKeyAsync(new RegistryBrowserViewModel()
                    {
                        Title = "Debug",
                        BrowseValue = true
                    });
                    break;

                case DebugDialogTypes.Message:
                    await RCF.MessageUI.DisplayMessageAsync("Debug message", "Debug header", MessageType.Information);
                    break;

                case DebugDialogTypes.Directory:
                    await RCF.BrowseUI.BrowseDirectoryAsync(new DirectoryBrowserViewModel()
                    {
                        Title = "Debug"
                    });
                    break;

                case DebugDialogTypes.Drive:
                    await RCF.BrowseUI.BrowseDriveAsync(new DriveBrowserViewModel()
                    {
                        Title = "Debug"
                    });
                    break;

                case DebugDialogTypes.File:
                    await RCF.BrowseUI.BrowseFileAsync(new FileBrowserViewModel()
                    {
                        Title = "Debug"
                    });
                    break;

                case DebugDialogTypes.SaveFile:
                    await RCF.BrowseUI.SaveFileAsync(new SaveFileViewModel()
                    {
                        Title = "Debug"
                    });
                    break;

                default:
                    await RCF.MessageUI.DisplayMessageAsync("Invalid selection");
                    break;
            }
        }

        /// <summary>
        /// Shows the log viewer
        /// </summary>
        public void ShowLog()
        {
            new LogViewer().Show();
        }

        /// <summary>
        /// Opens the app data directory
        /// </summary>
        /// <returns>The task</returns>
        public async Task OpenAppDataAsync()
        {
            await RCFRCP.File.OpenExplorerLocationAsync(CommonPaths.UserDataBaseDir);
        }

        /// <summary>
        /// Sets up the ubi.ini file symbolic link
        /// </summary>
        /// <returns>The task</returns>
        public async Task SetupUbiIniLinkAsync()
        {
            await RCF.MessageUI.DisplayMessageAsync("This feature has not been implemented");
        }

        /// <summary>
        /// Opens the primary ubi.ini file
        /// </summary>
        /// <returns>The task</returns>
        public async Task OpenPrimaryUbiIniAsync()
        {
            (await RCFRCP.File.LaunchFileAsync(CommonPaths.UbiIniPath1))?.Dispose();
        }

        /// <summary>
        /// Opens the secondary ubi.ini file
        /// </summary>
        /// <returns>The task</returns>
        public async Task OpenSecondaryUbiIniAsync()
        {
            (await RCFRCP.File.LaunchFileAsync(CommonPaths.UbiIniPath2))?.Dispose();
        }

        #endregion

        #region Commands

        public ICommand ShowDialogCommand { get; }

        public ICommand ShowLogCommand { get; }

        public ICommand OpenAppDataCommand { get; }

        public ICommand OpenPrimaryUbiIniCommand { get; }

        public ICommand OpenSecondaryUbiIniCommand { get; }

        public ICommand UbiIniLinkCommand { get; }

        #endregion
    }
}