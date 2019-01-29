using System.Diagnostics;
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
    public class DebugPageViewModel : BaseViewModel
    {
        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        public DebugPageViewModel()
        {
            ShowDialogCommand = new AsyncRelayCommand(ShowDialogAsync);
            ShowLogCommand = new RelayCommand(ShowLog);

            // Show log viewer if a debugger is attached
            if (Debugger.IsAttached)
                ShowLog();
        }

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

        #endregion

        #region Commands

        public ICommand ShowDialogCommand { get; }

        public ICommand ShowLogCommand { get; }

        #endregion
    }
}