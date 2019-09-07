using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.Versioning;
using System.Threading.Tasks;
using System.Windows.Input;
using RayCarrot.CarrotFramework.Abstractions;
using RayCarrot.Extensions;
using RayCarrot.UI;
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
            ShowInstalledUtilitiesCommand = new AsyncRelayCommand(ShowInstalledUtilitiesAsync);
            RefreshDataOutputCommand = new AsyncRelayCommand(RefreshDataOutputAsync);
            RefreshAllCommand = new AsyncRelayCommand(RefreshAllAsync);
            RefreshAllAsyncCommand = new AsyncRelayCommand(RefreshAllTaskAsync);

            // Show log viewer if a debugger is attached
            if (!FirstConstruction || !Debugger.IsAttached)
                return;

            ShowLog();
            FirstConstruction = false;
        }

        #endregion

        #region Private Static Properties

        /// <summary>
        /// Indicates if this is the first time the class has been constructed
        /// </summary>
        private static bool FirstConstruction = true;

        #endregion

        #region Public Properties

        /// <summary>
        /// The selected dialog type
        /// </summary>
        public DebugDialogTypes SelectedDialog { get; set; }

        /// <summary>
        /// The selected data output type
        /// </summary>
        public DebugDataOutputTypes SelectedDataOutputType { get; set; }

        /// <summary>
        /// The current data output
        /// </summary>
        public string DataOutput { get; set; }

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
                        AllowWinStore = true,
                        AllowEducationalDosBox = true
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
                    await RCFUI.MessageUI.DisplayMessageAsync("Debug message", "Debug header", MessageType.Information);
                    break;

                case DebugDialogTypes.Directory:
                    await RCFUI.BrowseUI.BrowseDirectoryAsync(new DirectoryBrowserViewModel()
                    {
                        Title = "Debug"
                    });
                    break;

                case DebugDialogTypes.Drive:
                    await RCFUI.BrowseUI.BrowseDriveAsync(new DriveBrowserViewModel()
                    {
                        Title = "Debug"
                    });
                    break;

                case DebugDialogTypes.File:
                    await RCFUI.BrowseUI.BrowseFileAsync(new FileBrowserViewModel()
                    {
                        Title = "Debug"
                    });
                    break;

                case DebugDialogTypes.SaveFile:
                    await RCFUI.BrowseUI.SaveFileAsync(new SaveFileViewModel()
                    {
                        Title = "Debug"
                    });
                    break;

                default:
                    await RCFUI.MessageUI.DisplayMessageAsync("Invalid selection");
                    break;
            }
        }

        /// <summary>
        /// Shows the log viewer
        /// </summary>
        public void ShowLog()
        {
            WindowHelpers.ShowWindow<LogViewer>();
        }

        /// <summary>
        /// Shows the installed utilities for each game to the user
        /// </summary>
        /// <returns>The task</returns>
        public async Task ShowInstalledUtilitiesAsync()
        {
            var lines = new List<string>();

            foreach (Games game in App.GetGames)
            {
                if (game.IsAdded())
                    lines.AddRange(from utility in await game.GetAppliedUtilitiesAsync() select $"{utility} ({game.GetDisplayName()})");
            }

            await RCFUI.MessageUI.DisplayMessageAsync(lines.JoinItems(Environment.NewLine), MessageType.Information);
        }

        /// <summary>
        /// Refreshes the data output
        /// </summary>
        /// <returns>The task</returns>
        public async Task RefreshDataOutputAsync()
        {
            try
            {
                // Clear current data
                DataOutput = String.Empty;

                switch (SelectedDataOutputType)
                {
                    case DebugDataOutputTypes.ReferencedAssemblies:
                        foreach (AssemblyName assemblyName in Assembly.GetExecutingAssembly().GetReferencedAssemblies())
                        {
                            Assembly assembly = null;

                            try
                            {
                                // Load the assembly
                                assembly = Assembly.Load(assemblyName);
                            }
                            catch (Exception ex)
                            {
                                ex.HandleUnexpected("Loading referenced assembly");
                            }

                            DataOutput += $"Name: {assemblyName.Name}{Environment.NewLine}";
                            DataOutput += $"FullName: {assemblyName.FullName}{Environment.NewLine}";
                            DataOutput += $"Version: {assemblyName.Version}{Environment.NewLine}";

                            if (assembly != null)
                            {
                                // Get the PEKind for the assembly
                                assembly.GetModules()[0].GetPEKind(out PortableExecutableKinds peKinds, out ImageFileMachine _);
                                PortableExecutableKinds referenceKind = peKinds;

                                DataOutput += $"Location: {assembly.Location}{Environment.NewLine}";
                                DataOutput += $"ProcessorArchitecture: {((referenceKind & PortableExecutableKinds.Required32Bit) > 0 ? "x86" : (referenceKind & PortableExecutableKinds.PE32Plus) > 0 ? "x64" : "Any CPU")}{Environment.NewLine}";
                                DataOutput += $"TargetFrameworkDisplayName: {assembly.GetCustomAttributes(true).OfType<TargetFrameworkAttribute>().FirstOrDefault()?.FrameworkDisplayName}{Environment.NewLine}";
                            }

                            DataOutput += Environment.NewLine;
                        }
                        break;

                    case DebugDataOutputTypes.AppUserData:
                        // Save app user data to update the file
                        await App.SaveUserDataAsync();

                        // Display the file contents
                        DataOutput = File.ReadAllText(CommonPaths.AppUserDataPath);

                        break;

                    case DebugDataOutputTypes.UpdateManifest:

                        // Download the manifest as a string and display it
                        using (WebClient wc = new WebClient())
                            DataOutput = await wc.DownloadStringTaskAsync(CommonUrls.UpdateManifestUrl);

                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            catch (Exception ex)
            {
                ex.HandleError("Updating debug data output");
            }
        }

        /// <summary>
        /// Refreshes everything in the app
        /// </summary>
        /// <returns>The task</returns>
        public async Task RefreshAllAsync()
        {
            await App.OnRefreshRequiredAsync(new RefreshRequiredEventArgs(null, true, true, true, true, true));
        }

        /// <summary>
        /// Refreshes everything in the app on a new thread
        /// </summary>
        /// <returns>The task</returns>
        public async Task RefreshAllTaskAsync()
        {
            await Task.Run(async () => await App.OnRefreshRequiredAsync(new RefreshRequiredEventArgs(null, true, true, true, true, true)));
        }

        #endregion

        #region Commands

        public ICommand ShowDialogCommand { get; }

        public ICommand ShowLogCommand { get; }

        public ICommand ShowInstalledUtilitiesCommand { get; }

        public ICommand RefreshDataOutputCommand { get; }

        public ICommand RefreshAllCommand { get; }

        public ICommand RefreshAllAsyncCommand { get; }

        #endregion
    }
}