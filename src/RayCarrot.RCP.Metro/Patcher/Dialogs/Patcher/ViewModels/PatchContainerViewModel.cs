using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using NLog;

namespace RayCarrot.RCP.Metro.Patcher;

// TODO-UPDATE: Merge with PatcherViewModel
public class PatchContainerViewModel : BaseViewModel, IDisposable
{
    #region Constructor

    public PatchContainerViewModel(Games game, FileSystemPath gameDirectory, BindableOperation loadOperation)
    {
        ContainerFilePath = PatchContainerFile.GetContainerFilePath(gameDirectory);
        Container = new PatchContainerFile(ContainerFilePath, readOnly: true);

        Game = game;
        GameDirectory = gameDirectory;
        LoadOperation = loadOperation;
        Patches = new ObservableCollection<PatchViewModel>();
        PatchedFiles = new ObservableCollection<PatchedFileViewModel>();

        AddPatchCommand = new AsyncRelayCommand(AddPatchAsync);
    }

    #endregion

    #region Logger

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    #endregion

    #region Private Fields

    private readonly HashSet<string> _removedPatches = new();

    #endregion

    #region Commands

    public ICommand AddPatchCommand { get; }

    #endregion

    #region Public Properties

    public PatchContainerFile Container { get; }
    public Games Game { get; }
    public FileSystemPath GameDirectory { get; }
    public FileSystemPath ContainerFilePath { get; }
    public BindableOperation LoadOperation { get; }

    public PatchHistoryManifest? PatchHistory { get; set; }
    public ObservableCollection<PatchViewModel> Patches { get; }
    public PatchViewModel? SelectedPatch { get; set; }

    public ObservableCollection<PatchedFileViewModel> PatchedFiles { get; set; }
    public bool HasPatchedFiles => PatchedFiles.Any();
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

        PatchContainerManifest ? containerManifest = Container.ReadManifest();

        Patches.Clear();

        if (containerManifest is { ContainerVersion: > PatchContainerFile.Version })
        {
            Logger.Warn("Failed to load container due to the version number {0} being higher than the current one ({1})",
                containerManifest.ContainerVersion, PatchContainerFile.Version);

            // TODO-UPDATE: Localize
            await Services.MessageUI.DisplayMessageAsync("The game patch container was made with a newer version of the Rayman Control Panel and can thus not be read", MessageType.Error);
            PatchHistory = null;

            return false;
        }

        PatchHistory = containerManifest?.History;

        if (containerManifest is null)
            return true;

        foreach (PatchManifest patch in containerManifest.Patches)
        {
            PatchContainerDataSource src = new(Container, patch.ID, true);
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
        Dictionary<string, PatchedFileViewModel> files = new();

        foreach (PatchViewModel patchViewModel in Patches.Where(x => x.IsEnabled))
        {
            PatchManifest patch = patchViewModel.Manifest;

            if (patch.AddedFiles != null)
                foreach (string addedFile in patch.AddedFiles)
                    addFile(addedFile, PatchedFileViewModel.PatchedFileModification.Add);

            if (patch.RemovedFiles != null)
                foreach (string removedFile in patch.RemovedFiles)
                    addFile(removedFile, PatchedFileViewModel.PatchedFileModification.Remove);

            void addFile(string fileName, PatchedFileViewModel.PatchedFileModification modification)
            {
                string lowerCaseFile = fileName.ToLowerInvariant();

                if (files.ContainsKey(lowerCaseFile))
                    files[lowerCaseFile].OverridenPatches.Add(patch);
                else
                    files.Add(lowerCaseFile, new PatchedFileViewModel(fileName, modification, patch));
            }
        }

        PatchedFiles = new ObservableCollection<PatchedFileViewModel>(files.Values.OrderBy(x => x.FilePath));
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
            ExtensionFilter = new FileExtension(".ap").GetFileFilterItem.StringRepresentation,
            MultiSelection = true
        });

        if (result.CanceledByUser)
            return;

        Logger.Info("Adding {0} patches", result.SelectedFiles.Length);

        foreach (FileSystemPath patchFile in result.SelectedFiles)
        {
            Logger.Trace("Adding patch frm {0}", patchFile);

            PatchFile? patch = null;

            try
            {
                patch = new PatchFile(patchFile, true);

                PatchManifest manifest = patch.ReadManifest();

                if (manifest.PatchVersion > PatchFile.Version)
                {
                    Logger.Warn("Failed to add patch due to the version number {0} being higher than the current one ({1})",
                        manifest.PatchVersion, PatchFile.Version);

                    // TODO-UPDATE: Localize
                    await Services.MessageUI.DisplayMessageAsync("The selected patch was made with a newer version of the Rayman Control Panel and can thus not be read", MessageType.Error);

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

                foreach (string addedFile in manifest.AddedFiles)
                {
                    operation.SetProgress(new Progress(fileIndex, manifest.AddedFiles.Length));
                    fileIndex++;

                    FileSystemPath fileDest = result.SelectedDirectory + addedFile;
                    Directory.CreateDirectory(fileDest.Parent);

                    using FileStream dstStream = File.Create(fileDest);
                    using Stream srcStream = src.GetResource(addedFile, false);

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
                    foreach (string addedFile in manifest.AddedFiles)
                    {
                        using Stream srcStream = src.GetResource(addedFile, false);
                        patchFile.AddPatchResource(addedFile, false, srcStream);
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

    public void Apply()
    {
        Logger.Info("Applying patches for container for game {0}", Game);

        // Create a patcher
        Patcher patcher = new(); // TODO: Use DI?

        // Add files to the container for patches which do not currently exist in the container
        foreach (PatchViewModel patchViewModel in Patches.Where(x => x.DataSource is not PatchContainerDataSource))
            patcher.AddPatchFiles(Container, patchViewModel.Manifest, patchViewModel.DataSource);

        // Clear removed patches
        foreach (string patch in _removedPatches.Where(x => Patches.All(p => p.Manifest.ID != x)))
            Container.ClearPatchFiles(patch);

        _removedPatches.Clear();

        // Get the current patch data
        PatchManifest[] patchManifests = Patches.Select(x => x.Manifest).ToArray();
        string[] enabledPatches = Patches.Where(x => x.IsEnabled).Select(x => x.Manifest.ID).ToArray();

        patcher.Apply(Container, PatchHistory, GameDirectory, ContainerFilePath, patchManifests, enabledPatches);
    }

    public void Dispose()
    {
        Patches.DisposeAll();
        Container.Dispose();
    }

    #endregion
}