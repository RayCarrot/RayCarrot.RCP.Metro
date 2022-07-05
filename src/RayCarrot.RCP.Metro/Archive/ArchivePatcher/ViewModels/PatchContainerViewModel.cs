using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace RayCarrot.RCP.Metro.Archive;

// TODO-UPDATE: Log

public class PatchContainerViewModel : BaseViewModel, IDisposable
{
    public PatchContainerViewModel(PatchContainerFile container, FileSystemPath archiveFilePath, BindableOperation loadOperation)
    {
        Container = container;
        ArchiveFilePath = archiveFilePath;
        LoadOperation = loadOperation;
        DisplayName = archiveFilePath.Name;
        Patches = new ObservableCollection<PatchViewModel>();
        PatchedFiles = new ObservableCollection<PatchedFileViewModel>();

        AddPatchCommand = new AsyncRelayCommand(AddPatchAsync);
    }

    private readonly HashSet<string> _removedPatches = new();

    public ICommand AddPatchCommand { get; }

    public PatchContainerFile Container { get; }
    public FileSystemPath ArchiveFilePath { get; }
    public string DisplayName { get; }
    public BindableOperation LoadOperation { get; }

    public PatchHistoryManifest? PatchHistory { get; set; }
    public ObservableCollection<PatchViewModel> Patches { get; }
    public PatchViewModel? SelectedPatch { get; set; }

    public ObservableCollection<PatchedFileViewModel> PatchedFiles { get; set; }
    public bool HasPatchedFiles => PatchedFiles.Any();

    private void AddPatch(PatchFile patchFile, PatchManifest manifest)
    {
        PatchViewModel patchViewModel = new(this, manifest, false, new PatchFileDataSource(patchFile, false));
        patchViewModel.LoadThumbnail(null);
        Patches.Add(patchViewModel);
    }

    public async Task<bool> LoadExistingPatchesAsync()
    {
        PatchContainerManifest? containerManifest = Container.ReadManifest();

        Patches.Clear();

        if (containerManifest is { ContainerVersion: > PatchContainerFile.Version })
        {
            await Services.MessageUI.DisplayMessageAsync("The archive patch container was made with a newer version of the Rayman Control Panel and can thus not be read", MessageType.Error);
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
            patchVM.LoadThumbnail(Container);

            Patches.Add(patchVM);
        }

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

        foreach (FileSystemPath patchFile in result.SelectedFiles)
        {
            PatchFile? patch = null;

            try
            {
                patch = new PatchFile(patchFile, true);

                PatchManifest manifest = patch.ReadManifest();

                if (manifest.PatchVersion > PatchFile.Version)
                {
                    await Services.MessageUI.DisplayMessageAsync("The selected patch was made with a newer version of the Rayman Control Panel and can thus not be read", MessageType.Error);

                    patch.Dispose();
                    continue;
                }

                PatchViewModel? conflict = Patches.FirstOrDefault(x => x.Manifest.ID == manifest.ID);

                if (conflict != null)
                {
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
                // TODO-UPDATE: Log exception

                patch?.Dispose();

                // TODO-UPDATE: Localize
                await Services.MessageUI.DisplayExceptionMessageAsync(ex, "An error occurred when adding the patch");
            }
        }
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
                Extensions = new FileFilterItem("*.ap", "Archive Patch").StringRepresentation,
            });

            if (browseResult.CanceledByUser)
                return;

            PatchManifest manifest = patchViewModel.Manifest;
            IPatchDataSource src = patchViewModel.DataSource;

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

            // TODO-UPDATE: Localize
            await Services.MessageUI.DisplaySuccessfulActionMessageAsync("The patch was successfully exported");
        }
    }

    public void RemovePatch(PatchViewModel patchViewModel)
    {
        Patches.Remove(patchViewModel);
        _removedPatches.Add(patchViewModel.Manifest.ID);

        if (SelectedPatch == patchViewModel)
            SelectedPatch = null;

        // If the patch was enabled we need to refresh the patched files
        if (patchViewModel.IsEnabled)
            RefreshPatchedFiles();

        patchViewModel.Dispose();
    }

    public async Task UpdatePatchAsync(PatchViewModel patchViewModel)
    {
        FileBrowserResult result = await Services.BrowseUI.BrowseFileAsync(new FileBrowserViewModel
        {
            // TODO-UPDATE: Localize
            Title = "Select updated patch",
            DefaultDirectory = default,
            DefaultName = null,
            ExtensionFilter = new FileExtension(".ap").GetFileFilterItem.StringRepresentation,
        });

        if (result.CanceledByUser)
            return;

        PatchFile? patch = null;

        try
        {
            patch = new PatchFile(result.SelectedFile, true);

            PatchManifest manifest = patch.ReadManifest();

            if (manifest.PatchVersion > PatchFile.Version)
            {
                await Services.MessageUI.DisplayMessageAsync("The selected patch was made with a newer version of the Rayman Control Panel and can thus not be read", MessageType.Error);

                patch.Dispose();
                return;
            }

            // Verify the ID
            if (patchViewModel.Manifest.ID != manifest.ID)
            {
                // TODO-UPDATE: Localize
                await Services.MessageUI.DisplayMessageAsync($"The selected patch does not match and can not be used to update {patchViewModel.Manifest.Name}.", MessageType.Error);

                patch.Dispose();
                return;
            }

            // Verify the revision is newer
            if (patchViewModel.Manifest.Revision > manifest.Revision)
            {
                // TODO-UPDATE: Localize
                await Services.MessageUI.DisplayMessageAsync($"The selected patch revision is lower or the same as the current revision and can not be used to update {patchViewModel.Manifest.Name}.", MessageType.Error);

                patch.Dispose();
                return;
            }

            // Remove the current patch
            RemovePatch(patchViewModel);

            // Add the updated patch
            AddPatch(patch, manifest);
        }
        catch (Exception ex)
        {
            // TODO-UPDATE: Log exception

            patch?.Dispose();

            // TODO-UPDATE: Localize
            await Services.MessageUI.DisplayExceptionMessageAsync(ex, "An error occurred when updating the patch");
        }
    }

    public void Apply(IArchiveDataManager manager)
    {
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

        patcher.Apply(manager, Container, PatchHistory, ArchiveFilePath, patchManifests, enabledPatches);
    }

    public void Dispose()
    {
        Patches.DisposeAll();
        Container.Dispose();
    }
}