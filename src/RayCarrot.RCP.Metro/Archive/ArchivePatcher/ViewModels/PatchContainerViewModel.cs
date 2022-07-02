using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace RayCarrot.RCP.Metro.Archive;

public class PatchContainerViewModel : BaseViewModel, IDisposable
{
    public PatchContainerViewModel(PatchContainer container)
    {
        Container = container;
        DisplayName = container.ArchiveFilePath.Name;
        Patches = new ObservableCollection<PatchViewModel>();
        PatchedFiles = new ObservableCollection<PatchedFileViewModel>();
    }

    private readonly HashSet<PatchManifest> _removedPatches = new HashSet<PatchManifest>();

    public PatchContainer Container { get; }
    public string DisplayName { get; }

    public PatchHistoryManifest? PatchHistory { get; set; }
    public ObservableCollection<PatchViewModel> Patches { get; }
    public PatchViewModel? SelectedPatch { get; set; }

    public ObservableCollection<PatchedFileViewModel> PatchedFiles { get; set; }
    public bool HasPatchedFiles => PatchedFiles.Any();

    private Dictionary<string, FileModification> GetFileModifications()
    {
        Dictionary<string, FileModification> fileModifications = new();

        if (PatchHistory != null)
        {
            string id = PatchHistory.ID;

            foreach (string addedFile in PatchHistory.AddedFiles)
                fileModifications[Container.GetResourceName(addedFile)] = 
                    new FileModification(FileModificationType.Remove, id, addedFile, false);

            foreach (string replacedFile in PatchHistory.ReplacedFiles)
                fileModifications[Container.GetResourceName(replacedFile)] = 
                    new FileModification(FileModificationType.Add, id, replacedFile, false);

            foreach (string removedFile in PatchHistory.RemovedFiles)
                fileModifications[Container.GetResourceName(removedFile)] = 
                    new FileModification(FileModificationType.Add, id, removedFile, false);
        }

        foreach (PatchManifest patch in Patches.Where(x => x.IsEnabled).Select(x => x.Patch).Reverse())
        {
            string id = patch.ID;

            if (patch.AddedFiles != null && patch.AddedFileChecksums != null)
            {
                for (var i = 0; i < patch.AddedFiles.Length; i++)
                {
                    string addedFile = patch.AddedFiles[i];
                    string addedFileChecksum = patch.AddedFileChecksums[i];
                    fileModifications[Container.GetResourceName(addedFile)] =
                        new FileModification(FileModificationType.Add, id, addedFile, true, addedFileChecksum);
                }
            }

            if (patch.RemovedFiles != null)
                foreach (string removedFile in patch.RemovedFiles)
                    fileModifications[Container.GetResourceName(removedFile)] = 
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

        Container.AddResource(history.ID, resourceName, fileData.Stream);
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
        PatchContainerManifest? manifest = Container.ReadManifest();

        Patches.Clear();

        PatchHistory = manifest?.History;

        if (manifest is null)
            return;

        foreach (PatchManifest patch in manifest.Patches)
        {
            PatchViewModel patchVM = new(this, patch, manifest.EnabledPatches?.Contains(patch.ID) == true);

            // TODO: Load this async? Or maybe it's fast enough that it doesn't matter.
            patchVM.LoadThumbnail();

            Patches.Add(patchVM);
        }
    }

    public void RefreshPatchedFiles()
    {
        Dictionary<string, PatchedFileViewModel> files = new();

        foreach (PatchViewModel patchViewModel in Patches.Where(x => x.IsEnabled))
        {
            PatchManifest patch = patchViewModel.Patch;

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

    public void RemovePatch(PatchViewModel patch)
    {
        Patches.Remove(patch);
        _removedPatches.Add(patch.Patch);
        SelectedPatch = null;
        RefreshPatchedFiles();
    }

    public void Apply(IArchiveDataManager manager)
    {
        FileSystemPath archivePath = Container.ArchiveFilePath;
        using TempFile archiveOutputFile = new(true);

        // TODO-UPDATE: In case of error the container will be corrupt. Perhaps we read the container as read-only and then when writing we copy to temp and write to that and then replace?

        PatchHistoryManifest history = new(Container.GetNewPatchID(_removedPatches.Select(x => x.ID).Append(PatchHistory?.ID).ToArray()), Container.ContainerVersion);

        // Read the archive
        using (FileStream archiveStream = File.OpenRead(archivePath))
        {
            string archiveFileName = Container.ArchiveFilePath.Name;
            
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
                string[]? prevAddedFiles = PatchHistory?.AddedFiles.Select(x => Container.GetResourceName(x)).ToArray();
                string[]? prevReplacedFiles = PatchHistory?.ReplacedFiles.Select(x => Container.GetResourceName(x)).ToArray();

                // Replace or remove existing files
                foreach (ArchiveDirectory dir in archiveData.Directories)
                {
                    foreach (FileItem file in dir.Files)
                    {
                        string filePath = manager.CombinePaths(file.Directory, file.FileName);
                        string resourceName = Container.GetResourceName(filePath);

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
                                    using Stream prevSavedFile = Container.GetPatchResource(PatchHistory!.ID, resourceName);

                                    history.ReplacedFiles.Add(filePath);
                                    history.ReplacedFileChecksums.Add(checksum);

                                    history.TotalSize += prevSavedFile.Length;

                                    Container.AddResource(history.ID, resourceName, prevSavedFile);
                                }
                                else
                                {
                                    SaveRemovedFileInHistory(file, archiveData.Generator, filePath, resourceName, history, checksum);
                                }
                            }

                            using Stream resource = Container.GetPatchResource(modification.PatchID, resourceName);
                            ReplaceArchiveFile(file, manager, resource);
                        }
                    }
                }

                // Add files not already in the archive
                foreach (FileModification modification in fileModifications.Values.Where(x => x.Type == FileModificationType.Add))
                {
                    // TODO-UPDATE: Don't use System.IO here since a different separator char might be used
                    string filePath = modification.FilePath;
                    string resourceName = Container.GetResourceName(filePath);
                    string fileName = Path.GetFileName(filePath);
                    string dir = Path.GetDirectoryName(filePath) ?? String.Empty;
                    object entry = manager.GetNewFileEntry(archive, dir, fileName);

                    FileItem file = new(manager, fileName, dir, entry);

                    archiveFiles.Add(file);

                    using (Stream resource = Container.GetPatchResource(modification.PatchID, resourceName))
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
        Services.File.MoveFile(archiveOutputFile.TempPath, archivePath, true);

        // Update the patch manifests
        foreach (PatchViewModel patchViewModel in Patches)
        {
            //patchViewModel.Patch.ContainerVersion = Container.ContainerVersion;
        }

        if (PatchHistory != null)
            Container.ClearResources(PatchHistory.ID);

        // Update the container manifest
        string[] enabledPatches = Patches.Where(x => x.IsEnabled).Select(x => x.Patch.ID).ToArray();
        Container.WriteManifest(history, Patches.Select(x => x.Patch).ToArray(), enabledPatches);

        // Clear removed patches
        foreach (PatchManifest patch in _removedPatches.Where(x => Patches.All(p => p.Patch.ID != x.ID)))
            Container.ClearResources(patch.ID);
    }

    public void Dispose()
    {
        Container.Dispose();
    }

    private record FileModification(FileModificationType Type, string PatchID, string FilePath, bool AddToHistory, string? Checksum = null);

    private enum FileModificationType
    {
        Add,
        Remove,
    }
}