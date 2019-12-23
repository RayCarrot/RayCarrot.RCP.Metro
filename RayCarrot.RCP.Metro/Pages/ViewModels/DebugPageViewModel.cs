using RayCarrot.CarrotFramework.Abstractions;
using RayCarrot.Extensions;
using RayCarrot.IO;
using RayCarrot.UI;
using RayCarrot.Windows.Registry;
using RayCarrot.WPF;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.Versioning;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using RayCarrot.Rayman;
using RayCarrot.RCP.Core;

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
            // Create commands
            ShowDialogCommand = new AsyncRelayCommand(ShowDialogAsync);
            ShowLogCommand = new AsyncRelayCommand(ShowLogAsync);
            ShowInstalledUtilitiesCommand = new AsyncRelayCommand(ShowInstalledUtilitiesAsync);
            RefreshDataOutputCommand = new AsyncRelayCommand(RefreshDataOutputAsync);
            RefreshAllCommand = new AsyncRelayCommand(RefreshAllAsync);
            RefreshAllAsyncCommand = new AsyncRelayCommand(RefreshAllTaskAsync);
            RunInstallerCommand = new RelayCommand(RunInstaller);
            ShutdownAppCommand = new AsyncRelayCommand(async () => await Task.Run(async () => await Metro.App.Current.ShutdownRCFAppAsync(false)));
            OpenArchiveExplorerCommand = new AsyncRelayCommand(OpenArchiveExplorerAsync);

            // Get properties
            AvailableInstallers = App.GetGames.Where(x => x.GetGameInfo().CanBeInstalledFromDisc).ToArray();
            SelectedInstaller = AvailableInstallers.First();
        }

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

        /// <summary>
        /// The available game installers
        /// </summary>
        public Games[] AvailableInstallers { get; }

        /// <summary>
        /// The selected game installer
        /// </summary>
        public Games SelectedInstaller { get; set; }

        /// <summary>
        /// The selected archive explorer
        /// </summary>
        public DebugArchiveExplorer SelectedArchiveExplorer { get; set; }

        /// <summary>
        /// The selected OpenSpace game mode
        /// </summary>
        public OpenSpaceGameMode SelectedOpenSpaceGameMode { get; set; }

        /// <summary>
        /// The available OpenSpace game modes
        /// </summary>
        public string[] OpenSpaceGameModes => OpenSpaceGameMode.DinosaurPC.GetValues().Select(x => x.GetDisplayName()).ToArray();

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
                        AllowEducationalDosBox = true,
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
        /// <returns>The task</returns>
        public async Task ShowLogAsync()
        {
            await new LogViewer().ShowWindowAsync();
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
                // Get the game info
                var info = game.GetGameInfo();

                if (info.IsAdded)
                    lines.AddRange(from utility in await info.GetAppliedUtilitiesAsync() select $"{utility} ({info.DisplayName})");
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

                    case DebugDataOutputTypes.AppWindows:
                        var mainWindow = Application.Current.MainWindow;

                        foreach (Window window in Application.Current.Windows)
                        {
                            DataOutput += Environment.NewLine;
                            DataOutput += $"Type: {window.GetType().FullName}" + Environment.NewLine;
                            DataOutput += $"Title: {window.Title}" + Environment.NewLine;
                            DataOutput += $"Owner: {window.Owner}" + Environment.NewLine;
                            DataOutput += $"Owned windows: {window.OwnedWindows.Count}" + Environment.NewLine;
                            DataOutput += $"Show in taskbar: {window.ShowInTaskbar}" + Environment.NewLine;
                            DataOutput += $"Show activated: {window.ShowActivated}" + Environment.NewLine;
                            DataOutput += $"Visibility: {window.Visibility}" + Environment.NewLine;
                            DataOutput += $"Is main window: {window == mainWindow}" + Environment.NewLine;
                            DataOutput += Environment.NewLine;
                            DataOutput += "------------------------------------------";
                            DataOutput += Environment.NewLine;
                        }
                        
                        break;

                    case DebugDataOutputTypes.GameFinder:
                        // Select the games to find
                        var selectionResult = await RCFRCP.UI.SelectGamesAsync(new GamesSelectionViewModel());

                        if (selectionResult.CanceledByUser)
                            return;

                        // Run and get the result
                        var result = await new GameFinder(selectionResult.SelectedGames, null).FindGamesAsync();
                        
                        // Output the found games
                        DataOutput = result.OfType<GameFinderResult>().Select(x => $"{x.Game} ({x.GameType}) - {x.InstallLocation}").JoinItems(Environment.NewLine);
                        
                        break;

                    case DebugDataOutputTypes.GameInfo:

                        // Helper method for adding a new line of text
                        void AddLine(string header, object content) => DataOutput += $"{header}: {content}{Environment.NewLine}";

                        IBaseSerializer serializer = new JsonBaseSerializer();

                        foreach (Games game in App.GetGames)
                        {
                            var info = game.GetGameInfo();

                            AddLine("Display name", info.DisplayName);
                            AddLine("Default file name", info.DefaultFileName);
                            AddLine("Icon source", info.IconSource);
                            AddLine("Has disc installer", info.CanBeInstalledFromDisc);
                            AddLine("Dialog group names", info.DialogGroupNames.JoinItems(", "));
                            
                            if (info.IsAdded)
                            {
                                AddLine("Backup directories", await serializer.SerializeAsync(info.GetBackupInfos));
                                AddLine("Game file links", await serializer.SerializeAsync(info.GetGameFileLinks));
                            }

                            DataOutput += Environment.NewLine;
                            DataOutput += "------------------------------------------";
                            DataOutput += Environment.NewLine;
                            DataOutput += Environment.NewLine;
                        }

                        break;

                    case DebugDataOutputTypes.GameSizes:

                        // TODO: Update with new system when done

                        var totalTime = 0L;

                        // Enumerate each game
                        foreach (var game in App.GetGames.Where(x => x.IsAdded()))
                        {
                            try
                            {
                                var gameTimer = new Stopwatch();

                                gameTimer.Start();

                                // Get the size
                                var size = game.GetInstallDir().GetSize();

                                gameTimer.Stop();

                                totalTime += gameTimer.ElapsedMilliseconds;

                                // Output the size
                                DataOutput += $"{game.GetGameInfo().DisplayName}: {size.ToString()} ({gameTimer.ElapsedMilliseconds} ms){Environment.NewLine}";
                            }
                            catch (Exception ex)
                            {
                                ex.HandleError("Getting game install dir size");
                                DataOutput += $"{game.GetGameInfo().DisplayName}: N/A{Environment.NewLine}";
                            }
                        }

                        DataOutput += $"{Environment.NewLine}{totalTime} ms";

                        break;

                    case DebugDataOutputTypes.APIVersions:

                        // Get the API libraries
                        var libraries = new string[]
                        {
                            "RayCarrot.RCP.Core",
                            "RayCarrot.RCP.Core.UI",
                            "RayCarrot.CarrotFramework",
                            "RayCarrot.CarrotFramework.Abstractions",
                            "RayCarrot.UI",
                            "RayCarrot.IO",
                            "RayCarrot.Extensions",
                            "RayCarrot.UserData",
                            "RayCarrot.Windows.Shell",
                            "RayCarrot.Windows.Registry",
                        };

                        // Enumerate each library
                        foreach (string lib in libraries)
                        {
                            // Load the assembly
                            var a = Assembly.Load(lib);

                            // Output
                            DataOutput += $"{lib} ({a.GetCustomAttribute<AssemblyFileVersionAttribute>()?.Version ?? "N/A"})";

                            DataOutput += Environment.NewLine;
                        }

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

        /// <summary>
        /// Runs the selected installer
        /// </summary>
        public void RunInstaller()
        {
            new GameInstaller(SelectedInstaller).ShowDialog();
        }

        /// <summary>
        /// Opens the selected archive explorer
        /// </summary>
        /// <returns>The task</returns>
        public async Task OpenArchiveExplorerAsync()
        {
            // Helper method for getting the game for the selected archive explorer mode
            Games? GetGame()
            {
                return SelectedArchiveExplorer switch
                {
                    DebugArchiveExplorer.OpenSpace_CNT => (SelectedOpenSpaceGameMode switch
                    {
                        OpenSpaceGameMode.Rayman2PC => Games.Rayman2,
                        OpenSpaceGameMode.RaymanMPC => Games.RaymanM,
                        OpenSpaceGameMode.RaymanArenaPC => Games.RaymanArena,
                        OpenSpaceGameMode.Rayman3PC => Games.Rayman3,
                        _ => (Games?) null
                    }),
                    _ => throw new ArgumentOutOfRangeException()
                };
            }

            // Helper method for getting the extension filter for the selected archive explorer mode
            FileFilterItemCollection GetExtensionFilter() => SelectedArchiveExplorer switch
            {
                DebugArchiveExplorer.OpenSpace_CNT => new FileFilterItemCollection()
                {
                    new FileFilterItem("*.cnt", "CNT files")
                },
                _ => throw new ArgumentOutOfRangeException()
            };

            // Allow the user to select the file
            var fileResult = await RCFUI.BrowseUI.BrowseFileAsync(new FileBrowserViewModel()
            {
                Title = "Select archive files",
                DefaultDirectory = GetGame()?.GetInstallDir(false).FullPath,
                ExtensionFilter = GetExtensionFilter().ToString(),
                MultiSelection = true
            });

            if (fileResult.CanceledByUser)
                return;

            await new ArchiveExplorer(new ArchiveExplorerDialogViewModel(new OpenSpaceCntArchiveDataManager(SelectedOpenSpaceGameMode.GetSettings()), fileResult.SelectedFiles)).ShowWindowAsync();
        }

        #endregion

        #region Commands

        public ICommand ShowDialogCommand { get; }

        public ICommand ShowLogCommand { get; }

        public ICommand ShowInstalledUtilitiesCommand { get; }

        public ICommand RefreshDataOutputCommand { get; }

        public ICommand RefreshAllCommand { get; }

        public ICommand RefreshAllAsyncCommand { get; }

        public ICommand RunInstallerCommand { get; }

        public ICommand ShutdownAppCommand { get; }

        public ICommand OpenArchiveExplorerCommand { get; }

        #endregion

        #region Enums

        /// <summary>
        /// The available debug data output types
        /// </summary>
        public enum DebugDataOutputTypes
        {
            /// <summary>
            /// Displays a list of the referenced assemblies
            /// </summary>
            ReferencedAssemblies,

            /// <summary>
            /// Displays the app user data file contents
            /// </summary>
            AppUserData,

            /// <summary>
            /// Displays the update manifest from the server
            /// </summary>
            UpdateManifest,

            /// <summary>
            /// Displays a list of windows in the current application
            /// </summary>
            AppWindows,

            /// <summary>
            /// Runs the game finder, searching for all games and displaying the output of found games and their install locations
            /// </summary>
            GameFinder,

            /// <summary>
            /// Display the info available for each game
            /// </summary>
            GameInfo,

            /// <summary>
            /// Displays the install sizes for each game
            /// </summary>
            GameSizes,

            /// <summary>
            /// Displays the versions of the Carrot Framework and Rayman Control Panel API
            /// </summary>
            APIVersions
        }

        /// <summary>
        /// The available archive explorer modes
        /// </summary>
        public enum DebugArchiveExplorer
        {
            /// <summary>
            /// OpenSpace .cnt archives
            /// </summary>
            OpenSpace_CNT
        }

        #endregion
    }
}