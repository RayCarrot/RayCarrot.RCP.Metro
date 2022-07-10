using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        ContainerFilePath = PatchContainerFile.GetContainerFilePath(GameDirectory);

        Patches = new ObservableCollection<PatchViewModel>();
        PatchedFileLocations = new ObservableCollection<PatchedFileLocationViewModel>();

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

    #endregion

    #region Public Properties

    public Games Game { get; }
    public FileSystemPath GameDirectory { get; }
    public FileSystemPath ContainerFilePath { get; }
    
    public PatchHistoryManifest? PatchHistory { get; set; }

    public ObservableCollection<PatchViewModel> Patches { get; }
    public PatchViewModel? SelectedPatch { get; set; }

    public ObservableCollection<PatchedFileLocationViewModel> PatchedFileLocations { get; set; }
    
    public BindableOperation LoadOperation { get; }
    public bool HasPatchedFiles => PatchedFileLocations.Any();
    public bool HasChanges { get; set; }

    #endregion

    #region Private Methods

    private void AddPatch(PatchFile patchFile, PatchManifest manifest)
    {
        PatchViewModel patchViewModel = new(this, manifest, false, new PatchFileDataSource(patchFile, false));
        patchViewModel.LoadThumbnail();
        Patches.Add(patchViewModel);
        HasChanges = true;

        Logger.Info("Added patch '{0}' with revision {1} and ID {2}", manifest.Name, manifest.Revision, manifest.ID);
    }

    #endregion

    #region Public Methods

    public async Task<bool> LoadExistingPatchesAsync()
    {
        Logger.Info("Loading existing patches");

        using PatchContainerFile container = new(ContainerFilePath, readOnly: true);

        PatchContainerManifest? containerManifest = container.ReadManifest();

        Patches.Clear();

        // Verify version
        if (containerManifest is { ContainerVersion: > PatchContainerFile.Version })
        {
            Logger.Warn("Failed to load container due to the version number {0} being higher than the current one ({1})",
                containerManifest.ContainerVersion, PatchContainerFile.Version);

            // TODO-UPDATE: Localize
            await Services.MessageUI.DisplayMessageAsync("The game patch container was made with a newer version of the Rayman Control Panel and can thus not be read", MessageType.Error);
            PatchHistory = null;

            return false;
        }

        // Verify game
        if (containerManifest != null && containerManifest.Game != Game)
        {
            Logger.Warn("Failed to load container due to the game {0} not matching the current one ({1})",
                containerManifest.Game, Game);

            // TODO-UPDATE: Localize
            await Services.MessageUI.DisplayMessageAsync($"The game patch container was made with for {Game}", MessageType.Error);
            PatchHistory = null;

            return false;
        }

        PatchHistory = containerManifest?.History;

        if (containerManifest is null)
            return true;

        foreach (PatchManifest patch in containerManifest.Patches)
        {
            PatchContainerDataSource src = new(container, patch.ID, true);
            PatchViewModel patchVM = new(this, patch, containerManifest.EnabledPatches?.Contains(patch.ID) == true, src);

            // TODO: Load this async? Or maybe it's fast enough that it doesn't matter.
            patchVM.LoadThumbnail();

            Patches.Add(patchVM);
        }

        Logger.Info("Loaded {0} patches", containerManifest.Patches.Length);

        return true;
    }

    public void RefreshPatchedFiles()
    {
        Dictionary<string, Dictionary<string, PatchedFileViewModel>> files = new();

        foreach (PatchViewModel patchViewModel in Patches.Where(x => x.IsEnabled))
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
                string locationKey = fileName.Location.ToLowerInvariant().Replace('\\', '/');
                string filePathKey = fileName.FilePath.ToLowerInvariant().Replace('\\', '/');

                if (!files.ContainsKey(locationKey))
                    files.Add(locationKey, new Dictionary<string, PatchedFileViewModel>());

                if (files[locationKey].ContainsKey(filePathKey))
                    files[locationKey][filePathKey].OverridenPatches.Add(patch);
                else
                    files[locationKey].Add(filePathKey, new PatchedFileViewModel(fileName, modification, patch));
            }
        }

        var locations = files.Values.Select(x =>
            new PatchedFileLocationViewModel(x.First().Value.FilePath.Location, x.Values.OrderBy(f => f.FilePath.FilePath)));
        PatchedFileLocations = new ObservableCollection<PatchedFileLocationViewModel>(locations.OrderBy(x => x.Location));
        OnPropertyChanged(nameof(HasPatchedFiles));

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
            ExtensionFilter = new FileExtension(PatchFile.FileExtension).GetFileFilterItem.StringRepresentation,
            MultiSelection = true
        });

        if (result.CanceledByUser)
            return;

        Logger.Info("Adding {0} patches", result.SelectedFiles.Length);

        foreach (FileSystemPath patchFile in result.SelectedFiles)
        {
            Logger.Trace("Adding patch from {0}", patchFile);

            PatchFile? patch = null;

            try
            {
                patch = new PatchFile(patchFile, true);

                PatchManifest manifest = patch.ReadManifest();

                // Verify version
                if (manifest.PatchVersion > PatchFile.Version)
                {
                    Logger.Warn("Failed to add patch due to the version number {0} being higher than the current one ({1})",
                        manifest.PatchVersion, PatchFile.Version);

                    // TODO-UPDATE: Localize
                    await Services.MessageUI.DisplayMessageAsync("The selected patch was made with a newer version of the Rayman Control Panel and can thus not be read", MessageType.Error);

                    patch.Dispose();
                    continue;
                }

                // Verify game, if locked to one
                if (manifest.Game != null && manifest.Game != Game)
                {
                    Logger.Warn("Failed to add patch due to the specified game {0} not matching the current one ({1})",
                        manifest.Game, Game);

                    // TODO-UPDATE: Localize
                    await Services.MessageUI.DisplayMessageAsync($"The selected patch can only be applied to {manifest.Game}", MessageType.Error);

                    patch.Dispose();
                    continue;
                }

                PatchViewModel? conflict = Patches.FirstOrDefault(x => x.Manifest.ID == manifest.ID);

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
                AddPatch(patch, manifest);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Adding patch");

                patch?.Dispose();

                // TODO-UPDATE: Localize
                await Services.MessageUI.DisplayExceptionMessageAsync(ex, "An error occurred when adding the patch");
            }
        }

        Logger.Info("Added patches");
    }

    public async Task ExtractPatchContentsAsync(PatchViewModel patchViewModel)
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
            IPatchDataSource src = patchViewModel.DataSource;

            Logger.Info("Extracting patch contents");

            // TODO-UPDATE: Try/catch
            // Extract resources
            if (manifest.AddedFiles != null)
            {
                int fileIndex = 0;

                foreach (PatchFilePath addedFile in manifest.AddedFiles)
                {
                    operation.SetProgress(new Progress(fileIndex, manifest.AddedFiles.Length));
                    fileIndex++;

                    FileSystemPath fileDest = result.SelectedDirectory + addedFile.FullFilePath;
                    Directory.CreateDirectory(fileDest.Parent);

                    using FileStream dstStream = File.Create(fileDest);
                    using Stream srcStream = src.GetResource(addedFile);

                    await srcStream.CopyToAsync(dstStream);
                }

                operation.SetProgress(new Progress(fileIndex, manifest.AddedFiles.Length));
            }

            Logger.Info("Extracted patch contents");

            // TODO-UPDATE: Localize
            await Services.MessageUI.DisplaySuccessfulActionMessageAsync("The patch contents were successfully extracted");
        }
    }

    public async Task ExportPatchAsync(PatchViewModel patchViewModel)
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

            PatchManifest manifest = patchViewModel.Manifest;
            IPatchDataSource src = patchViewModel.DataSource;

            Logger.Info("Exporting patch from container");

            // TODO-UPDATE: Try/catch
            await Task.Run(() =>
            {
                // Create a new patch
                using PatchFile patchFile = new(browseResult.SelectedFileLocation);

                // Copy resources
                if (manifest.AddedFiles != null)
                {
                    foreach (PatchFilePath addedFile in manifest.AddedFiles)
                    {
                        using Stream srcStream = src.GetResource(addedFile);
                        patchFile.AddPatchResource(addedFile, srcStream);
                    }
                }

                // Copy assets
                if (manifest.Assets != null)
                {
                    foreach (string asset in manifest.Assets)
                    {
                        using Stream srcStream = src.GetAsset(asset);
                        patchFile.AddPatchAsset(asset, srcStream);
                    }
                }

                // Write the manifest
                patchFile.WriteManifest(manifest);

                patchFile.Apply();
            });

            Logger.Info("Exported patch");

            // TODO-UPDATE: Localize
            await Services.MessageUI.DisplaySuccessfulActionMessageAsync("The patch was successfully exported");
        }
    }

    public void RemovePatch(PatchViewModel patchViewModel)
    {
        PatchManifest manifest = patchViewModel.Manifest;

        Patches.Remove(patchViewModel);
        _removedPatches.Add(manifest.ID);

        if (SelectedPatch == patchViewModel)
            SelectedPatch = null;

        // If the patch was enabled we need to refresh the patched files
        if (patchViewModel.IsEnabled)
            RefreshPatchedFiles();

        patchViewModel.Dispose();
        HasChanges = true;

        Logger.Info("Removed patch '{0}' with revision {1} and ID {2}", manifest.Name, manifest.Revision, manifest.ID);
    }

    public async Task UpdatePatchAsync(PatchViewModel patchViewModel)
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

        PatchFile? patch = null;

        Logger.Info("Updating patch '{0}' with revision {1} and ID {2}",
            patchViewModel.Manifest.Name, patchViewModel.Manifest.Revision, patchViewModel.Manifest.ID);

        try
        {
            patch = new PatchFile(result.SelectedFile, true);

            PatchManifest manifest = patch.ReadManifest();

            // Verify version
            if (manifest.PatchVersion > PatchFile.Version)
            {
                Logger.Warn("Failed to update patch due to the version number {0} being higher than the current one ({1})",
                    manifest.PatchVersion, PatchFile.Version);

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
            RemovePatch(patchViewModel);

            // Add the updated patch
            AddPatch(patch, manifest);

            Logger.Info("Updated patch to revision {0}", manifest.Revision);
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Updated patch with ID {0}", patchViewModel.Manifest.ID);

            patch?.Dispose();

            // TODO-UPDATE: Localize
            await Services.MessageUI.DisplayExceptionMessageAsync(ex, "An error occurred when updating the patch");
        }
    }

    public async Task<bool> LoadPatchesAsync()
    {
        Logger.Info("Loading patch containers");

        try
        {
            bool success = await LoadExistingPatchesAsync();

            if (!success)
            {
                Logger.Warn("Failed to load patch container for game {0}", Game);
                return false;
            }

            RefreshPatchedFiles();

            Logger.Info("Loaded patch container for game {0}", Game);

            return true;
        }
        catch (Exception ex)
        {
            // TODO-UPDATE: Log exception

            // TODO-UPDATE: Localize
            await Services.MessageUI.DisplayExceptionMessageAsync(ex, "An error occurred when loading the patches");

            return false;
        }
    }

    public async Task ApplyAsync()
    {
        // TODO-UPDATE: Set progress?
        // TODO-UPDATE: Localize
        using (await LoadOperation.RunAsync("Applying patches"))
        {
            Logger.Info("Applying patches");

            try
            {
                if (HasChanges)
                {
                    await Task.Run(() =>
                    {
                        // TODO-UPDATE: In case of error we should copy existing container to temp, open the temp one here and replace
                        //              actual container with temp once it succeeds. This avoids corrupting it in case of error. Though
                        //              on the other hand if it fails then some patches might be half applied...
                        // Open the container for writing
                        using PatchContainerFile container = new(ContainerFilePath);

                        // Create a patcher
                        Patcher patcher = new(); // TODO: Use DI?

                        // Add files to the container for patches which do not currently exist in the container
                        foreach (PatchViewModel patchViewModel in Patches.Where(x => x.DataSource is not PatchContainerDataSource))
                            patcher.AddPatchFiles(container, patchViewModel.Manifest, patchViewModel.DataSource);

                        // Clear removed patches
                        foreach (string patch in _removedPatches.Where(x => Patches.All(p => p.Manifest.ID != x)))
                            container.ClearPatchFiles(patch);

                        _removedPatches.Clear();

                        // Get the current patch data
                        PatchManifest[] patchManifests = Patches.Select(x => x.Manifest).ToArray();
                        string[] enabledPatches = Patches.Where(x => x.IsEnabled).Select(x => x.Manifest.ID).ToArray();

                        patcher.Apply(
                            game: Game,
                            container: container, 
                            archiveDataManager: Game.GetGameInfo().GetArchiveDataManager, 
                            patchHistory: PatchHistory, 
                            gameDirectory: GameDirectory, 
                            patchManifests: patchManifests, 
                            enabledPatches: enabledPatches);
                    });
                }

                // TODO-UPDATE: Do we actually still want to auto-sync textures?
                //await Manager.OnRepackedArchivesAsync(Containers.Where(x => x.HasChanges).Select(x => x.ArchiveFilePath).ToArray());

                Logger.Info("Applied patches");

                // TODO-UPDATE: Localize
                await Services.MessageUI.DisplaySuccessfulActionMessageAsync("Successfully applied all patches");
            }
            catch (Exception ex)
            {
                // TODO-UPDATE: Localize
                await Services.MessageUI.DisplayExceptionMessageAsync(ex,
                    "An error occurred when applying the patches. Some patches might still have been applied.");
            }
        }
    }

    public void Dispose()
    {
        Patches.DisposeAll();
    }

    #endregion
}