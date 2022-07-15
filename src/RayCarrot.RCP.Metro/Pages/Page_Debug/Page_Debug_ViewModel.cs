﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.Versioning;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using ControlzEx.Theming;
using Newtonsoft.Json;
using NLog;
using RayCarrot.RCP.Metro.Patcher;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// View model for the debug page
/// </summary>
public class Page_Debug_ViewModel : BasePageViewModel
{
    #region Constructor

    /// <summary>
    /// Default constructor
    /// </summary>
    public Page_Debug_ViewModel(
        AppViewModel app, 
        AppUserData data, 
        AppUIManager ui, 
        IFileManager fileManager, 
        IBrowseUIManager browseUi, 
        IMessageUIManager messageUi, 
        IDialogBaseManager dialogBaseManager) : base(app)
    {
        // Set services
        Data = data ?? throw new ArgumentNullException(nameof(data));
        UI = ui ?? throw new ArgumentNullException(nameof(ui));
        FileManager = fileManager ?? throw new ArgumentNullException(nameof(fileManager));
        BrowseUI = browseUi ?? throw new ArgumentNullException(nameof(browseUi));
        MessageUI = messageUi ?? throw new ArgumentNullException(nameof(messageUi));
        DialogBaseManager = dialogBaseManager ?? throw new ArgumentNullException(nameof(dialogBaseManager));

        // Create commands
        ShowDialogCommand = new AsyncRelayCommand(ShowDialogAsync);
        ShowLogCommand = new AsyncRelayCommand(ShowLogAsync);
        ShowWelcomeWindowCommand = new RelayCommand(ShowWelcomeWindow);
        ShowInstalledUtilitiesCommand = new AsyncRelayCommand(ShowInstalledUtilitiesAsync);
        RefreshDataOutputCommand = new AsyncRelayCommand(RefreshDataOutputAsync);
        RefreshAllCommand = new AsyncRelayCommand(RefreshAllAsync);
        RefreshAllAsyncCommand = new AsyncRelayCommand(RefreshAllTaskAsync);
        RefreshAllParallelAsyncCommand = new AsyncRelayCommand(RefreshAllParallelTaskAsync);
        GCCollectCommand = new RelayCommand(GCCollect);
        ThrowUnhandledExceptionCommand = new RelayCommand(ThrowUnhandledException);
        ThrowUnhandledExceptionAsyncCommand = new AsyncRelayCommand(ThrowUnhandledAsyncException);
        RunInstallerCommand = new AsyncRelayCommand(RunInstallerAsync);
        ShutdownAppCommand = new AsyncRelayCommand(async () => await Task.Run(async () => await Metro.App.Current.ShutdownAppAsync(false)));
        UpdateThemeCommand = new RelayCommand(UpdateTheme);
        ExportWebPatchesFilesCommand = new AsyncRelayCommand(ExportWebPatchesFilesAsync);
    }

    #endregion

    #region Logger

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    #endregion

    #region Commands

    public ICommand ShowDialogCommand { get; }
    public ICommand ShowLogCommand { get; }
    public ICommand ShowWelcomeWindowCommand { get; }
    public ICommand ShowInstalledUtilitiesCommand { get; }
    public ICommand RefreshDataOutputCommand { get; }
    public ICommand RefreshAllCommand { get; }
    public ICommand RefreshAllAsyncCommand { get; }
    public ICommand RefreshAllParallelAsyncCommand { get; }
    public ICommand GCCollectCommand { get; }
    public ICommand ThrowUnhandledExceptionCommand { get; }
    public ICommand ThrowUnhandledExceptionAsyncCommand { get; }
    public ICommand RunInstallerCommand { get; }
    public ICommand ShutdownAppCommand { get; }
    public ICommand UpdateThemeCommand { get; }
    public ICommand ExportWebPatchesFilesCommand { get; }

    #endregion

    #region Private Fields

    private bool _showGridLines;

    #endregion

    #region Services

    public AppUserData Data { get; }
    public AppUIManager UI { get; }
    public IFileManager FileManager { get; }
    public IBrowseUIManager BrowseUI { get; }
    public IMessageUIManager MessageUI { get; }
    public IDialogBaseManager DialogBaseManager { get; }

    #endregion

    #region Public Properties

    public override AppPage Page => AppPage.Debug;

    /// <summary>
    /// The selected dialog type
    /// </summary>
    public DebugDialogType SelectedDialog { get; set; }

    public bool ShowDialogAsAsync { get; set; }

    /// <summary>
    /// The selected data output type
    /// </summary>
    public DebugDataOutputType SelectedDataOutputType { get; set; }

    /// <summary>
    /// The current data output
    /// </summary>
    public string? DataOutput { get; set; }

    /// <summary>
    /// The available game installers
    /// </summary>
    public ObservableCollection<Games>? AvailableInstallers { get; set; }

    /// <summary>
    /// The selected game installer
    /// </summary>
    public Games SelectedInstaller { get; set; }

    public Color SelectedAccentColor { get; set; }

    public bool ShowGridLines
    {
        get => _showGridLines;
        set
        {
            _showGridLines = value;

            Application app = Application.Current;
            
            if (_showGridLines)
                app.Resources.Add(typeof(Grid), new Style(typeof(Grid))
                {
                    Setters =
                    {
                        new Setter(Grid.ShowGridLinesProperty, true)
                    }
                });
            else
                app.Resources.Remove(typeof(Grid));
        }
    }

    #endregion

    #region Methods

    protected override Task InitializeAsync()
    {
        // Set properties
        AvailableInstallers = App.GetGames.Where(x => x.GetGameInfo().CanBeInstalledFromDisc).ToObservableCollection();
        SelectedInstaller = AvailableInstallers.First();
        SelectedAccentColor = ThemeManager.Current.DetectTheme(Metro.App.Current)?.PrimaryAccentColor ?? new Color();

        return Task.CompletedTask;
    }

    /// <summary>
    /// Shows the selected dialog
    /// </summary>
    /// <returns></returns>
    public async Task ShowDialogAsync()
    {
        if (ShowDialogAsAsync)
            await Task.Run(showAsync);
        else
            await showAsync();

        async Task showAsync()
        {
            switch (SelectedDialog)
            {
                case DebugDialogType.GameSelection:
                    await UI.SelectGamesAsync(new GamesSelectionViewModel()
                    {
                        Title = "Debug",
                    });
                    break;

                case DebugDialogType.GameTypeSelection:
                    await UI.SelectGameTypeAsync(new GameTypeSelectionViewModel()
                    {
                        Title = "Debug",
                        AllowSteam = true,
                        AllowWin32 = true,
                        AllowDosBox = true,
                        AllowWinStore = true,
                        AllowEducationalDosBox = true,
                    });
                    break;

                case DebugDialogType.Message:
                    await MessageUI.DisplayMessageAsync("Debug message", "Debug header", MessageType.Information);
                    break;

                case DebugDialogType.Directory:
                    await BrowseUI.BrowseDirectoryAsync(new DirectoryBrowserViewModel()
                    {
                        Title = "Debug"
                    });
                    break;

                case DebugDialogType.Drive:
                    await UI.BrowseDriveAsync(new DriveBrowserViewModel()
                    {
                        Title = "Debug"
                    });
                    break;

                case DebugDialogType.File:
                    await BrowseUI.BrowseFileAsync(new FileBrowserViewModel()
                    {
                        Title = "Debug"
                    });
                    break;

                case DebugDialogType.SaveFile:
                    await BrowseUI.SaveFileAsync(new SaveFileViewModel()
                    {
                        Title = "Debug"
                    });
                    break;

                case DebugDialogType.EditEducationalDosGame:
                    await UI.EditEducationalDosGameAsync(new EducationalDosGameEditViewModel(new UserData_EducationalDosBoxGameData(FileSystemPath.EmptyPath, "DEBUG", "DEBUG"), new string[]
                    {
                        "DEBUG"
                    })
                    {
                        Title = "Debug"
                    });
                    break;

                case DebugDialogType.EditJumpList:
                    await UI.EditJumpListAsync(new JumpListEditViewModel()
                    {
                        Title = "Debug"
                    });
                    break;

                case DebugDialogType.FileExtensionSelection:
                    await UI.SelectFileExtensionAsync(new FileExtensionSelectionDialogViewModel(new string[]
                    {
                        "EXT 1",
                        "EXT 2",
                    }, "Select a file extension")
                    {
                        Title = "Debug"
                    });
                    break;

                case DebugDialogType.StringInput:
                    await UI.GetStringInputAsync(new StringInputViewModel()
                    {
                        Title = "Debug",
                        HeaderText = "Specify a string"
                    });
                    break;

                case DebugDialogType.ProgramSelection:
                    await UI.GetProgramAsync(new ProgramSelectionViewModel()
                    {
                        Title = "Debug",
                    });
                    break;

                default:
                    await MessageUI.DisplayMessageAsync("Invalid selection");
                    break;
            }
        }
    }

    /// <summary>
    /// Shows the log viewer
    /// </summary>
    /// <returns>The task</returns>
    public async Task ShowLogAsync()
    {
        if (!Metro.App.Current.IsLogViewerAvailable)
        {
            await MessageUI.DisplayMessageAsync("The log viewer is not enabled. Use the launch argument -logviewer to enabled it. The log file will open instead.", MessageType.Warning);

            await FileManager.LaunchFileAsync(AppFilePaths.LogFile);

            return;
        }

        LogViewer.Open();
    }

    /// <summary>
    /// Shows the welcome window
    /// </summary>
    public void ShowWelcomeWindow()
    {
        new FirstLaunchInfoDialog().ShowDialog();
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

        await MessageUI.DisplayMessageAsync(lines.JoinItems(Environment.NewLine), MessageType.Information);
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
                case DebugDataOutputType.ReferencedAssemblies:
                    foreach (AssemblyName assemblyName in Assembly.GetEntryAssembly()?.GetReferencedAssemblies() ?? Array.Empty<AssemblyName>())
                    {
                        Assembly? assembly = null;

                        try
                        {
                            // Load the assembly
                            assembly = Assembly.Load(assemblyName);
                        }
                        catch (Exception ex)
                        {
                            Logger.Warn(ex, "Loading referenced assembly");
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

                case DebugDataOutputType.AppUserData:
                    // Save app user data to update the file
                    await App.SaveUserDataAsync();

                    // Display the file contents
                    DataOutput = File.ReadAllText(AppFilePaths.AppUserDataPath);

                    break;

                case DebugDataOutputType.UpdateManifest:

                    // Download the manifest as a string and display it
                    using (WebClient wc = new WebClient())
                        DataOutput = await wc.DownloadStringTaskAsync(AppURLs.UpdateManifestUrl);

                    break;

                case DebugDataOutputType.AppWindows:
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

                case DebugDataOutputType.GameFinder:
                    // Select the games to find
                    var selectionResult = await UI.SelectGamesAsync(new GamesSelectionViewModel());

                    if (selectionResult.CanceledByUser)
                        return;

                    // Run and get the result
                    var result = await new GameFinder(selectionResult.SelectedGames, null).FindGamesAsync();
                        
                    // Output the found games
                    DataOutput = result.OfType<GameFinder_GameResult>().Select(x => $"{x.Game} ({x.GameType}) - {x.InstallLocation}").JoinItems(Environment.NewLine);
                        
                    break;

                case DebugDataOutputType.GameInfo:

                    // Helper method for adding a new line of text
                    void AddLine(string header, object content) => DataOutput += $"{header}: {content}{Environment.NewLine}";

                    foreach (Games game in App.GetGames)
                    {
                        var info = game.GetGameInfo();

                        AddLine("Display name", info.DisplayName);
                        AddLine("Default file name", info.DefaultFileName);
                        AddLine("Icon source", info.IconSource);
                        AddLine("Has disc installer", info.CanBeInstalledFromDisc);
                        AddLine("Dialog group names", info.DialogGroupNames.JoinItems(", "));
                            
                        if (info.IsAdded)
                            AddLine("Game file links", JsonConvert.SerializeObject(info.GetGameFileLinks));

                        DataOutput += Environment.NewLine;
                        DataOutput += "------------------------------------------";
                        DataOutput += Environment.NewLine;
                        DataOutput += Environment.NewLine;
                    }

                    break;

                case DebugDataOutputType.GameSizes:

                    // IDEA: Update with new system when done

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
                            Logger.Error(ex, "Getting game install dir size");
                            DataOutput += $"{game.GetGameInfo().DisplayName}: N/A{Environment.NewLine}";
                        }
                    }

                    DataOutput += $"{Environment.NewLine}{totalTime} ms";

                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Updating debug data output");
        }
    }

    /// <summary>
    /// Refreshes everything in the app
    /// </summary>
    /// <returns>The task</returns>
    public async Task RefreshAllAsync()
    {
        await App.OnRefreshRequiredAsync(new RefreshRequiredEventArgs(RefreshFlags.All));
    }

    /// <summary>
    /// Refreshes everything in the app on a new thread
    /// </summary>
    /// <returns>The task</returns>
    public async Task RefreshAllTaskAsync()
    {
        await Task.Run(async () => await App.OnRefreshRequiredAsync(new RefreshRequiredEventArgs(RefreshFlags.All)));
    }

    public async Task RefreshAllParallelTaskAsync()
    {
        await Task.WhenAll(Enumerable.Range(0, 50).Select(async _ => await App.OnRefreshRequiredAsync(new RefreshRequiredEventArgs(RefreshFlags.All))));
        await Task.WhenAll(Enumerable.Range(0, 20).Select(async _ => await App.OnRefreshRequiredAsync(new RefreshRequiredEventArgs(RefreshFlags.All))));
        await Task.WhenAll(Enumerable.Range(0, 5).Select(async _ => await App.OnRefreshRequiredAsync(new RefreshRequiredEventArgs(RefreshFlags.All))));
    }

    public void GCCollect() => GC.Collect();

    public void ThrowUnhandledException()
    {
        throw new Exception("Debug");
    }

    public async Task ThrowUnhandledAsyncException()
    {
        await Task.Run(() => throw new Exception("Debug"));
    }

    /// <summary>
    /// Runs the selected installer
    /// </summary>
    public async Task RunInstallerAsync()
    {
        await DialogBaseManager.ShowDialogWindowAsync(new GameInstaller_Window(SelectedInstaller));
    }

    public void UpdateTheme()
    {
        Metro.App.Current.SetTheme(Data.Theme_DarkMode, false, SelectedAccentColor);
    }

    public async Task ExportWebPatchesFilesAsync()
    {
        try
        {
            FileBrowserResult inputResult = await BrowseUI.BrowseFileAsync(new FileBrowserViewModel()
            {
                Title = "Select patches",
                MultiSelection = true,
                ExtensionFilter = new FileFilterItem($"*{PatchFile.FileExtension}", "Game Patch").StringRepresentation,
            });

            if (inputResult.CanceledByUser)
                return;

            DirectoryBrowserResult outputResult = await BrowseUI.BrowseDirectoryAsync(new DirectoryBrowserViewModel()
            {
                Title = "Select output folder",
            });

            if (outputResult.CanceledByUser)
                return;

            List<(Games Game, ExternalPatchManifest Manifest)> patches = new();
            Dictionary<Games, string> gameManifestURLs = new();

            // Process each patch
            foreach (FileSystemPath patchFilePath in inputResult.SelectedFiles)
            {
                PatchManifest manifest;
                string? thumbURL = null;

                // Open the patch
                using (PatchFile patch = new(patchFilePath, readOnly: true))
                {
                    // Read the manifest
                    manifest = patch.ReadManifest();

                    // Extract thumbnail, if one exists
                    if (manifest.HasAsset(PatchAsset.Thumbnail))
                    {
                        thumbURL = $"{manifest.Game.ToString().ToLowerInvariant()}/{patchFilePath.ChangeFileExtension(new FileExtension(".png")).Name}";

                        using Stream thumbOutputStream = File.Create(outputResult.SelectedDirectory + "patches" + thumbURL);
                        using Stream thumbStream = patch.GetPatchAsset(PatchAsset.Thumbnail);

                        thumbStream.CopyTo(thumbOutputStream);
                    }
                }

                string patchURL = $"{manifest.Game.ToString().ToLowerInvariant()}/{patchFilePath.Name}";

                // Copy the patch file
                FileManager.CopyFile(patchFilePath, outputResult.SelectedDirectory + "patches" + patchURL, true);

                patches.Add((manifest.Game, new ExternalPatchManifest(
                    ID: manifest.ID,
                    Name: manifest.Name,
                    Description: manifest.Description,
                    Author: manifest.Author,
                    TotalSize: manifest.TotalSize,
                    ModifiedDate: manifest.ModifiedDate,
                    Revision: manifest.Revision,
                    AddedFilesCount: manifest.AddedFiles?.Length ?? 0,
                    RemovedFilesCount: manifest.RemovedFiles?.Length ?? 0,
                    Patch: patchURL,
                    PatchSize: (int)patchFilePath.GetSize().Bytes,
                    Thumbnail: thumbURL)));
            }

            // Write game manifests detailing the patches for each game
            foreach (var gamePatches in patches.GroupBy(x => x.Game))
            {
                ExternalGamePatchesManifest manifest = new(gamePatches.Key, gamePatches.Select(x => x.Manifest).ToArray());

                string url = $"patches/{gamePatches.Key}.json";
                gameManifestURLs[gamePatches.Key] = url;

                JsonHelpers.SerializeToFile(manifest, outputResult.SelectedDirectory + url);
            }

            // Write the main patches manifest
            ExternalPatchesManifest patchesManifest = new(ExternalPatchesManifest.LatestVersion, gameManifestURLs);
            JsonHelpers.SerializeToFile(patchesManifest, outputResult.SelectedDirectory + "patches.json");
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Generating web patches files");

            await MessageUI.DisplayExceptionMessageAsync(ex, "An error occurred when generating the files");
        }
    }

    #endregion

    #region Enums

    /// <summary>
    /// The available debug dialog types
    /// </summary>
    public enum DebugDialogType
    {
        GameSelection,

        /// <summary>
        /// A game type selection dialog
        /// </summary>
        GameTypeSelection,

        /// <summary>
        /// A message dialog
        /// </summary>
        Message,

        /// <summary>
        /// A directory selection dialog
        /// </summary>
        Directory,

        /// <summary>
        /// A drive selection dialog
        /// </summary>
        Drive,

        /// <summary>
        /// A file selection dialog
        /// </summary>
        File,

        /// <summary>
        /// A save file dialog
        /// </summary>
        SaveFile,

        EditEducationalDosGame,

        EditJumpList,

        FileExtensionSelection,

        StringInput,

        ProgramSelection,
    }

    /// <summary>
    /// The available debug data output types
    /// </summary>
    public enum DebugDataOutputType
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
    }

    #endregion
}