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

    private Dictionary<string, FileModification> GetFileModifications()
    {
        Dictionary<string, FileModification> fileModifications = new();

        if (PatchHistory != null)
        {
            string id = PatchHistory.ID;

            foreach (string addedFile in PatchHistory.AddedFiles)
                fileModifications[Container.NormalizeResourceName(addedFile)] = 
                    new FileModification(FileModificationType.Remove, id, addedFile, false);

            foreach (string replacedFile in PatchHistory.ReplacedFiles)
                fileModifications[Container.NormalizeResourceName(replacedFile)] = 
                    new FileModification(FileModificationType.Add, id, replacedFile, false);

            foreach (string removedFile in PatchHistory.RemovedFiles)
                fileModifications[Container.NormalizeResourceName(removedFile)] = 
                    new FileModification(FileModificationType.Add, id, removedFile, false);
        }

        foreach (PatchManifest patch in Patches.Where(x => x.IsEnabled).Select(x => x.Manifest).Reverse())
        {
            string id = patch.ID;

            if (patch.AddedFiles != null && patch.AddedFileChecksums != null)
            {
                for (var i = 0; i < patch.AddedFiles.Length; i++)
                {
                    string addedFile = patch.AddedFiles[i];
                    string addedFileChecksum = patch.AddedFileChecksums[i];
                    fileModifications[Container.NormalizeResourceName(addedFile)] =
                        new FileModification(FileModificationType.Add, id, addedFile, true, addedFileChecksum);
                }
            }

            if (patch.RemovedFiles != null)
                foreach (string removedFile in patch.RemovedFiles)
                    fileModifications[Container.NormalizeResourceName(removedFile)] = 
                        new FileModification(FileModificationType.Remove, id, removedFile, true);
        }

        return fileModifications;
    }

    private void SaveRemovedFileInHistory(FileItem file, IDisposable generator, string filePath, string resourceName, PatchHistoryManifest history, string? replacedFileChecksum = null)
    {
        using ArchiveFileStream fileData = file.GetDecodedFileData(generator);

        if (replacedFileChecksum != null)
        {
            history.ReplacedFiles.Add(filePath);
            history.ReplacedFileChecksums.Add(replacedFileChecksum);
        }
        else
        {
            history.RemovedFiles.Add(filePath);
        }
        history.TotalSize += fileData.Stream.Length;

        Container.AddPatchResource(history.ID, resourceName, true, fileData.Stream);
    }

    private void ReplaceArchiveFile(FileItem file, IArchiveDataManager manager, Stream resource)
    {
        // Get the temp stream to store the pending import data
        file.SetPendingImport();

        // Encode the data to the pending import stream
        manager.EncodeFile(resource, file.PendingImport, file.ArchiveEntry);

        // If no data was encoded we copy over the decoded data
        if (file.PendingImport.Length == 0)
            resource.CopyTo(file.PendingImport);
    }

    public void LoadExistingPatches()
    {
        PatchContainerManifest? containerManifest = Container.ReadManifest();

        Patches.Clear();

        PatchHistory = containerManifest?.History;

        if (containerManifest is null)
            return;

        foreach (PatchManifest patch in containerManifest.Patches)
        {
            PatchContainerDataSource src = new(Container, patch.ID, true);
            PatchViewModel patchVM = new(this, patch, containerManifest.EnabledPatches?.Contains(patch.ID) == true, src);

            // TODO: Load this async? Or maybe it's fast enough that it doesn't matter.
            patchVM.LoadThumbnail(Container);

            Patches.Add(patchVM);
        }
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

                PatchManifest? manifest = patch.ReadManifest();

                if (manifest == null)
                    throw new Exception("Patch file does not contain a valid manifest");

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

            PatchManifest? manifest = patch.ReadManifest();

            if (manifest == null)
                throw new Exception("Patch file does not contain a valid manifest");

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
        using TempFile archiveOutputFile = new(true);

        // TODO-UPDATE: In case of error the container will be corrupt. Perhaps we read the container as read-only and then when writing we copy to temp and write to that and then replace?

        // Add files to the container for patches which do not currently exist in the container
        foreach (PatchViewModel patchViewModel in Patches.Where(x => x.DataSource is not PatchContainerDataSource))
        {
            PatchManifest manifest = patchViewModel.Manifest;
            IPatchDataSource src = patchViewModel.DataSource;

            // Clear any leftover files before importing
            Container.ClearPatchFiles(manifest.ID);

            // Import the resources
            if (manifest.AddedFiles != null)
            {
                foreach (string addedFile in manifest.AddedFiles)
                {
                    using Stream resourceStream = src.GetResource(addedFile, false);
                    Container.AddPatchResource(manifest.ID, addedFile, false, resourceStream);
                }
            }

            // Import the assets
            if (manifest.Assets != null)
            {
                foreach (string asset in manifest.Assets)
                {
                    using Stream assetStream = src.GetAsset(asset);
                    Container.AddPatchAsset(manifest.ID, asset, assetStream);
                }
            }
        }

        // Clear removed patches
        foreach (string patch in _removedPatches.Where(x => Patches.All(p => p.Manifest.ID != x)))
            Container.ClearPatchFiles(patch);

        _removedPatches.Clear();

        // The history gets re-created each time we save, so generate a new ID
        string newHistoryID = PatchFile.GenerateID(Patches.Select(x => x.Manifest.ID).Append(PatchHistory?.ID).ToArray());
        PatchHistoryManifest history = new(newHistoryID, PatchContainerFile.Version);

        // Read the archive
        using (FileStream archiveStream = File.OpenRead(ArchiveFilePath))
        {
            string archiveFileName = ArchiveFilePath.Name;
            
            object archive = manager.LoadArchive(archiveStream, archiveFileName);

            ArchiveData? archiveData = null;

            // Files to be repacked
            List<FileItem> archiveFiles = new();

            try
            {
                archiveData = manager.LoadArchiveData(archive, archiveStream, archiveFileName);

                // The file modifications we want to perform
                Dictionary<string, FileModification> fileModifications = GetFileModifications();

                // The previously applied modifications
                string[]? prevAddedFiles = PatchHistory?.AddedFiles.Select(x => Container.NormalizeResourceName(x)).ToArray();
                string[]? prevReplacedFiles = PatchHistory?.ReplacedFiles.Select(x => Container.NormalizeResourceName(x)).ToArray();

                // Replace or remove existing files
                foreach (ArchiveDirectory dir in archiveData.Directories)
                {
                    foreach (FileItem file in dir.Files)
                    {
                        string filePath = manager.CombinePaths(file.Directory, file.FileName);
                        string resourceName = Container.NormalizeResourceName(filePath);

                        FileModification? modification = fileModifications.TryGetValue(resourceName);

                        if (modification is not null)
                            fileModifications.Remove(resourceName);

                        // Remove existing file
                        if (modification?.Type == FileModificationType.Remove)
                        {
                            if (modification.AddToHistory)
                                SaveRemovedFileInHistory(file, archiveData.Generator, filePath, resourceName, history);
                            continue;
                        }

                        archiveFiles.Add(file);

                        // Replace existing file
                        if (modification?.Type == FileModificationType.Add)
                        {
                            if (modification.AddToHistory)
                            {
                                string checksum = modification.Checksum ?? throw new Exception("Missing checksum");

                                // If the file was added previously we don't want to mark it as being replaced or else we'd 
                                // be replacing the previously added file
                                if (prevAddedFiles?.Any(x => x == resourceName) == true)
                                {
                                    history.AddedFiles.Add(modification.FilePath);
                                    history.AddedFileChecksums.Add(checksum);
                                }
                                // If the file was replaced previously we want to keep the originally removed file instead
                                // of the one it was replaced with before
                                else if (prevReplacedFiles?.Any(x => x == resourceName) == true)
                                {
                                    using Stream prevSavedFile = Container.GetPatchResource(PatchHistory!.ID, resourceName, true);

                                    history.ReplacedFiles.Add(filePath);
                                    history.ReplacedFileChecksums.Add(checksum);

                                    history.TotalSize += prevSavedFile.Length;

                                    Container.AddPatchResource(history.ID, resourceName, true, prevSavedFile);
                                }
                                else
                                {
                                    SaveRemovedFileInHistory(file, archiveData.Generator, filePath, resourceName, history, checksum);
                                }
                            }

                            using Stream resource = Container.GetPatchResource(modification.PatchID, resourceName, true);
                            ReplaceArchiveFile(file, manager, resource);
                        }
                    }
                }

                // Add files not already in the archive
                foreach (FileModification modification in fileModifications.Values.Where(x => x.Type == FileModificationType.Add))
                {
                    // TODO-UPDATE: Don't use System.IO here since a different separator char might be used
                    string filePath = modification.FilePath;
                    string fileName = Path.GetFileName(filePath);
                    string dir = Path.GetDirectoryName(filePath) ?? String.Empty;
                    object entry = manager.GetNewFileEntry(archive, dir, fileName);

                    FileItem file = new(manager, fileName, dir, entry);

                    archiveFiles.Add(file);

                    using (Stream resource = Container.GetPatchResource(modification.PatchID, filePath, false))
                        ReplaceArchiveFile(file, manager, resource);

                    if (modification.AddToHistory)
                    {
                        history.AddedFiles.Add(filePath);
                        history.AddedFileChecksums.Add(modification.Checksum ?? throw new Exception("Missing checksum"));
                    }
                }

                using ArchiveFileStream archiveOutputStream = new(File.OpenWrite(archiveOutputFile.TempPath),
                    archiveOutputFile.TempPath.Name, true);

                manager.WriteArchive(archiveData.Generator, archive, archiveOutputStream, archiveFiles, _ => { });
            }
            finally
            {
                archiveData?.Generator.Dispose();
                archiveFiles.DisposeAll();
            }
        }

        // Replace the archive with the modified one
        Services.File.MoveFile(archiveOutputFile.TempPath, ArchiveFilePath, true);

        // Clear old history
        if (PatchHistory != null)
            Container.ClearPatchFiles(PatchHistory.ID);

        // Get the current patch data
        PatchManifest[] patchManifests = Patches.Select(
            // Update each patch manifest to the latest version
            x => x.Manifest with { PatchVersion = PatchFile.Version }).ToArray();
        string[] enabledPatches = Patches.Where(x => x.IsEnabled).Select(x => x.Manifest.ID).ToArray();
        
        // Update the container manifest
        Container.WriteManifest(history, patchManifests, enabledPatches);

        Container.Apply();
    }

    public void Dispose()
    {
        Patches.DisposeAll();
        Container.Dispose();
    }

    private record FileModification(FileModificationType Type, string PatchID, string FilePath, bool AddToHistory, string? Checksum = null);

    private enum FileModificationType
    {
        Add,
        Remove,
    }
}