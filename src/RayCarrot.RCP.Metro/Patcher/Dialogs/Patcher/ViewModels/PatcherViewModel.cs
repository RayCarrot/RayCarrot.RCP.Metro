﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using BinarySerializer;
using NLog;

namespace RayCarrot.RCP.Metro.Patcher;

public class PatcherViewModel : BaseViewModel, IDisposable
{
    #region Constructor

    public PatcherViewModel(Games game)
    {
        Game = game;
        GameDirectory = game.GetInstallDir();
        Library = new PatchLibrary(GameDirectory, Services.File);

        LocalPatches = new ObservableCollection<LocalPatchViewModel>();
        DisplayedExternalPatches = new ObservableCollection<ExternalPatchViewModel>();
        PatchedFiles = new ObservableCollection<PatchedFileViewModel>();

        LoadOperation = new BindableOperation();

        _context = new RCPContext(String.Empty);

        AddPatchCommand = new AsyncRelayCommand(AddPatchAsync);
    }

    #endregion

    #region Logger

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    #endregion

    #region Commands

    public ICommand AddPatchCommand { get; }

    #endregion

    #region Private Fields

    private readonly HashSet<string> _removedPatches = new();
    private readonly RCPContext _context;

    private ExternalPatchViewModel[]? _externalPatches;
    private Uri? _externalGamePatchesURL;

    private LocalPatchViewModel? _selectedLocalPatch;
    private ExternalPatchViewModel? _selectedExternalPatch;

    #endregion

    #region Public Properties

    public Games Game { get; }
    public FileSystemPath GameDirectory { get; }
    
    public PatchLibrary Library { get; }

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

    public BindableOperation LoadOperation { get; }
    public bool IsLoadingExternalPatches { get; set; }
    [MemberNotNullWhen(true, nameof(_externalPatches), nameof(_externalGamePatchesURL))]
    public bool HasLoadedExternalPatches => _externalPatches != null && _externalGamePatchesURL != null;
    public bool HasChanges { get; set; }

    #endregion

    #region Private Methods

    private void AddPatchFromFile(Context context, PatchFile patchFile, FileSystemPath filePath, int? insertIndex = null, bool isEnabled = false)
    {
        PendingImportedLocalPatchViewModel patchViewModel = new(this, patchFile, false, filePath);
        patchViewModel.LoadThumbnail(context);

        if (insertIndex == null)
            LocalPatches.Add(patchViewModel);
        else
            LocalPatches.Insert(insertIndex.Value, patchViewModel);

        if (isEnabled)
            patchViewModel.IsEnabled = true;

        SelectedLocalPatch = patchViewModel;

        PatchMetadata metaData = patchFile.Metadata;
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
                Logger.Info("The library file does not exist");
            else
                Logger.Info("Read patch library file with format version {0}", libraryFile.FormatVersion);
        }
        catch (UnsupportedFormatVersionException ex)
        {
            Logger.Warn(ex, "Reading library file");

            // TODO-UPDATE: Localize
            await Services.MessageUI.DisplayMessageAsync("The game patch library was made with a newer version of the Rayman Control Panel and can not be read", MessageType.Error);

            return false;
        }

        // Verify game
        if (libraryFile != null && libraryFile.Game != Game)
        {
            Logger.Warn("Failed to load library due to the game {0} not matching the current one ({1})", libraryFile.Game, Game);

            // TODO-UPDATE: Localize
            await Services.MessageUI.DisplayMessageAsync($"The game patch library was made with for {Game}", MessageType.Error);

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

        Logger.Info("Refresh patches files");
    }

    public async Task AddPatchAsync()
    {
        FileBrowserResult result = await Services.BrowseUI.BrowseFileAsync(new FileBrowserViewModel
        {
            // TODO-UPDATE: Localize
            Title = "Select patches to add",
            DefaultDirectory = default,
            DefaultName = null,
            ExtensionFilter = new FileFilterItem($"*{PatchFile.FileExtension}", "Game Patch").StringRepresentation,
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

                PatchFile patch;

                try
                {
                    patch = _context.ReadRequiredFileData<PatchFile>(patchFilePath, removeFileWhenComplete: false);
                }
                catch (UnsupportedFormatVersionException ex)
                {
                    Logger.Warn(ex, "Adding patch");

                    // TODO-UPDATE: Localize
                    await Services.MessageUI.DisplayMessageAsync(
                        "The selected patch was made with a newer version of the Rayman Control Panel and can thus not be read",
                        MessageType.Error);
                    continue;
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "Adding patch");

                    // TODO-UPDATE: Localize
                    await Services.MessageUI.DisplayExceptionMessageAsync(ex, "An error occurred when adding the patch");
                    continue;
                }

                PatchMetadata metaData = patch.Metadata;

                // Verify game
                if (metaData.Game != Game)
                {
                    Logger.Warn("Failed to add patch due to the specified game {0} not matching the current one ({1})",
                        metaData.Game, Game);

                    // TODO-UPDATE: Localize
                    await Services.MessageUI.DisplayMessageAsync(
                        $"The selected patch can only be applied to {metaData.Game.GetGameInfo().DisplayName}",
                        MessageType.Error);

                    continue;
                }

                LocalPatchViewModel? conflict = LocalPatches.FirstOrDefault(x => x.ID == metaData.ID);
                int? insertIndex = null;

                if (conflict != null)
                {
                    insertIndex = LocalPatches.IndexOf(conflict);
                    RemovePatch(conflict, false, false);
                }

                // Add the patch view model so we can work with it
                AddPatchFromFile(_context, patch, patchFilePath, insertIndex, conflict?.IsEnabled ?? false);

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

            // TODO-UPDATE: Localize
            await Services.MessageUI.DisplayExceptionMessageAsync(ex, "An error occurred when adding the patch");
        }
    }

    public async Task ExtractPatchContentsAsync(LocalPatchViewModel patchViewModel)
    {
        // TODO-UPDATE: Localize
        using (DisposableOperation operation = await LoadOperation.RunAsync("Extracting patch contents"))
        {
            DirectoryBrowserResult result = await Services.BrowseUI.BrowseDirectoryAsync(new DirectoryBrowserViewModel
            {
                // TODO-UPDATE: Localize
                Title = "Select destination",
            });

            if (result.CanceledByUser)
                return;

            Logger.Info("Extracting patch contents");

            try
            {
                PatchFile patchFile = patchViewModel.PatchFile;

                // Extract metadata
                JsonHelpers.SerializeToFile(patchFile.Metadata, result.SelectedDirectory + "Metadata.png");

                // Extract resources
                using (_context)
                {
                    // Extract thumbnail
                    if (patchFile.HasThumbnail)
                    {
                        using Stream thumbOutputStream = File.Create(result.SelectedDirectory + "Thumbnail.png");
                        await patchFile.ThumbnailResource.ReadData(_context, true).CopyToAsync(thumbOutputStream);
                    }

                    // Extract added files
                    for (int i = 0; i < patchFile.AddedFileResources.Length; i++)
                    {
                        PatchFilePath filePath = patchFile.AddedFiles[i];
                        PackagedResourceEntry resource = patchFile.AddedFileResources[i];
                        
                        operation.SetProgress(new Progress(i, patchFile.AddedFiles.Length));

                        FileSystemPath fileDest = result.SelectedDirectory + filePath.FullFilePath;
                        Directory.CreateDirectory(fileDest.Parent);

                        using FileStream dstStream = File.Create(fileDest);
                        using Stream srcStream = resource.ReadData(_context, true);

                        await srcStream.CopyToAsync(dstStream);
                    }

                    operation.SetProgress(new Progress(patchFile.AddedFiles.Length, patchFile.AddedFiles.Length));
                }

                // Extract removed files
                File.WriteAllLines(result.SelectedDirectory + "Removed Files.txt", patchFile.RemovedFiles.Select(x => x.ToString()));

                Logger.Info("Extracted patch contents");

                // TODO-UPDATE: Localize
                await Services.MessageUI.DisplaySuccessfulActionMessageAsync("The patch contents were successfully extracted");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Extracting patch contents");

                // TODO-UPDATE: Localize
                await Services.MessageUI.DisplayExceptionMessageAsync(ex, "An error occurred when extracting the patch contents");
            }
        }
    }

    public async Task ExportPatchAsync(LocalPatchViewModel patchViewModel)
    {
        // TODO-UPDATE: Localize
        using (await LoadOperation.RunAsync("Exporting patch"))
        {
            // TODO-UPDATE: Localize
            SaveFileResult browseResult = await Services.BrowseUI.SaveFileAsync(new SaveFileViewModel()
            {
                Title = "Save patch file",
                Extensions = new FileFilterItem($"*{PatchFile.FileExtension}", "Game Patch").StringRepresentation,
            });

            if (browseResult.CanceledByUser)
                return;

            Logger.Info("Exporting patch");

            try
            {
                // Copy the patch file
                await Task.Run(() => Services.File.CopyFile(patchViewModel.FilePath, browseResult.SelectedFileLocation, true));

                Logger.Info("Exported patch");

                // TODO-UPDATE: Localize
                await Services.MessageUI.DisplaySuccessfulActionMessageAsync("The patch was successfully exported");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Extracting patch");

                // TODO-UPDATE: Localize
                await Services.MessageUI.DisplayExceptionMessageAsync(ex, "An error occurred when extracting the patch");
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

    public async Task<bool> LoadPatchesAsync()
    {
        Logger.Info("Loading patch library");

        try
        {
            bool success = await LoadExistingPatchesAsync();

            if (!success)
            {
                Logger.Warn("Failed to load patch library for game {0}", Game);
                return false;
            }

            RefreshPatchedFiles();

            Logger.Info("Loaded patch library for game {0}", Game);

            return true;
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Loading patches");

            // TODO-UPDATE: Localize
            await Services.MessageUI.DisplayExceptionMessageAsync(ex, "An error occurred when loading the patches");

            return false;
        }
    }

    public async Task LoadExternalPatchesAsync()
    {
        if (!Services.Data.Patcher_LoadExternalPatches)
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

                // TODO-UPDATE: Localize
                await Services.MessageUI.DisplayMessageAsync(
                    "External patches could not be loaded due to using a newer format. Please update the Rayman Control Panel to continue being able to download external patches.",
                    MessageType.Error);

                _externalPatches = null;
                return;
            }

            // Make sure the game has external patches defined
            if (manifest.Games?.ContainsKey(Game) != true)
            {
                Logger.Info("The game {0} has no external patches", Game);
                _externalGamePatchesURL = null;
                _externalPatches = null;
                return;
            }

            Logger.Info("Loading external patches for game {0}", Game);

            _externalGamePatchesURL = new Uri(new Uri(AppURLs.PatchesManifestUrl), manifest.Games[Game]);

            ExternalGamePatchesManifest gameManifest =
                await JsonHelpers.DeserializeFromURLAsync<ExternalGamePatchesManifest>(_externalGamePatchesURL
                    .AbsoluteUri);

            Logger.Info("Loaded {0} external patches for game {1}", gameManifest.Patches?.Length, Game);

            if (gameManifest.Patches == null)
            {
                _externalPatches = null;
                return;
            }

            _externalPatches = gameManifest.Patches.
                Where(x => x.FormatVersion <= PatchFile.LatestFormatVersion).
                Select(x => new ExternalPatchViewModel(this, x)).
                ToArray();
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Loading external patches");

            // TODO-UPDATE: Localize
            await Services.MessageUI.DisplayExceptionMessageAsync(ex, "An error occurred when loading external patches");

            _externalGamePatchesURL = null;
            _externalPatches = null;
        }
        finally
        {
            IsLoadingExternalPatches = false;
            RefreshDisplayExternalPatches();
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
            Version version = externalPatch.ExternalManifest.Version;

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

    public async Task ApplyAsync()
    {
        // TODO-UPDATE: Localize
        using (DisposableOperation operation = await LoadOperation.RunAsync("Applying patches"))
        {
            Logger.Info("Applying patches");

            try
            {
                await Task.Run(async () =>
                {
                    // Create a patcher
                    Patcher patcher = new(); // TODO: Use DI?

                    // Add pending patches to the library
                    foreach (var patchViewModel in LocalPatches.OfType<PendingImportedLocalPatchViewModel>())
                        Library.AddPatch(patchViewModel.FilePath, patchViewModel.ID, patchViewModel.MovePatch);

                    // Remove removed patches
                    foreach (string patch in _removedPatches.Where(x => LocalPatches.All(p => p.ID != x)))
                        Library.RemovePatch(patch);

                    _removedPatches.Clear();

                    // Get the current patch data
                    PatchLibraryPatchEntry[] patches = LocalPatches.Select(x => new PatchLibraryPatchEntry()
                    {
                        ID = x.ID,
                        IsEnabled = x.IsEnabled
                    }).ToArray();

                    bool success = await patcher.ApplyAsync(
                        game: Game,
                        library: Library,
                        gameDirectory: GameDirectory,
                        patches: patches,
                        progressCallback: operation.SetProgress);

                    Logger.Info("Applied patches");

                    // TODO-UPDATE: Localize
                    if (success)
                        await Services.MessageUI.DisplaySuccessfulActionMessageAsync("Successfully applied all patches");
                    else
                        await Services.MessageUI.DisplayMessageAsync("Finished applying patches. Some files could not be modified.", MessageType.Warning);
                });
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Applying patches");

                // TODO-UPDATE: Localize
                await Services.MessageUI.DisplayExceptionMessageAsync(ex,
                    "An error occurred when applying the patches. Some files might still have been modified by patches.");
            }
            finally
            {
                // At this point we have to clear the local patches since the files might have been moved around
                ClearLocalPatches();
            }
        }
    }

    public void Dispose()
    {
        _context.Dispose();
        LocalPatches.DisposeAll();
        DisplayedExternalPatches.DisposeAll();
    }

    #endregion
}