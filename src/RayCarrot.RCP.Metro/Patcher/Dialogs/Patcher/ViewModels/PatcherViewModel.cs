using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Windows.Input;
using BinarySerializer;

namespace RayCarrot.RCP.Metro.Patcher;

public class PatcherViewModel : BaseViewModel, IDisposable
{
    #region Constructor

    private PatcherViewModel(GameInstallation gameInstallation, Context context)
    {
        GameInstallation = gameInstallation;
        GameDirectory = gameInstallation.InstallLocation;
        Library = new PatchLibrary(GameDirectory, Services.File);

        LocalPatches = new ObservableCollection<LocalPatchViewModel>();
        DisplayedExternalPatches = new ObservableCollection<ExternalPatchViewModel>();
        PatchedFiles = new ObservableCollection<PatchedFileViewModel>();

        LoaderViewModel = new LoaderViewModel();

        _context = context;

        AddPatchCommand = new AsyncRelayCommand(AddPatchAsync);
    }

    private PatcherViewModel(GameInstallation gameInstallation, Context context, PendingPatch[] pendingPatchFiles) 
        : this(gameInstallation, context)
    {
        _pendingPatchFiles = pendingPatchFiles;
    }

    public PatcherViewModel(GameInstallation gameInstallation) : this(gameInstallation, new RCPContext(String.Empty)) { }

    #endregion

    #region Logger

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    #endregion

    #region Commands

    public ICommand AddPatchCommand { get; }

    #endregion

    #region Private Fields

    private PendingPatch[]? _pendingPatchFiles;

    private readonly HashSet<string> _removedPatches = new();
    private readonly Context _context;

    private ExternalPatchViewModel[]? _externalPatches;
    private Uri? _externalGamePatchesURL;

    private LocalPatchViewModel? _selectedLocalPatch;
    private ExternalPatchViewModel? _selectedExternalPatch;

    #endregion

    #region Public Properties

    public GameInstallation GameInstallation { get; }
    public FileSystemPath GameDirectory { get; }
    
    public PatchLibrary Library { get; }
    public ObservableCollection<DuoGridItemViewModel>? LibraryInfo { get; set; }

    public ObservableCollection<LocalPatchViewModel> LocalPatches { get; }
    public ObservableCollection<ExternalPatchViewModel> DisplayedExternalPatches { get; }

    public LocalPatchViewModel? SelectedLocalPatch
    {
        get => _selectedLocalPatch;
        set
        {
            _selectedLocalPatch = value;

            if (value != null)
            {
                SelectedExternalPatch = null;
                SelectedPatch = value;
            }
            else if (SelectedExternalPatch == null)
            {
                SelectedPatch = null;
            }
        }
    }
    public ExternalPatchViewModel? SelectedExternalPatch
    {
        get => _selectedExternalPatch;
        set
        {
            _selectedExternalPatch = value;

            if (value != null)
            {
                SelectedLocalPatch = null;
                SelectedPatch = value;
            }
            else if (SelectedLocalPatch == null)
            {
                SelectedPatch = null;
            }
        }
    }
    public PatchViewModel? SelectedPatch { get; private set; }

    public ObservableCollection<PatchedFileViewModel> PatchedFiles { get; set; }

    public LoaderViewModel LoaderViewModel { get; }
    public bool IsLoadingExternalPatches { get; set; }
    [MemberNotNullWhen(true, nameof(_externalPatches), nameof(_externalGamePatchesURL))]
    public bool HasLoadedExternalPatches => _externalPatches != null && _externalGamePatchesURL != null;
    public bool HasChanges { get; set; }

    #endregion

    #region Private Static Methods

    private static async Task<PatchFile?> ReadPatchFileAsync(Context context, FileSystemPath patchFilePath)
    {
        try
        {
            return context.ReadRequiredFileData<PatchFile>(patchFilePath, removeFileWhenComplete: false);
        }
        catch (UnsupportedFormatVersionException ex)
        {
            Logger.Warn(ex, "Adding patch");

            await Services.MessageUI.DisplayMessageAsync(Resources.Patcher_ReadPatchNewerVersionError, MessageType.Error);
            return null;
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Adding patch");

            await Services.MessageUI.DisplayExceptionMessageAsync(ex, Resources.Patcher_ReadPatchGenericError);
            return null;
        }
    }

    #endregion

    #region Private Methods

    private void AddPatchFromFile(Context context, PatchFile patchFile, FileSystemPath filePath)
    {
        PatchMetadata metaData = patchFile.Metadata;

        // Check for a conflict
        LocalPatchViewModel? conflict = LocalPatches.FirstOrDefault(x => x.ID == metaData.ID);
        int? insertIndex = null;

        // Replace if there's a conflict
        if (conflict != null)
        {
            insertIndex = LocalPatches.IndexOf(conflict);
            RemovePatch(conflict, false, false);
        }

        PendingImportedLocalPatchViewModel patchViewModel = new(this, patchFile, false, filePath);
        patchViewModel.LoadThumbnail(context);

        if (insertIndex == null)
            LocalPatches.Add(patchViewModel);
        else
            LocalPatches.Insert(insertIndex.Value, patchViewModel);

        if (conflict?.IsEnabled == true)
            patchViewModel.IsEnabled = true;

        SelectedLocalPatch = patchViewModel;

        Logger.Info("Added patch '{0}' from file with version {1} and ID {2}", metaData.Name, metaData.Version, metaData.ID);
    }

    private void AddDownloadedPatchFromFile(Context context, PatchFile patchFile, FileSystemPath filePath, TempDirectory tempDir, int? insertIndex = null, bool isEnabled = false)
    {
        DownloadedLocalPatchViewModel patchViewModel = new(this, patchFile, false, filePath, tempDir);
        patchViewModel.LoadThumbnail(context);

        if (insertIndex == null)
            LocalPatches.Add(patchViewModel);
        else
            LocalPatches.Insert(insertIndex.Value, patchViewModel);

        if (isEnabled)
            patchViewModel.IsEnabled = true;

        SelectedLocalPatch = patchViewModel;

        PatchMetadata metaData = patchFile.Metadata;
        Logger.Info("Added patch '{0}' from downloaded file with version {1} and ID {2}", metaData.Name, metaData.Version, metaData.ID);
    }

    private void AddPatchFromLibrary(PatchLibrary library, PatchLibraryPatchEntry patchEntry)
    {
        using (_context)
        {
            PatchFile? patchFile = _context.ReadFileData<PatchFile>(library.GetPatchFilePath(patchEntry.ID), removeFileWhenComplete: false);

            if (patchFile == null)
            {
                Logger.Warn("Patch with ID {0} was not found in library", patchEntry.ID);
                return;
            }

            ExistingLocalPatchViewModel patchViewModel = new(this, patchFile, patchEntry.IsEnabled, library);
            patchViewModel.LoadThumbnail(_context);
            LocalPatches.Add(patchViewModel);

            PatchMetadata metaData = patchFile.Metadata;
            Logger.Info("Added patch '{0}' from library with version {1} and ID {2}", metaData.Name, metaData.Version, metaData.ID);
        }
    }

    private void ClearLocalPatches()
    {
        foreach (LocalPatchViewModel patch in LocalPatches)
            _context.RemoveFile(patch.FilePath);

        LocalPatches.DisposeAll();
        LocalPatches.Clear();
    }

    private async Task<bool> LoadExistingPatchesAsync()
    {
        Logger.Info("Loading existing patches");

        // Clear any previously loaded patches
        ClearLocalPatches();

        // Read the library file
        PatchLibraryFile? libraryFile;

        try
        {
            Logger.Info("Reading patch library file");

            using (_context)
                libraryFile = _context.ReadFileData<PatchLibraryFile>(Library.LibraryFilePath, removeFileWhenComplete: true);

            if (libraryFile == null)
            {
                LibraryInfo = null;
                Logger.Info("The library file does not exist");
            }
            else
            {
                LibraryInfo = new ObservableCollection<DuoGridItemViewModel>()
                {
                    new(new ResourceLocString(nameof(Resources.Patcher_LibraryInfo_Game)), GameInstallation.GameDescriptor.GameDescriptorName),
                    new(new ResourceLocString(nameof(Resources.Patcher_LibraryInfo_Patches)), libraryFile.Patches.Length.ToString()),
                    new(new ResourceLocString(nameof(Resources.Patcher_LibraryInfo_AppliedPatches)), libraryFile.Patches.Count(x => x.IsEnabled).ToString()),
                    new(new ResourceLocString(nameof(Resources.Patcher_LibraryInfo_ModifiedDate)), libraryFile.History.ModifiedDate.ToString(CultureInfo.CurrentCulture)),
                    new(new ResourceLocString(nameof(Resources.Patcher_LibraryInfo_FormatVersion)), libraryFile.FormatVersion.ToString(), UserLevel.Debug),
                    new(new ResourceLocString(nameof(Resources.Patcher_LibraryInfo_Location)), Library.DirectoryPath.FullPath, UserLevel.Debug),
                    new(new ResourceLocString(nameof(Resources.Patcher_LibraryInfo_AddedFiles)), libraryFile.History.AddedFiles.Length.ToString()),
                    new(new ResourceLocString(nameof(Resources.Patcher_LibraryInfo_ReplacedFiles)), libraryFile.History.ReplacedFiles.Length.ToString()),
                    new(new ResourceLocString(nameof(Resources.Patcher_LibraryInfo_RemovedFiles)), libraryFile.History.RemovedFiles.Length.ToString()),
                };
                Logger.Info("Read patch library file with format version {0}", libraryFile.FormatVersion);
            }
        }
        catch (UnsupportedFormatVersionException ex)
        {
            Logger.Warn(ex, "Reading library file");

            LibraryInfo = null;

            await Services.MessageUI.DisplayMessageAsync(Resources.Patcher_ReadLibraryNewerVersionError, MessageType.Error);

            return false;
        }

        // Verify game
        if (libraryFile != null && !libraryFile.IsGameValid(GameInstallation.GameDescriptor))
        {
            Logger.Warn("Failed to load library due to the game {0} not matching the current one ({1})", libraryFile.GameId, GameInstallation.FullId);

            await Services.MessageUI.DisplayMessageAsync(String.Format(Resources.Patcher_ReadLibraryGameMismatchError, GameInstallation.GameDescriptor.GameDescriptorName),
                MessageType.Error);

            return false;
        }

        if (libraryFile == null)
            return true;

        foreach (PatchLibraryPatchEntry patch in libraryFile.Patches)
            AddPatchFromLibrary(Library, patch);

        RefreshDisplayExternalPatches();

        Logger.Info("Loaded {0} patches", libraryFile.Patches.Length);

        return true;
    }

    private async Task<bool> VerifyPatchSecurityAsync(PatchFile patch)
    {
        // Check if the patch adds or replaces exe or dll files. Expand to check other file types too?
        bool hasCodeFiles = patch.AddedFiles.Any(x =>
            x.FilePath.EndsWith(".exe", StringComparison.InvariantCultureIgnoreCase) ||
            x.FilePath.EndsWith(".dll", StringComparison.InvariantCultureIgnoreCase));

        // Have the user verify
        if (hasCodeFiles)
        {
            Logger.Info("Patch with ID {0} contains one or more potentially harmful files", patch.Metadata.ID);

            return await Services.MessageUI.DisplayMessageAsync(String.Format(Resources.Patcher_SecurityWarning, patch.Metadata.Name), 
                MessageType.Question, true);
        }
        else
        {
            return true;
        }
    }

    #endregion

    #region Public Static Methods

    public static async Task<PatcherViewModel?> FromFilesAsync(FileSystemPath[] patchFilePaths)
    {
        if (patchFilePaths.Length == 0)
            throw new ArgumentException("There has to be a least one patch file provided", nameof(patchFilePaths));

        // Create the context. Normally we do this in the constructor, but we need
        // to read the patch file first here, so we create it earlier
        RCPContext context = new(String.Empty);

        try
        {
            // Read the first patch file. This will determine the game.
            PatchFile? patch = await ReadPatchFileAsync(context, patchFilePaths[0]);

            // If the patch file could not be read then we return null
            if (patch == null)
                return null;

            // Get all the installations which the patch supports
            List<GameInstallation> gameInstallations = Services.Games.GetInstalledGames().
                Where(x => patch.Metadata.IsGameValid(x.GameDescriptor)).
                ToList();

            // Make sure there is an installed game which can be patched
            if (!gameInstallations.Any())
            {
                // TODO-14: Update error message
                //await Services.MessageUI.DisplayMessageAsync(String.Format(Resources.Patcher_ReadPatchGameNotAddedError,
                //        patch.Metadata.Name, patch.Metadata.Game.GetGameDescriptor().DisplayName), MessageType.Error);
                return null;
            }

            GameInstallation gameInstallation;

            // If there is more than 1 matching game we ask the user which one to patch
            if (gameInstallations.Count > 1)
            {
                GamesSelectionResult result = await Services.UI.SelectGamesAsync(new GamesSelectionViewModel(gameInstallations)
                {
                    // TODO-UPDATE: Localize
                    Title = "Select game to patch"
                });

                if (result.CanceledByUser)
                    return null;

                gameInstallation = result.SelectedGame;
            }
            else
            {
                gameInstallation = gameInstallations.First();
            }

            // Create the view model
            PendingPatch[] pendingPatches = patchFilePaths.Select((x, i) => new PendingPatch(x, i == 0 ? patch : null)).ToArray();
            return new PatcherViewModel(gameInstallation, context, pendingPatches);
        }
        catch
        {
            context.Dispose();
            throw;
        }
    }

    #endregion

    #region Public Methods

    public void RefreshPatchedFiles()
    {
        Dictionary<string, PatchedFileViewModel> files = new();

        foreach (LocalPatchViewModel patchViewModel in LocalPatches.Where(x => x.IsEnabled))
        {
            PatchFile patch = patchViewModel.PatchFile;

            foreach (PatchFilePath addedFile in patch.AddedFiles)
                addFile(addedFile, PatchedFileViewModel.PatchedFileModification.Add);

            foreach (PatchFilePath removedFile in patch.RemovedFiles)
                addFile(removedFile, PatchedFileViewModel.PatchedFileModification.Remove);

            void addFile(PatchFilePath fileName, PatchedFileViewModel.PatchedFileModification modification)
            {
                string key = fileName.HasLocation ? $"{fileName.Location}:{fileName.FilePath}" : fileName.FilePath;

                key = key.ToLowerInvariant().Replace('\\', '/');
                
                if (files.ContainsKey(key))
                    files[key].OverridenPatches.Add(patch.Metadata);
                else
                    files.Add(key, new PatchedFileViewModel(fileName, modification, patch.Metadata));
            }
        }

        PatchedFiles = new ObservableCollection<PatchedFileViewModel>(files.Values.
            OrderBy(x => x.FilePath.Location).
            ThenBy(x => x.FilePath.FilePath));

        Logger.Info("Refreshed patches files");
    }

    public async Task AddPatchAsync()
    {
        FileBrowserResult result = await Services.BrowseUI.BrowseFileAsync(new FileBrowserViewModel
        {
            Title = Resources.Patcher_AddPatchHeader,
            DefaultDirectory = default,
            DefaultName = null,
            ExtensionFilter = new FileFilterItem($"*{PatchFile.FileExtension}", Resources.Patcher_FileType).StringRepresentation,
            MultiSelection = true
        });

        if (result.CanceledByUser)
            return;

        Logger.Info("Adding {0} patches", result.SelectedFiles.Length);

        using (_context)
        {
            foreach (FileSystemPath patchFilePath in result.SelectedFiles)
            {
                if (_context.FileExists(patchFilePath))
                {
                    Logger.Trace("Skip adding patch from {0} due to it already having been added", patchFilePath);
                    continue;
                }

                Logger.Trace("Adding patch from {0}", patchFilePath);

                PatchFile? patch = await ReadPatchFileAsync(_context, patchFilePath);

                if (patch == null)
                    continue;

                PatchMetadata metaData = patch.Metadata;

                // Verify game
                if (!metaData.IsGameValid(GameInstallation.GameDescriptor))
                {
                    Logger.Warn("Failed to add patch due to the current game {0} not being supported", GameInstallation.FullId);

                    // TODO-14: Update error message
                    //await Services.MessageUI.DisplayMessageAsync(String.Format(Resources.Patcher_ReadPatchGameMismatchError,
                    //        metaData.Game.GetGameDescriptor().DisplayName), MessageType.Error);

                    _context.RemoveFile(patchFilePath);

                    continue;
                }

                // Verify the security
                if (!await VerifyPatchSecurityAsync(patch))
                {
                    _context.RemoveFile(patchFilePath);
                    continue;
                }

                // Add the patch view model so we can work with it
                AddPatchFromFile(_context, patch, patchFilePath);

                HasChanges = true;
            }
        }

        RefreshDisplayExternalPatches();

        Logger.Info("Added patches");
    }

    public async Task DownloadPatchAsync(ExternalPatchManifest externalManifest)
    {
        if (_externalGamePatchesURL == null)
            throw new Exception("Attempted to download patch before the URL was set");

        Logger.Info("Downloading external patch");

        TempDirectory tempDir = new(true);

        try
        {
            Uri patchURL = new(_externalGamePatchesURL, externalManifest.Patch);

            bool result = await Services.App.DownloadAsync(new[] { patchURL }, false, tempDir.TempPath);

            if (!result)
            {
                tempDir.Dispose();
                return;
            }

            // Due to how the downloading system currently works we need to get the path like this
            FileSystemPath patchFilePath = tempDir.TempPath + Path.GetFileName(patchURL.AbsoluteUri);

            using (_context)
            {
                // Read the patch file
                PatchFile patch = _context.ReadRequiredFileData<PatchFile>(patchFilePath, removeFileWhenComplete: false);

                // Remove any existing patch with the same ID (this allows it to be updated)
                LocalPatchViewModel? conflict = LocalPatches.FirstOrDefault(x => x.ID == patch.Metadata.ID);
                int? insertIndex = null;

                if (conflict != null)
                {
                    insertIndex = LocalPatches.IndexOf(conflict);
                    RemovePatch(conflict, false, false);
                }

                // Add the patch view model so we can work with it
                AddDownloadedPatchFromFile(_context, patch, patchFilePath, tempDir, insertIndex, conflict?.IsEnabled ?? false);

                HasChanges = true;

                RefreshDisplayExternalPatches();
            }

            Logger.Info("Added downloaded external patch");
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Adding downloaded patch");

            tempDir.Dispose();

            await Services.MessageUI.DisplayExceptionMessageAsync(ex, Resources.Patcher_ReadPatchGenericError);
        }
    }

    public async Task ExtractPatchContentsAsync(LocalPatchViewModel patchViewModel)
    {
        using (LoadState state = await LoaderViewModel.RunAsync(Resources.Patcher_ExtractContents_Status, canCancel: true))
        {
            DirectoryBrowserResult result = await Services.BrowseUI.BrowseDirectoryAsync(new DirectoryBrowserViewModel
            {
                Title = Resources.Browse_DestinationHeader,
            });

            if (result.CanceledByUser)
                return;

            Logger.Info("Extracting patch contents");

            try
            {
                PatchFile patchFile = patchViewModel.PatchFile;

                // Extract metadata
                JsonHelpers.SerializeToFile(patchFile.Metadata, result.SelectedDirectory + "metadata.json");

                // Extract resources
                using (_context)
                {
                    // Extract thumbnail
                    if (patchFile.HasThumbnail)
                    {
                        using Stream thumbOutputStream = File.Create(result.SelectedDirectory + "thumbnail.png");
                        await patchFile.ThumbnailResource.ReadData(_context, true).CopyToAsync(thumbOutputStream);
                    }

                    // Extract added files
                    for (int i = 0; i < patchFile.AddedFileResources.Length; i++)
                    {
                        state.CancellationToken.ThrowIfCancellationRequested();

                        PatchFilePath filePath = patchFile.AddedFiles[i];
                        PackagedResourceEntry resource = patchFile.AddedFileResources[i];

                        state.SetProgress(new Progress(i, patchFile.AddedFiles.Length));

                        FileSystemPath fileDest = result.SelectedDirectory + "added_files" + filePath.FullFilePath;
                        Directory.CreateDirectory(fileDest.Parent);

                        using FileStream dstStream = File.Create(fileDest);
                        using Stream srcStream = resource.ReadData(_context, true);

                        await srcStream.CopyToAsync(dstStream);
                    }

                    state.SetProgress(new Progress(patchFile.AddedFiles.Length, patchFile.AddedFiles.Length));
                }

                // Extract removed files
                File.WriteAllLines(result.SelectedDirectory + "removed_files.txt",
                    patchFile.RemovedFiles.Select(x => x.ToString()));

                Logger.Info("Extracted patch contents");

                await Services.MessageUI.DisplaySuccessfulActionMessageAsync(Resources.Patcher_ExtractContentsSuccess);
            }
            catch (OperationCanceledException ex)
            {
                Logger.Trace(ex, "Cancelled extracting patch contents");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Extracting patch contents");

                await Services.MessageUI.DisplayExceptionMessageAsync(ex, Resources.Patcher_ExtractContentsError);
            }
        }
    }

    public async Task ExportPatchAsync(LocalPatchViewModel patchViewModel)
    {
        using (await LoaderViewModel.RunAsync(Resources.Patcher_Export_Status))
        {
            SaveFileResult browseResult = await Services.BrowseUI.SaveFileAsync(new SaveFileViewModel()
            {
                Title = Resources.PatchCreator_CreateSaveFileHeader,
                Extensions = new FileFilterItem($"*{PatchFile.FileExtension}", Resources.Patcher_FileType).StringRepresentation,
            });

            if (browseResult.CanceledByUser)
                return;

            Logger.Info("Exporting patch");

            try
            {
                // Copy the patch file
                await Task.Run(() => Services.File.CopyFile(patchViewModel.FilePath, browseResult.SelectedFileLocation, true));

                Logger.Info("Exported patch");

                await Services.MessageUI.DisplaySuccessfulActionMessageAsync(Resources.Patcher_ExportSuccess);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Exported patch");

                await Services.MessageUI.DisplayExceptionMessageAsync(ex, Resources.Patcher_ExportError);
            }
        }
    }

    public void RemovePatch(LocalPatchViewModel patchViewModel, bool refreshExternalPatches, bool refreshPatchedFiles)
    {
        LocalPatches.Remove(patchViewModel);
        _removedPatches.Add(patchViewModel.ID);

        if (SelectedPatch == patchViewModel)
            SelectedPatch = null;

        // If the patch was enabled we need to refresh the patched files
        if (patchViewModel.IsEnabled && refreshPatchedFiles)
            RefreshPatchedFiles();

        // Remove the file from the context
        _context.RemoveFile(patchViewModel.FilePath);

        patchViewModel.Dispose();
        HasChanges = true;

        if (refreshExternalPatches)
            RefreshDisplayExternalPatches();

        Logger.Info("Removed patch '{0}' with version {1} and ID {2}", patchViewModel.Name, patchViewModel.Metadata.Version, patchViewModel.ID);
    }

    public async Task<bool> InitializeAsync()
    {
        // Reset properties
        _removedPatches.Clear();
        HasChanges = false;
        SelectedLocalPatch = null;
        SelectedExternalPatch = null;
        LibraryInfo = null;

        // Load patches
        bool success = await LoadPatchesAsync();

        if (!success)
            return false;

        // Add any pending patch files
        if (_pendingPatchFiles != null)
        {
            foreach (PendingPatch pendingPatch in _pendingPatchFiles)
            {
                FileSystemPath patchFilePath = pendingPatch.PatchFilePath;
                PatchFile? patch = pendingPatch.PatchFile;

                // Read the patch file if it hasn't already been read
                if (patch == null)
                {
                    // Read the patch file
                    patch = await ReadPatchFileAsync(_context, patchFilePath);

                    // Skip if it's still null
                    if (patch == null)
                        continue;
                }

                // Verify game
                if (!patch.Metadata.IsGameValid(GameInstallation.GameDescriptor))
                {
                    Logger.Warn("Failed to add pending patch due to the current game {0} not being supported", GameInstallation.FullId);

                    // Do not show message to user as at least one game from the pending patches will be added

                    _context.RemoveFile(patchFilePath);

                    continue;
                }

                // Verify the security
                if (!await VerifyPatchSecurityAsync(patch))
                {
                    _context.RemoveFile(patchFilePath);
                    continue;
                }

                // Add the patch file
                AddPatchFromFile(_context, patch, patchFilePath);
                HasChanges = true;
            }

            _pendingPatchFiles = null;
        }

        // Load external patches
        await LoadExternalPatchesAsync();

        // Refresh displayed external patches
        RefreshDisplayExternalPatches();

        return true;
    }

    public async Task<bool> LoadPatchesAsync()
    {
        Logger.Info("Loading patch library");

        try
        {
            bool success = await LoadExistingPatchesAsync();

            if (!success)
            {
                Logger.Warn("Failed to load patch library for game {0}", GameInstallation.FullId);
                return false;
            }

            RefreshPatchedFiles();

            Logger.Info("Loaded patch library for game {0}", GameInstallation.FullId);

            return true;
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Loading patches");

            await Services.MessageUI.DisplayExceptionMessageAsync(ex, Resources.Patcher_LoadError);

            return false;
        }
    }

    public async Task LoadExternalPatchesAsync()
    {
        if (!Services.Data.Patcher_LoadExternalPatches || HasLoadedExternalPatches)
            return;

        IsLoadingExternalPatches = true;

        try
        {
            Logger.Info("Loading external patches");

            ExternalPatchesManifest manifest =
                await JsonHelpers.DeserializeFromURLAsync<ExternalPatchesManifest>(AppURLs.PatchesManifestUrl);

            Logger.Info("Read external patches manifest with version {0}", manifest.ManifestVersion);

            if (manifest.ManifestVersion > ExternalPatchesManifest.LatestVersion)
            {
                Logger.Warn(
                    "Failed to load external patches due to the version number {0} being higher than the current one ({1})",
                    manifest.ManifestVersion, ExternalPatchesManifest.LatestVersion);

                await Services.MessageUI.DisplayMessageAsync(Resources.Patcher_LoadExternalNewerVersionError, MessageType.Error);

                _externalPatches = null;
                return;
            }

            // Make sure the game has external patches defined
            if (manifest.Games?.ContainsKey(GameInstallation.LegacyGame.Value) != true)
            {
                Logger.Info("The game {0} has no external patches", GameInstallation.FullId);
                _externalGamePatchesURL = null;
                _externalPatches = null;
                return;
            }

            Logger.Info("Loading external patches for game {0}", GameInstallation.FullId);

            _externalGamePatchesURL = new Uri(new Uri(AppURLs.PatchesManifestUrl), manifest.Games[GameInstallation.LegacyGame.Value]);

            ExternalGamePatchesManifest gameManifest =
                await JsonHelpers.DeserializeFromURLAsync<ExternalGamePatchesManifest>(_externalGamePatchesURL
                    .AbsoluteUri);

            Logger.Info("Loaded {0} external patches for game {1}", gameManifest.Patches?.Length, GameInstallation.FullId);

            if (gameManifest.Patches == null)
            {
                _externalPatches = null;
                return;
            }

            _externalPatches = gameManifest.Patches.
                Where(x => x.FormatVersion <= PatchFile.LatestFormatVersion && x.MinAppVersion <= Services.App.CurrentAppVersion).
                Select(x => new ExternalPatchViewModel(this, x)).
                ToArray();
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Loading external patches");

            await Services.MessageUI.DisplayExceptionMessageAsync(ex, Resources.Patcher_LoadExternalGenericError);

            _externalGamePatchesURL = null;
            _externalPatches = null;
        }
        finally
        {
            IsLoadingExternalPatches = false;
        }
    }

    public void RefreshDisplayExternalPatches()
    {
        Logger.Info("Refreshing displayed external patches");

        DisplayedExternalPatches.DisposeAll();
        DisplayedExternalPatches.Clear();

        if (!HasLoadedExternalPatches)
        {
            Logger.Info("The external patches haven't been loaded yet");
            return;
        }

        foreach (ExternalPatchViewModel externalPatch in _externalPatches)
        {
            string id = externalPatch.ID;
            PatchVersion version = externalPatch.ExternalManifest.Version;

            // TODO: Ideally access to the local patches collection should be locked as it might be modified on another thread
            // Don't show if it exists locally (except if the external revision is newer)
            if (LocalPatches.Any(x => x.ID == id && x.Metadata.Version >= version))
                continue;

            // Add view model
            DisplayedExternalPatches.Add(externalPatch);

            // Load the thumbnail
            _ = externalPatch.LoadThumbnailAsync(_externalGamePatchesURL);
        }
        
        Logger.Info("Refreshed displayed external patches");
    }

    public async Task<bool> ApplyAsync()
    {
        using (LoadState state = await LoaderViewModel.RunAsync(Resources.Patcher_Apply_Status))
        {
            Logger.Info("Applying patches");

            try
            {
                await Task.Run(async () =>
                { 
                    // Make sure the library is set up before we modify it
                    Library.Setup();

                    // Create a patcher
                    Patcher patcher = new(); // TODO: Use DI?

                    // Add pending patches to the library
                    foreach (var patchViewModel in LocalPatches.OfType<PendingImportedLocalPatchViewModel>())
                        Library.AddPatch(patchViewModel.FilePath, patchViewModel.ID, patchViewModel.MovePatch);

                    // Remove removed patches
                    foreach (string patch in _removedPatches.Where(x => LocalPatches.All(p => p.ID != x)))
                        Library.RemovePatch(patch);

                    // Get the current patch data
                    PatchLibraryPatchEntry[] patches = LocalPatches.Select(x => new PatchLibraryPatchEntry()
                    {
                        ID = x.ID,
                        IsEnabled = x.IsEnabled
                    }).ToArray();

                    bool success = await patcher.ApplyAsync(
                        gameInstallation: GameInstallation,
                        library: Library,
                        gameDirectory: GameDirectory,
                        patches: patches,
                        progressCallback: state.SetProgress);

                    Logger.Info("Applied patches");

                    if (success)
                        await Services.MessageUI.DisplaySuccessfulActionMessageAsync(Resources.Patcher_ApplySuccess);
                    else
                        await Services.MessageUI.DisplayMessageAsync(Resources.Patcher_ApplySuccessWithErrors, MessageType.Warning);
                });
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Applying patches");

                await Services.MessageUI.DisplayExceptionMessageAsync(ex, Resources.Patcher_ApplyError);
            }

            // No matter if it succeeds or fails we want to reset the state
            return await InitializeAsync();
        }
    }

    public void Dispose()
    {
        _context.Dispose();
        LocalPatches.DisposeAll();
        DisplayedExternalPatches.DisposeAll();
    }

    #endregion

    #region Data Types

    private record PendingPatch(FileSystemPath PatchFilePath, PatchFile? PatchFile);

    #endregion
}