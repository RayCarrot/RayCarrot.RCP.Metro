using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using NLog;

namespace RayCarrot.RCP.Metro.Patcher;

public class PatcherViewModel : BaseViewModel, IDisposable
{
    #region Constructor

    public PatcherViewModel(Games game)
    {
        Game = game;
        GameDirectory = game.GetInstallDir();
        Library = new PatchLibrary(PatchLibrary.GetLibraryDirectoryPath(GameDirectory), Services.File);

        LocalPatches = new ObservableCollection<LocalPatchViewModel>();
        ExternalPatches = new ObservableCollection<ExternalPatchViewModel>();
        PatchedFiles = new ObservableCollection<PatchedFileViewModel>();

        LoadOperation = new BindableOperation();

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

    private LocalPatchViewModel? _selectedLocalPatch;
    private ExternalPatchViewModel? _selectedExternalPatch;

    #endregion

    #region Public Properties

    public Games Game { get; }
    public FileSystemPath GameDirectory { get; }
    
    public PatchLibrary Library { get; }
    public PatchHistoryManifest? HistoryManifest { get; set; }

    public ExternalPatchManifest[]? ExternalPatchManifests { get; set; }
    public Uri? ExternalGamePatchesURL { get; set; }

    public ObservableCollection<LocalPatchViewModel> LocalPatches { get; }
    public ObservableCollection<ExternalPatchViewModel> ExternalPatches { get; }

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
    [MemberNotNullWhen(true, nameof(ExternalPatchManifests))]
    public bool HasLoadedExternalPatches => ExternalPatchManifests != null;
    public bool HasChanges { get; set; }

    #endregion

    #region Private Methods

    private void AddPatchFromFile(PatchFile patchFile, PatchManifest manifest)
    {
        PendingImportedLocalPatchViewModel patchViewModel = new(this, manifest, false, patchFile.FilePath);
        patchViewModel.LoadThumbnail();
        LocalPatches.Add(patchViewModel);

        Logger.Info("Added patch '{0}' from file with revision {1} and ID {2}", manifest.Name, manifest.Revision, manifest.ID);
    }

    private void AddDownloadedPatchFromFile(PatchFile patchFile, PatchManifest manifest, TempDirectory tempDir)
    {
        DownloadedLocalPatchViewModel patchViewModel = new(this, manifest, false, patchFile.FilePath, tempDir);
        patchViewModel.LoadThumbnail();
        LocalPatches.Add(patchViewModel);

        Logger.Info("Added patch '{0}' from downloaded file with revision {1} and ID {2}", manifest.Name, manifest.Revision, manifest.ID);
    }

    private void AddPatchFromLibrary(PatchLibrary library, PatchLibraryManifest libraryManifest, PatchManifest manifest)
    {
        ExistingLocalPatchViewModel patchViewModel = new( this, manifest, libraryManifest.EnabledPatches?.Contains(manifest.ID) == true, library);
        patchViewModel.LoadThumbnail();
        LocalPatches.Add(patchViewModel);

        Logger.Info("Added patch '{0}' from library with revision {1} and ID {2}", manifest.Name, manifest.Revision, manifest.ID);
    }

    private async Task<bool> LoadExistingPatchesAsync()
    {
        Logger.Info("Loading existing patches");

        // Clear any previously loaded patches
        LocalPatches.DisposeAll();
        LocalPatches.Clear();

        // Read the library manifest
        PatchLibraryManifest? libraryManifest = Library.ReadManifest();

        // Verify version
        if (libraryManifest is { LibraryVersion: > PatchLibraryManifest.LatestVersion })
        {
            Logger.Warn("Failed to load library due to the version number {0} being higher than the current one ({1})",
                libraryManifest.LibraryVersion, PatchLibraryManifest.LatestVersion);

            // TODO-UPDATE: Localize
            await Services.MessageUI.DisplayMessageAsync("The game patch library was made with a newer version of the Rayman Control Panel and can not be read", MessageType.Error);

            return false;
        }

        // Verify game
        if (libraryManifest != null && libraryManifest.Game != Game)
        {
            Logger.Warn("Failed to load library due to the game {0} not matching the current one ({1})", libraryManifest.Game, Game);

            // TODO-UPDATE: Localize
            await Services.MessageUI.DisplayMessageAsync($"The game patch library was made with for {Game}", MessageType.Error);

            return false;
        }

        HistoryManifest = libraryManifest?.History;

        if (libraryManifest?.Patches is null)
            return true;

        foreach (PatchManifest patchManifest in libraryManifest.Patches)
            AddPatchFromLibrary(Library, libraryManifest, patchManifest);

        RefreshExternalPatches();

        Logger.Info("Loaded {0} patches", libraryManifest.Patches.Length);

        return true;
    }

    #endregion

    #region Public Methods

    public void RefreshPatchedFiles()
    {
        Dictionary<string, PatchedFileViewModel> files = new();

        foreach (LocalPatchViewModel patchViewModel in LocalPatches.Where(x => x.IsEnabled))
        {
            PatchManifest patch = patchViewModel.Manifest;

            if (patch.AddedFiles != null)
                foreach (PatchFilePath addedFile in patch.AddedFiles)
                    addFile(addedFile, PatchedFileViewModel.PatchedFileModification.Add);

            if (patch.RemovedFiles != null)
                foreach (PatchFilePath removedFile in patch.RemovedFiles)
                    addFile(removedFile, PatchedFileViewModel.PatchedFileModification.Remove);

            void addFile(PatchFilePath fileName, PatchedFileViewModel.PatchedFileModification modification)
            {
                string key = fileName.HasLocation ? $"{fileName.Location}:{fileName.FilePath}" : fileName.FilePath;

                key = key.ToLowerInvariant().Replace('\\', '/');
                
                if (files.ContainsKey(key))
                    files[key].OverridenPatches.Add(patch);
                else
                    files.Add(key, new PatchedFileViewModel(fileName, modification, patch));
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

        foreach (FileSystemPath patchFile in result.SelectedFiles)
        {
            Logger.Trace("Adding patch from {0}", patchFile);

            try
            {
                using PatchFile patch = new(patchFile, true);

                PatchManifest manifest = patch.ReadManifest();

                // Verify version
                if (manifest.PatchVersion > PatchManifest.LatestVersion)
                {
                    Logger.Warn("Failed to add patch due to the version number {0} being higher than the current one ({1})",
                        manifest.PatchVersion, PatchManifest.LatestVersion);

                    // TODO-UPDATE: Localize
                    await Services.MessageUI.DisplayMessageAsync("The selected patch was made with a newer version of the Rayman Control Panel and can thus not be read", MessageType.Error);

                    patch.Dispose();
                    continue;
                }

                // Verify game
                if (manifest.Game != Game)
                {
                    Logger.Warn("Failed to add patch due to the specified game {0} not matching the current one ({1})",
                        manifest.Game, Game);

                    // TODO-UPDATE: Localize
                    await Services.MessageUI.DisplayMessageAsync($"The selected patch can only be applied to {manifest.Game.GetGameInfo().DisplayName}", MessageType.Error);

                    patch.Dispose();
                    continue;
                }

                LocalPatchViewModel? conflict = LocalPatches.FirstOrDefault(x => x.Manifest.ID == manifest.ID);

                if (conflict != null)
                {
                    Logger.Warn("Failed to add patch due to the ID {0} conflicting with an existing patch",
                        manifest.ID);

                    // TODO-UPDATE: Localize
                    await Services.MessageUI.DisplayMessageAsync($"The patch {manifest.Name} conflicts with the existing patch {conflict.Manifest.Name}. Please remove the conflicting patch before adding the new one.",
                        "Patch conflict", MessageType.Error);

                    patch.Dispose();
                    continue;
                }

                // Add the patch view model so we can work with it
                AddPatchFromFile(patch, manifest);

                HasChanges = true;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Adding patch");

                // TODO-UPDATE: Localize
                await Services.MessageUI.DisplayExceptionMessageAsync(ex, "An error occurred when adding the patch");
            }
        }

        RefreshExternalPatches();

        Logger.Info("Added patches");
    }

    public async Task DownloadPatchAsync(ExternalPatchManifest externalManifest)
    {
        if (ExternalGamePatchesURL == null)
            throw new Exception("Attempted to download patch before the URL was set");

        // TODO-UPDATE: Verify there is no patch added with conflicting ID. Shouldn't be possible, but make sure!
        // TODO-UPDATE: Log

        TempDirectory tempDir = new(true);

        try
        {
            Uri patchURL = new(ExternalGamePatchesURL, externalManifest.Patch);

            bool result = await Services.App.DownloadAsync(new[] { patchURL }, false, tempDir.TempPath);

            if (!result)
            {
                tempDir.Dispose();
                return;
            }

            // Due to how the downloading system currently works we need to get the path like this
            FileSystemPath patchFilePath = tempDir.TempPath + Path.GetFileName(patchURL.AbsoluteUri);

            // Open the patch file
            using PatchFile patch = new(patchFilePath, readOnly: true);
            
            // Read the manifest
            PatchManifest manifest = patch.ReadManifest();

            // Add the patch view model so we can work with it
            AddDownloadedPatchFromFile(patch, manifest, tempDir);

            HasChanges = true;

            RefreshExternalPatches();
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

            PatchManifest manifest = patchViewModel.Manifest;

            Logger.Info("Extracting patch contents");

            try
            {
                // Extract resources
                if (manifest.AddedFiles != null)
                {
                    using PatchFile patch = patchViewModel.ReadPatchFile();

                    int fileIndex = 0;

                    foreach (PatchFilePath addedFile in manifest.AddedFiles)
                    {
                        operation.SetProgress(new Progress(fileIndex, manifest.AddedFiles.Length));
                        fileIndex++;

                        FileSystemPath fileDest = result.SelectedDirectory + addedFile.FullFilePath;
                        Directory.CreateDirectory(fileDest.Parent);

                        using FileStream dstStream = File.Create(fileDest);
                        using Stream srcStream = patch.GetPatchResource(addedFile);

                        await srcStream.CopyToAsync(dstStream);
                    }

                    operation.SetProgress(new Progress(fileIndex, manifest.AddedFiles.Length));
                }

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

            // TODO-UPDATE: Re-implement

            //PatchManifest manifest = patchViewModel.Manifest;
            //IPatchDataSource src = patchViewModel.DataSource;

            //Logger.Info("Exporting patch from library");

            //try
            //{
            //    await Task.Run(() =>
            //    {
            //        // Create a new patch
            //        using PatchFile patchFile = new(browseResult.SelectedFileLocation);

            //        // Copy resources
            //        if (manifest.AddedFiles != null)
            //        {
            //            foreach (PatchFilePath addedFile in manifest.AddedFiles)
            //            {
            //                using Stream srcStream = src.GetResource(addedFile);
            //                patchFile.AddPatchResource(addedFile, srcStream);
            //            }
            //        }

            //        // Copy assets
            //        if (manifest.Assets != null)
            //        {
            //            foreach (string asset in manifest.Assets)
            //            {
            //                using Stream srcStream = src.GetAsset(asset);
            //                patchFile.AddPatchAsset(asset, srcStream);
            //            }
            //        }

            //        // Write the manifest
            //        patchFile.WriteManifest(manifest);

            //        patchFile.Apply();
            //    });

            //    Logger.Info("Exported patch");

            //    // TODO-UPDATE: Localize
            //    await Services.MessageUI.DisplaySuccessfulActionMessageAsync("The patch was successfully exported");
            //}
            //catch (Exception ex)
            //{
            //    Logger.Error(ex, "Extracting patch");

            //    // TODO-UPDATE: Localize
            //    await Services.MessageUI.DisplayExceptionMessageAsync(ex, "An error occurred when extracting the patch");
            //}
        }
    }

    public void RemovePatch(LocalPatchViewModel patchViewModel, bool refreshExternalPatches)
    {
        PatchManifest manifest = patchViewModel.Manifest;

        LocalPatches.Remove(patchViewModel);
        _removedPatches.Add(manifest.ID);

        if (SelectedPatch == patchViewModel)
            SelectedPatch = null;

        // If the patch was enabled we need to refresh the patched files
        if (patchViewModel.IsEnabled)
            RefreshPatchedFiles();

        patchViewModel.Dispose();
        HasChanges = true;

        if (refreshExternalPatches)
            RefreshExternalPatches();

        Logger.Info("Removed patch '{0}' with revision {1} and ID {2}", manifest.Name, manifest.Revision, manifest.ID);
    }

    public async Task UpdatePatchAsync(LocalPatchViewModel patchViewModel)
    {
        FileBrowserResult result = await Services.BrowseUI.BrowseFileAsync(new FileBrowserViewModel
        {
            // TODO-UPDATE: Localize
            Title = "Select updated patch",
            DefaultDirectory = default,
            DefaultName = null,
            ExtensionFilter = new FileFilterItem($"*{PatchFile.FileExtension}", "Game Patch").StringRepresentation,
        });

        if (result.CanceledByUser)
            return;

        Logger.Info("Updating patch '{0}' with revision {1} and ID {2}",
            patchViewModel.Manifest.Name, patchViewModel.Manifest.Revision, patchViewModel.Manifest.ID);

        try
        {
            using PatchFile patch = new(result.SelectedFile, true);

            PatchManifest manifest = patch.ReadManifest();

            // Verify version
            if (manifest.PatchVersion > PatchManifest.LatestVersion)
            {
                Logger.Warn("Failed to update patch due to the version number {0} being higher than the current one ({1})",
                    manifest.PatchVersion, PatchManifest.LatestVersion);

                // TODO-UPDATE: Localize
                await Services.MessageUI.DisplayMessageAsync("The selected patch was made with a newer version of the Rayman Control Panel and can thus not be read", MessageType.Error);

                patch.Dispose();
                return;
            }

            // Verify the ID
            if (patchViewModel.Manifest.ID != manifest.ID)
            {
                Logger.Warn("Failed to update patch due to the selected patch ID {0} not matching the original ID {1}",
                    manifest.ID, patchViewModel.Manifest.ID);

                // TODO-UPDATE: Localize
                await Services.MessageUI.DisplayMessageAsync($"The selected patch does not match and can not be used to update {patchViewModel.Manifest.Name}.", MessageType.Error);

                patch.Dispose();
                return;
            }

            // Verify the revision is newer
            if (patchViewModel.Manifest.Revision > manifest.Revision)
            {
                Logger.Warn("Failed to update patch due to the selected patch revision {0} being less than or equal to the original revision {1}",
                    manifest.Revision, patchViewModel.Manifest.Revision);

                // TODO-UPDATE: Localize
                await Services.MessageUI.DisplayMessageAsync($"The selected patch revision is lower or the same as the current revision and can not be used to update {patchViewModel.Manifest.Name}.", MessageType.Error);

                patch.Dispose();
                return;
            }

            // Remove the current patch
            RemovePatch(patchViewModel, false);

            // Add the updated patch
            AddPatchFromFile(patch, manifest);

            HasChanges = true;

            Logger.Info("Updated patch to revision {0}", manifest.Revision);
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Updated patch with ID {0}", patchViewModel.Manifest.ID);

            // TODO-UPDATE: Localize
            await Services.MessageUI.DisplayExceptionMessageAsync(ex, "An error occurred when updating the patch");
        }

        RefreshExternalPatches();
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
        IsLoadingExternalPatches = true;

        try
        {
            // TODO-UPDATE: Try/catch
            // TODO-UPDATE: Log

            ExternalPatchesManifest manifest = await JsonHelpers.DeserializeFromURLAsync<ExternalPatchesManifest>(AppURLs.PatchesManifestUrl);

            if (manifest.ManifestVersion > ExternalPatchesManifest.LatestVersion)
            {
                Logger.Warn("Failed to load external patches due to the version number {0} being higher than the current one ({1})",
                    manifest.ManifestVersion, ExternalPatchesManifest.LatestVersion);

                // TODO-UPDATE: Localize
                await Services.MessageUI.DisplayMessageAsync("External patches could not be loaded due to using a newer format. Please update the Rayman Control Panel to continue being able to download external patches.", MessageType.Error);

                ExternalPatchManifests = null;
                return;
            }

            // Make sure the game has external patches defined
            if (manifest.Games?.ContainsKey(Game) != true)
            {
                ExternalPatchManifests = null;
                return;
            }

            ExternalGamePatchesURL = new Uri(new Uri(AppURLs.PatchesManifestUrl), manifest.Games[Game]);

            ExternalGamePatchesManifest gameManifest = await JsonHelpers.DeserializeFromURLAsync<ExternalGamePatchesManifest>(ExternalGamePatchesURL.AbsoluteUri);

            if (gameManifest.Patches == null)
            {
                ExternalPatchManifests = null;
                return;
            }

            ExternalPatchManifests = gameManifest.Patches;
            
            RefreshExternalPatches();
        }
        finally
        {
            IsLoadingExternalPatches = false;
        }
    }

    public void RefreshExternalPatches()
    {
        if (!HasLoadedExternalPatches)
            return;

        ExternalPatches.DisposeAll();
        ExternalPatches.Clear();

        foreach (ExternalPatchManifest externalPatchManifest in ExternalPatchManifests)
        {
            // TODO: Ideally access to the local patches collection should be locked as it might be modified on another thread
            // Don't show external patches if they exist locally
            if (LocalPatches.Any(x => x.Manifest.ID == externalPatchManifest.ID))
                continue;

            // Add view model
            ExternalPatches.Add(new ExternalPatchViewModel(this, externalPatchManifest));
            
            // TODO-UPDATE: Load thumbnail async (also cache them somewhere)
        }
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
                        Library.AddPatch(patchViewModel.PatchFilePath, patchViewModel.Manifest.ID, patchViewModel.MovePatch);

                    // Remove removed patches
                    foreach (string patch in _removedPatches.Where(x => LocalPatches.All(p => p.Manifest.ID != x)))
                        Library.RemovePatch(patch);

                    _removedPatches.Clear();

                    // Get the current patch data
                    PatchManifest[] patchManifests = LocalPatches.Select(x => x.Manifest).ToArray();
                    string[] enabledPatches = LocalPatches.Where(x => x.IsEnabled).Select(x => x.Manifest.ID).ToArray();

                    await patcher.ApplyAsync(
                        game: Game,
                        library: Library,
                        patchHistory: HistoryManifest,
                        gameDirectory: GameDirectory,
                        patchManifests: patchManifests,
                        enabledPatches: enabledPatches,
                        progressCallback: operation.SetProgress);
                });

                Logger.Info("Applied patches");

                // TODO-UPDATE: Localize
                await Services.MessageUI.DisplaySuccessfulActionMessageAsync("Successfully applied all patches");
            }
            catch (Exception ex)
            {
                // TODO-UPDATE: Localize
                await Services.MessageUI.DisplayExceptionMessageAsync(ex,
                    "An error occurred when applying the patches. Some files might still have been modified by patches.");
            }
        }
    }

    public void Dispose()
    {
        LocalPatches.DisposeAll();
        ExternalPatches.DisposeAll();
    }

    #endregion
}