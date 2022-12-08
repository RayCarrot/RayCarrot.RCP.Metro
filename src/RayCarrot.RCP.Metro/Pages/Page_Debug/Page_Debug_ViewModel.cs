using System;
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
using BinarySerializer;
using ByteSizeLib;
using ControlzEx.Theming;
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
        FileManager fileManager, 
        IBrowseUIManager browseUi, 
        IMessageUIManager messageUi, 
        LoggerManager loggerManager, 
        AppDataManager appDataManager, 
        GamesManager gamesManager) : base(app)
    {
        // Set services
        Data = data ?? throw new ArgumentNullException(nameof(data));
        UI = ui ?? throw new ArgumentNullException(nameof(ui));
        FileManager = fileManager ?? throw new ArgumentNullException(nameof(fileManager));
        BrowseUI = browseUi ?? throw new ArgumentNullException(nameof(browseUi));
        MessageUI = messageUi ?? throw new ArgumentNullException(nameof(messageUi));
        LoggerManager = loggerManager ?? throw new ArgumentNullException(nameof(loggerManager));
        AppDataManager = appDataManager ?? throw new ArgumentNullException(nameof(appDataManager));
        GamesManager = gamesManager ?? throw new ArgumentNullException(nameof(gamesManager));

        // Create commands
        ShowDialogCommand = new AsyncRelayCommand(ShowDialogAsync);
        ShowLogCommand = new AsyncRelayCommand(ShowLogAsync);
        ShowWelcomeWindowCommand = new RelayCommand(ShowWelcomeWindow);
        ShowInstalledUtilitiesCommand = new AsyncRelayCommand(ShowInstalledUtilitiesAsync);
        RefreshDataOutputCommand = new AsyncRelayCommand(RefreshDataOutputAsync);
        GCCollectCommand = new RelayCommand(GCCollect);
        ThrowUnhandledExceptionCommand = new RelayCommand(ThrowUnhandledException);
        ThrowUnhandledExceptionAsyncCommand = new AsyncRelayCommand(ThrowUnhandledAsyncException);
        RunInstallerCommand = new AsyncRelayCommand(RunInstallerAsync);
        ShutdownAppCommand = new AsyncRelayCommand(async () => await Task.Run(async () => await Metro.App.Current.ShutdownAppAsync(false)));
        UpdateThemeCommand = new RelayCommand(UpdateTheme);
        ExportWebPatchesJSONCommand = new AsyncRelayCommand(ExportWebPatchesJSONAsync);
        ExportWebPatchesFilesCommand = new AsyncRelayCommand(ExportWebPatchesFilesAsync);
        ExtractPatchLibraryCommand = new AsyncRelayCommand(ExtractPatchLibraryAsync);
        RunLoadOperationCommand = new AsyncRelayCommand(RunLoadOperationAsync);
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
    public ICommand GCCollectCommand { get; }
    public ICommand ThrowUnhandledExceptionCommand { get; }
    public ICommand ThrowUnhandledExceptionAsyncCommand { get; }
    public ICommand RunInstallerCommand { get; }
    public ICommand ShutdownAppCommand { get; }
    public ICommand UpdateThemeCommand { get; }
    public ICommand ExportWebPatchesJSONCommand { get; }
    public ICommand ExportWebPatchesFilesCommand { get; }
    public ICommand ExtractPatchLibraryCommand { get; }
    public ICommand RunLoadOperationCommand { get; }

    #endregion

    #region Private Fields

    private bool _showGridLines;

    #endregion

    #region Services

    private AppUserData Data { get; }
    private AppUIManager UI { get; }
    private FileManager FileManager { get; }
    private IBrowseUIManager BrowseUI { get; }
    private IMessageUIManager MessageUI { get; }
    private LoggerManager LoggerManager { get; }
    private AppDataManager AppDataManager { get; }
    private GamesManager GamesManager { get; }

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
    public ObservableCollection<GameDescriptor>? AvailableInstallers { get; set; }

    /// <summary>
    /// The selected game installer
    /// </summary>
    public GameDescriptor? SelectedInstaller { get; set; }

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
        AvailableInstallers = GamesManager.EnumerateGameDescriptors().Where(x => x.GetAddActions().OfType<DiscInstallGameAddAction>().Any()).ToObservableCollection();
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
        if (!LoggerManager.IsLogViewerAvailable)
        {
            await MessageUI.DisplayMessageAsync("The log viewer is not enabled. Use the launch argument -logviewer to enabled it. The log file will open instead.", MessageType.Warning);

            await FileManager.LaunchFileAsync(AppFilePaths.LogFile);

            return;
        }

        LogViewer.Open(LoggerManager.LogViewerViewModel);
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

        foreach (GameInstallation gameInstallation in GamesManager.EnumerateInstalledGames())
        {
            lines.AddRange((await gameInstallation.GameDescriptor.GetAppliedUtilitiesAsync(gameInstallation)).
                Select(utility => $"{utility} ({gameInstallation.GameDescriptor.DisplayName})"));
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
                    AppDataManager.Save();

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
                    // Run and get the result
                    var result = new GameFinder(GamesManager.EnumerateGameDescriptors(), null).FindGames();
                        
                    // Output the found games
                    DataOutput = result.OfType<GameFinder_GameResult>().Select(x => $"{x.GameDescriptor.GameId} - {x.InstallLocation}").JoinItems(Environment.NewLine);
                        
                    break;

                // TODO-14: Replace with data grid view in UI
                case DebugDataOutputType.GameDescriptor:

                    // Helper method for adding a new line of text
                    void AddLine(string header, object content) => DataOutput += $"{header}: {content}{Environment.NewLine}";

                    foreach (GameDescriptor gameDescriptor in GamesManager.EnumerateGameDescriptors())
                    {
                        AddLine("Display name", gameDescriptor.DisplayName);
                        AddLine("Default file name", gameDescriptor.DefaultFileName);
                        AddLine("Icon source", gameDescriptor.Icon.GetAssetPath());
                        AddLine("Dialog group names", gameDescriptor.DialogGroupNames.JoinItems(", "));
                        
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
                    foreach (GameInstallation installedGame in GamesManager.EnumerateInstalledGames())
                    {
                        try
                        {
                            Stopwatch gameTimer = Stopwatch.StartNew();

                            // Get the size
                            ByteSize size = installedGame.InstallLocation.GetSize();

                            gameTimer.Stop();

                            totalTime += gameTimer.ElapsedMilliseconds;

                            // Output the size
                            DataOutput += $"{installedGame.GameDescriptor.DisplayName}: {size.ToString()} ({gameTimer.ElapsedMilliseconds} ms){Environment.NewLine}";
                        }
                        catch (Exception ex)
                        {
                            Logger.Error(ex, "Getting game install dir size");
                            DataOutput += $"{installedGame.GameDescriptor.DisplayName}: N/A{Environment.NewLine}";
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
        if (SelectedInstaller != null)
            await UI.InstallGameAsync(SelectedInstaller, SelectedInstaller.GetAddActions().OfType<DiscInstallGameAddAction>().First().InstallerInfo);
    }

    public void UpdateTheme()
    {
        Metro.App.Current.SetTheme(Data.Theme_DarkMode, false, SelectedAccentColor);
    }

    public async Task ExportWebPatchesJSONAsync()
    {
        try
        {
            DirectoryBrowserResult browseResult = await BrowseUI.BrowseDirectoryAsync(new DirectoryBrowserViewModel()
            {
                Title = "Select patches directory"
            });

            if (browseResult.CanceledByUser)
                return;

            // Write out JSON manually to allow for nicer formatting
            using StreamWriter writer = new(browseResult.SelectedDirectory + "external_patches.jsonc", false);

            await writer.WriteLineAsync("[");

            var files = Directory.EnumerateFiles(browseResult.SelectedDirectory, $"*{PatchFile.FileExtension}", SearchOption.AllDirectories);
            foreach (FileSystemPath patchFilePath in files)
            {
                await writer.WriteLineAsync($"    {{ " +
                                       $"\"{nameof(WebPatchEntry.FilePath)}\": \"{patchFilePath - browseResult.SelectedDirectory}\", " +
                                       $"\"{nameof(WebPatchEntry.MinVersion)}\": \"{App.CurrentAppVersion.ToString(4)}\"" +
                                       $" }},");
            }

            await writer.WriteLineAsync("]");
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Generating web patches files");

            await MessageUI.DisplayExceptionMessageAsync(ex, "An error occurred when generating the files");
        }
    }

    public async Task ExportWebPatchesFilesAsync()
    {
        try
        {
            FileBrowserResult inputResult = await BrowseUI.BrowseFileAsync(new FileBrowserViewModel()
            {
                Title = "Select patches JSON",
                ExtensionFilter = new FileFilterItem($"*jsonc", "JSON").StringRepresentation,
            });

            if (inputResult.CanceledByUser)
                return;

            DirectoryBrowserResult outputResult = await BrowseUI.BrowseDirectoryAsync(new DirectoryBrowserViewModel()
            {
                Title = "Select output folder",
            });

            if (outputResult.CanceledByUser)
                return;

            WebPatchEntry[] patchEntries = JsonHelpers.DeserializeFromFile<WebPatchEntry[]>(inputResult.SelectedFile);

            List<(LegacyGame Game, ExternalPatchManifest Manifest)> patches = new();
            Dictionary<LegacyGame, string> gameManifestURLs = new();

            using RCPContext context = new(String.Empty);

            // Process each patch
            foreach (WebPatchEntry patchEntry in patchEntries)
            {
                FileSystemPath patchFilePath = inputResult.SelectedFile.Parent + patchEntry.FilePath;

                // Read the file
                PatchFile patch = context.ReadRequiredFileData<PatchFile>(patchFilePath, removeFileWhenComplete: false);

                // TODO-14: Update this
                LegacyGame game = patch.Metadata.GetGameDescriptors(GamesManager).First().LegacyGame.Value;
                string? thumbURL = null;

                // Extract thumbnail, if one exists
                if (patch.HasThumbnail)
                {
                    thumbURL = $"{game.ToString().ToLowerInvariant()}/{patchFilePath.ChangeFileExtension(new FileExtension(".png")).Name}";

                    FileSystemPath thumbFilePath = outputResult.SelectedDirectory + "patches" + thumbURL;

                    Directory.CreateDirectory(thumbFilePath.Parent);

                    using Stream thumbOutputStream = File.Create(thumbFilePath);
                    patch.ThumbnailResource.ReadData(context, true).CopyTo(thumbOutputStream);
                }

                string patchURL = $"{game.ToString().ToLowerInvariant()}/{patchFilePath.Name}";

                // Copy the patch file
                FileManager.CopyFile(patchFilePath, outputResult.SelectedDirectory + "patches" + patchURL, true);

                patches.Add((game, new ExternalPatchManifest(
                    ID: patch.Metadata.ID,
                    FormatVersion: patch.FormatVersion,
                    MinAppVersion: patchEntry.MinVersion,
                    Name: patch.Metadata.Name,
                    Description: patch.Metadata.Description,
                    Author: patch.Metadata.Author,
                    Website: patch.Metadata.Website,
                    TotalSize: patch.Metadata.TotalSize,
                    ModifiedDate: patch.Metadata.ModifiedDate,
                    Version: patch.Metadata.Version,
                    AddedFilesCount: patch.AddedFiles?.Length ?? 0,
                    RemovedFilesCount: patch.RemovedFiles?.Length ?? 0,
                    Patch: patchURL,
                    PatchSize: (int)patchFilePath.GetSize().Bytes,
                    Thumbnail: thumbURL)));
            }

            // Write game manifests detailing the patches for each game
            foreach (var gamePatches in patches.GroupBy(x => x.Game))
            {
                ExternalGamePatchesManifest manifest = new(gamePatches.Key, gamePatches.Select(x => x.Manifest).ToArray());

                string url = $"patches/{gamePatches.Key.ToString().ToLowerInvariant()}.json";
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

    public async Task ExtractPatchLibraryAsync()
    {
        FileBrowserResult inputResult = await BrowseUI.BrowseFileAsync(new FileBrowserViewModel()
        {
            Title = "Select patch library",
            ExtensionFilter = new FileFilterItem($"*{PatchLibraryFile.FileExtension}", "Game Patch Library").StringRepresentation,
        });

        if (inputResult.CanceledByUser)
            return;

        DirectoryBrowserResult outputResult = await BrowseUI.BrowseDirectoryAsync(new DirectoryBrowserViewModel()
        {
            Title = "Select output folder",
        });

        if (outputResult.CanceledByUser)
            return;

        try
        {
            using RCPContext context = new(String.Empty);

            // Read the file
            PatchLibraryFile lib = context.ReadRequiredFileData<PatchLibraryFile>(inputResult.SelectedFile, removeFileWhenComplete: false);

            // Extract removed files
            for (int i = 0; i < lib.History.RemovedFileResources.Length; i++)
                await extractResourcesAsync(
                    context: context, 
                    filePath: lib.History.RemovedFiles[i], 
                    resource: lib.History.RemovedFileResources[i], 
                    outputDir: outputResult.SelectedDirectory + "removed_files");

            // Extract replaced files
            for (int i = 0; i < lib.History.ReplacedFileResources.Length; i++)
                await extractResourcesAsync(
                    context: context, 
                    filePath: lib.History.ReplacedFiles[i], 
                    resource: lib.History.ReplacedFileResources[i], 
                    outputDir: outputResult.SelectedDirectory + "replaced_files");

            // Extract added files
            File.WriteAllLines(outputResult.SelectedDirectory + "added_files.txt", lib.History.AddedFiles.Select(x => x.ToString()));

            // Extract the patches list
            JsonHelpers.SerializeToFile(lib.Patches, outputResult.SelectedDirectory + "patches.json");

            static async Task extractResourcesAsync(
                Context context, 
                PatchFilePath filePath, 
                PackagedResourceEntry resource, 
                FileSystemPath outputDir)
            {
                FileSystemPath fileDest = outputDir + filePath.FullFilePath;
                Directory.CreateDirectory(fileDest.Parent);

                using FileStream dstStream = File.Create(fileDest);
                using Stream srcStream = resource.ReadData(context, true);

                await srcStream.CopyToAsync(dstStream);
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Extracting patch library");

            await MessageUI.DisplayExceptionMessageAsync(ex, "An error occurred when extracting the patch library");
        }
    }

    public async Task RunLoadOperationAsync()
    {
        using (LoadState state = await App.LoaderViewModel.RunAsync("Debug load operation"))
        {
            await Task.Run(async () =>
            {
                try
                {
                    const int max = 800;
                    for (int i = 0; i < max; i++)
                    {
                        state.SetProgress(new Progress(i, max));
                        await Task.Delay(10, state.CancellationToken);

                        if (i == max / 2)
                            state.SetCanCancel(true);
                    }

                    state.SetProgress(new Progress(max, max));
                }
                catch (OperationCanceledException)
                {
                    // Ignore
                }
            });
        }
    }

    #endregion

    #region Records

    private record WebPatchEntry(string FilePath, Version MinVersion);

    #endregion

    #region Enums

    /// <summary>
    /// The available debug dialog types
    /// </summary>
    public enum DebugDialogType
    {
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
        /// Display the descriptor info available for each game
        /// </summary>
        GameDescriptor,

        /// <summary>
        /// Displays the install sizes for each game
        /// </summary>
        GameSizes,
    }

    #endregion
}