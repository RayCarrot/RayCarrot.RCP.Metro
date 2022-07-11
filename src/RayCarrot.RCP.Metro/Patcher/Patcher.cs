using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NLog;
using RayCarrot.RCP.Metro.Archive;

namespace RayCarrot.RCP.Metro.Patcher;

/// <summary>
/// The service for patching game files and updating a patch container
/// </summary>
public class Patcher
{
    #region Logger

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    #endregion

    #region Private Methods

    private string NormalizePath(string path) => path.ToLowerInvariant().Replace('\\', '/');

    private Dictionary<string, LocationModifications> GetFileModificationsPerLocation(PatchHistoryManifest? patchHistory, IEnumerable<PatchManifest> enabledPatches)
    {
        Dictionary<string, LocationModifications> locationModifications = new();

        void addModification(
            PatchFilePath patchFilePath, 
            FileModificationType type,
            string patchID,
            bool addToHistory,
            string? checksum = null)
        {
            string locationKey = NormalizePath(patchFilePath.Location);
            string filePathKey = NormalizePath(patchFilePath.FilePath);

            if (!locationModifications.ContainsKey(locationKey))
                locationModifications.Add(locationKey, new LocationModifications(patchFilePath.Location));

            locationModifications[locationKey].FileModifications[filePathKey] = new FileModification(type, patchID, patchFilePath, addToHistory, checksum);
        }

        // If a patch history exists then we start by reverting the changes. If any of these changes shouldn't
        // actually be reverted then that will be overridden when we go through the patches to apply.
        if (patchHistory != null)
        {
            Logger.Info("Getting file modifications from history");

            string id = patchHistory.ID;

            // Remove added files
            if (patchHistory.AddedFiles != null)
                foreach (PatchFilePath addedFile in patchHistory.AddedFiles)
                {
                    addModification(addedFile, FileModificationType.Remove, id, false);
                    Logger.Trace("File mod add -> remove: {0}", addedFile);
                }

            // Add back replaced files
            if (patchHistory.ReplacedFiles != null)
                foreach (PatchFilePath replacedFile in patchHistory.ReplacedFiles)
                {
                    addModification(replacedFile, FileModificationType.Add, id, false);
                    Logger.Trace("File mod replace -> add: {0}", replacedFile);
                }

            // Add back removed files
            if (patchHistory.RemovedFiles != null)
                foreach (PatchFilePath removedFile in patchHistory.RemovedFiles)
                {
                    addModification(removedFile, FileModificationType.Add, id, false);
                    Logger.Trace("File mod remove -> add: {0}", removedFile);
                }
        }

        Logger.Info("Getting file modifications from enabled patches");

        // Add modifications for each enabled patch. Reverse the order for the correct patch priority.
        foreach (PatchManifest patch in enabledPatches.Reverse())
        {
            string id = patch.ID;

            if (patch.AddedFiles != null && patch.AddedFileChecksums != null)
            {
                for (var i = 0; i < patch.AddedFiles.Length; i++)
                {
                    PatchFilePath addedFile = patch.AddedFiles[i];
                    string addedFileChecksum = patch.AddedFileChecksums[i];
                    addModification(addedFile, FileModificationType.Add, id, true, addedFileChecksum);
                    Logger.Trace("File mod add: {0}", addedFile);
                }
            }
            else
            {
                Logger.Warn("Patch {0} added files array is null", id);
            }

            if (patch.RemovedFiles != null)
            {
                foreach (PatchFilePath removedFile in patch.RemovedFiles)
                {
                    addModification(removedFile, FileModificationType.Remove, id, true);
                    Logger.Trace("File mod remove: {0}", removedFile);
                }
            }
            else
            {
                Logger.Warn("Patch {0} removed files array is null", id);
            }
        }

        Logger.Info("Got {0} file modifications", locationModifications.Count);

        return locationModifications;
    }

    private void ModifyPhysicalFiles(
        PatchContainerFile container,
        PatchFileChanges fileChanges,
        string locationKey,
        Dictionary<string, FileModification> fileModifications,
        FileSystemPath dirPath,
        PatchHistoryManifest? patchHistory,
        Action<Progress>? progressCallback)
    {
        // The previously applied modifications for this location
        string[]? prevAddedFiles = patchHistory?.AddedFiles?.
            Where(x => NormalizePath(x.Location) == locationKey).
            Select(x => NormalizePath(x.FilePath)).
            ToArray();
        string[]? prevReplacedFiles = patchHistory?.ReplacedFiles?.
            Where(x => NormalizePath(x.Location) == locationKey).
            Select(x => NormalizePath(x.FilePath)).
            ToArray();

        int fileIndex = 0;

        // Process each file modification
        foreach (var fileModification in fileModifications)
        {
            FileModification modification = fileModification.Value;
            string filePathKey = fileModification.Key;
            PatchFilePath patchFilePath = modification.PatchFilePath;
            FileSystemPath physicalFilePath = dirPath + patchFilePath.FilePath;

            if (modification.Type == FileModificationType.Add)
            {
                if (physicalFilePath.FileExists)
                {
                    Logger.Trace("Replacing file {0}", patchFilePath);

                    if (modification.AddToHistory)
                    {
                        string checksum = modification.Checksum ?? throw new Exception("Missing checksum");

                        // If the file was added previously we don't want to mark it as being replaced or else we'd 
                        // be replacing the previously added file
                        if (prevAddedFiles?.Contains(filePathKey) == true)
                        {
                            fileChanges.AddedFiles.Add(patchFilePath);
                            fileChanges.AddedFileChecksums.Add(checksum);
                        }
                        // If the file was replaced previously we want to keep the originally removed file instead
                        // of the one it was replaced with before
                        else if (prevReplacedFiles?.Contains(filePathKey) == true)
                        {
                            using Stream prevSavedFile = container.GetPatchResource(patchHistory!.ID, patchFilePath);

                            fileChanges.ReplacedFiles.Add(patchFilePath);
                            fileChanges.ReplacedFileChecksums.Add(checksum);

                            fileChanges.TotalSize += prevSavedFile.Length;

                            container.AddPatchResource(fileChanges.NewHistoryID, patchFilePath, prevSavedFile);
                        }
                        else
                        {
                            SaveRemovedPhysicalFileInHistory(container, fileChanges, physicalFilePath, patchFilePath, checksum);
                        }
                    }

                    // Replace the file
                    using Stream resource = container.GetPatchResource(modification.PatchID, patchFilePath);
                    ReplacePhysicalFile(physicalFilePath, resource);
                }
                else
                {
                    Logger.Trace("Adding file {0}", patchFilePath);

                    // Replace the file
                    using (Stream resource = container.GetPatchResource(modification.PatchID, patchFilePath))
                        ReplacePhysicalFile(physicalFilePath, resource);

                    if (modification.AddToHistory)
                    {
                        fileChanges.AddedFiles.Add(patchFilePath);
                        fileChanges.AddedFileChecksums.Add(modification.Checksum ?? throw new Exception("Missing checksum"));
                    }
                }
            }
            else if (modification.Type == FileModificationType.Remove)
            {
                Logger.Trace("Removing file {0}", patchFilePath);

                if (modification.AddToHistory)
                    SaveRemovedPhysicalFileInHistory(container, fileChanges, physicalFilePath, patchFilePath);

                // Delete the file
                physicalFilePath.DeleteFile();
            }

            fileIndex++;
            progressCallback?.Invoke(new Progress(fileIndex, fileModifications.Count));
        }
    }

    private void ModifyArchive(
        PatchContainerFile container, 
        PatchFileChanges fileChanges, 
        string locationKey,
        Dictionary<string, FileModification> fileModifications, 
        FileSystemPath archiveFilePath, 
        IArchiveDataManager manager, 
        PatchHistoryManifest? patchHistory,
        Action<Progress>? progressCallback)
    {
        if (!archiveFilePath.FileExists)
        {
            // TODO-UPDATE: What do we do here? Log warning and ignore?
        }

        using TempFile archiveOutputFile = new(true);

        // Read the archive
        using (FileStream archiveStream = File.OpenRead(archiveFilePath))
        {
            string archiveFileName = archiveFilePath.Name;
            object archive = manager.LoadArchive(archiveStream, archiveFileName);

            ArchiveData? archiveData = null;

            // Files to be repacked
            List<FileItem> archiveFiles = new();

            try
            {
                archiveData = manager.LoadArchiveData(archive, archiveStream, archiveFileName);

                // The previously applied modifications for this location
                string[]? prevAddedFiles = patchHistory?.AddedFiles?.
                    Where(x => NormalizePath(x.Location) == locationKey).
                    Select(x => NormalizePath(x.FilePath)).
                    ToArray();
                string[]? prevReplacedFiles = patchHistory?.ReplacedFiles?.
                    Where(x => NormalizePath(x.Location) == locationKey).
                    Select(x => NormalizePath(x.FilePath)).
                    ToArray();

                Logger.Info("Modifying archive");

                // Replace or remove existing files
                foreach (ArchiveDirectory dir in archiveData.Directories)
                {
                    foreach (FileItem file in dir.Files)
                    {
                        string filePathKey = NormalizePath(manager.CombinePaths(file.Directory, file.FileName));

                        FileModification? modification = fileModifications.TryGetValue(filePathKey);

                        if (modification is not null)
                            fileModifications.Remove(filePathKey);

                        // Remove existing file
                        if (modification?.Type == FileModificationType.Remove)
                        {
                            PatchFilePath patchFilePath = modification.PatchFilePath;

                            Logger.Trace("Removing file {0}", patchFilePath);

                            if (modification.AddToHistory)
                                SaveRemovedArchiveFileInHistory(container, fileChanges, file, archiveData.Generator, patchFilePath);
                            continue;
                        }

                        archiveFiles.Add(file);

                        // Replace existing file
                        if (modification?.Type == FileModificationType.Add)
                        {
                            PatchFilePath patchFilePath = modification.PatchFilePath;

                            Logger.Trace("Replacing file {0}", patchFilePath);

                            if (modification.AddToHistory)
                            {
                                string checksum = modification.Checksum ?? throw new Exception("Missing checksum");

                                // If the file was added previously we don't want to mark it as being replaced or else we'd 
                                // be replacing the previously added file
                                if (prevAddedFiles?.Contains(filePathKey) == true)
                                {
                                    fileChanges.AddedFiles.Add(patchFilePath);
                                    fileChanges.AddedFileChecksums.Add(checksum);
                                }
                                // If the file was replaced previously we want to keep the originally removed file instead
                                // of the one it was replaced with before
                                else if (prevReplacedFiles?.Contains(filePathKey) == true)
                                {
                                    using Stream prevSavedFile = container.GetPatchResource(patchHistory!.ID, patchFilePath);

                                    fileChanges.ReplacedFiles.Add(patchFilePath);
                                    fileChanges.ReplacedFileChecksums.Add(checksum);

                                    fileChanges.TotalSize += prevSavedFile.Length;

                                    container.AddPatchResource(fileChanges.NewHistoryID, patchFilePath, prevSavedFile);
                                }
                                else
                                {
                                    SaveRemovedArchiveFileInHistory(container, fileChanges, file, archiveData.Generator, patchFilePath, checksum);
                                }
                            }

                            using Stream resource = container.GetPatchResource(modification.PatchID, patchFilePath);
                            ReplaceArchiveFile(file, manager, resource);
                        }
                    }
                }

                // Add files not already in the archive
                foreach (FileModification modification in fileModifications.Values.Where(x => x.Type == FileModificationType.Add))
                {
                    string fileName = Path.GetFileName(modification.PatchFilePath.FilePath);
                    string dir = Path.GetDirectoryName(modification.PatchFilePath.FilePath)?.
                        Replace(Path.DirectorySeparatorChar, manager.PathSeparatorCharacter) ?? String.Empty;

                    PatchFilePath patchFilePath = modification.PatchFilePath;
                    object entry = manager.GetNewFileEntry(archive, dir, fileName);

                    Logger.Trace("Adding file {0}", patchFilePath);

                    FileItem file = new(manager, fileName, dir, entry);

                    archiveFiles.Add(file);

                    using (Stream resource = container.GetPatchResource(modification.PatchID, patchFilePath))
                        ReplaceArchiveFile(file, manager, resource);

                    if (modification.AddToHistory)
                    {
                        fileChanges.AddedFiles.Add(patchFilePath);
                        fileChanges.AddedFileChecksums.Add(modification.Checksum ?? throw new Exception("Missing checksum"));
                    }
                }

                using ArchiveFileStream archiveOutputStream = new(File.OpenWrite(archiveOutputFile.TempPath),
                    archiveOutputFile.TempPath.Name, true);

                manager.WriteArchive(archiveData.Generator, archive, archiveOutputStream, archiveFiles, progressCallback ?? (_ => { }));
            }
            finally
            {
                archiveData?.Generator.Dispose();
                archiveFiles.DisposeAll();
            }
        }

        // Replace the archive with the modified one
        Services.File.MoveFile(archiveOutputFile.TempPath, archiveFilePath, true);
    }

    private void SaveRemovedFileInHistory(
        PatchContainerFile container, 
        PatchFileChanges fileChanges, 
        Stream fileStream, 
        PatchFilePath patchFilePath, 
        string? replacedFileChecksum = null)
    {
        if (replacedFileChecksum != null)
        {
            fileChanges.ReplacedFiles.Add(patchFilePath);
            fileChanges.ReplacedFileChecksums.Add(replacedFileChecksum);
        }
        else
        {
            fileChanges.RemovedFiles.Add(patchFilePath);
        }

        fileChanges.TotalSize += fileStream.Length;

        container.AddPatchResource(fileChanges.NewHistoryID, patchFilePath, fileStream);

        Logger.Trace("Saved removed file {0} in history", patchFilePath);
    }

    private void SaveRemovedPhysicalFileInHistory(
        PatchContainerFile container,
        PatchFileChanges fileChanges,
        FileSystemPath filePath,
        PatchFilePath patchFilePath,
        string? replacedFileChecksum = null)
    {
        using Stream fileStream = File.OpenRead(filePath);
        SaveRemovedFileInHistory(container, fileChanges, fileStream, patchFilePath, replacedFileChecksum);
    }

    private void SaveRemovedArchiveFileInHistory(
        PatchContainerFile container, 
        PatchFileChanges fileChanges, 
        FileItem file, 
        IDisposable generator,
        PatchFilePath patchFilePath, 
        string? replacedFileChecksum = null)
    {
        using ArchiveFileStream fileData = file.GetDecodedFileData(generator);
        SaveRemovedFileInHistory(container, fileChanges, fileData.Stream, patchFilePath, replacedFileChecksum);
    }

    private void ReplacePhysicalFile(FileSystemPath filePath, Stream resource)
    {
        Directory.CreateDirectory(filePath.Parent);
        using Stream fileStream = File.OpenWrite(filePath);
        fileStream.SetLength(0);
        resource.CopyTo(fileStream);
    }

    private void ReplaceArchiveFile(FileItem file, IArchiveDataManager manager, Stream resource)
    {
        Logger.Trace("Replacing archive file {0}/{1}", file.Directory, file.FileName);

        // Get the temp stream to store the pending import data
        file.SetPendingImport();

        // Encode the data to the pending import stream
        manager.EncodeFile(resource, file.PendingImport, file.ArchiveEntry);

        // If no data was encoded we copy over the decoded data
        if (file.PendingImport.Length == 0)
            resource.CopyTo(file.PendingImport);
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Adds the files to the container for a new patch not previously in the container
    /// </summary>
    /// <param name="container">The container to add the patch files to</param>
    /// <param name="manifest">The patch manifest</param>
    /// <param name="src">The patch data source</param>
    public void AddPatchFiles(PatchContainerFile container, PatchManifest manifest, IPatchDataSource src)
    {
        Logger.Info("Adding patch files to container for patch {0} with ID {1}", manifest.Name, manifest.ID);

        // Clear any leftover files before importing
        container.ClearPatchFiles(manifest.ID);

        // Import the resources
        if (manifest.AddedFiles != null)
        {
            foreach (PatchFilePath addedFile in manifest.AddedFiles)
            {
                using Stream resourceStream = src.GetResource(addedFile);
                container.AddPatchResource(manifest.ID, addedFile, resourceStream);
            }
        }

        // Import the assets
        if (manifest.Assets != null)
        {
            foreach (string asset in manifest.Assets)
            {
                using Stream assetStream = src.GetAsset(asset);
                container.AddPatchAsset(manifest.ID, asset, assetStream);
            }
        }
    }

    public void Apply(
        Games game,
        PatchContainerFile container, 
        IArchiveDataManager archiveDataManager,
        PatchHistoryManifest? patchHistory, 
        FileSystemPath gameDirectory,
        PatchManifest[] patchManifests,
        string[] enabledPatches,
        Action<Progress>? progressCallback = null)
    {
        Logger.Info("Applying patcher modifications with {0}/{1} enabled patches", enabledPatches.Length, patchManifests.Length);

        // The history gets re-created each time we save, so generate a new ID
        string newHistoryID = PatchFile.GenerateID(patchManifests.Select(x => x.ID).Append(patchHistory?.ID).ToArray());

        // Keep track of file changes
        PatchFileChanges fileChanges = new(newHistoryID);

        // Get the file modifications for each location
        Dictionary<string, LocationModifications> locationModifications =
            GetFileModificationsPerLocation(patchHistory, patchManifests.Where(x => enabledPatches.Contains(x.ID)));

        // Progress:
        // 00-80%  -> Modifying files
        // 80-100% -> Applying changes (repacking the container)
        // Sadly we can't get any actual progress for the last step, so even though it's slower
        // than the first it'll be better to show it like this
        progressCallback?.Invoke(new Progress(0, 100));

        // Modify every location
        foreach (string locationKey in locationModifications.Keys)
        {
            // Physical
            if (locationKey == String.Empty)
            {
                ModifyPhysicalFiles(
                    container: container,
                    fileChanges: fileChanges,
                    locationKey: locationKey,
                    fileModifications: locationModifications[locationKey].FileModifications,
                    dirPath: gameDirectory,
                    patchHistory: patchHistory,
                    progressCallback: progressCallback == null ? null : x => progressCallback(new Progress(x.Percentage * 0.8d, 100)));
            }
            // Archive
            else
            {
                ModifyArchive(
                    container: container, 
                    fileChanges: fileChanges, 
                    locationKey: locationKey,
                    fileModifications: locationModifications[locationKey].FileModifications, 
                    archiveFilePath: gameDirectory + locationModifications[locationKey].Location, 
                    manager: archiveDataManager, 
                    patchHistory: patchHistory,
                    progressCallback: progressCallback == null ? null : x => progressCallback(new Progress(x.Percentage * 0.8d, 100)));
            }
        }

        // Clear old history
        if (patchHistory != null)
            container.ClearPatchFiles(patchHistory.ID);

        // Create new history
        PatchHistoryManifest history = new(
            ID: newHistoryID,
            TotalSize: fileChanges.TotalSize,
            ModifiedDate: DateTime.Now,
            AddedFiles: fileChanges.AddedFiles.ToArray(),
            AddedFileChecksums: fileChanges.AddedFileChecksums.ToArray(),
            ReplacedFiles: fileChanges.ReplacedFiles.ToArray(),
            ReplacedFileChecksums: fileChanges.ReplacedFileChecksums.ToArray(),
            RemovedFiles: fileChanges.RemovedFiles.ToArray());

        // Update the container manifest
        container.WriteManifest(game, history, patchManifests, enabledPatches);

        progressCallback?.Invoke(new Progress(80, 100));

        // Apply changes
        container.Apply();

        progressCallback?.Invoke(new Progress(100, 100));
    }

    #endregion

    #region Data Types

    private record FileModification(
        FileModificationType Type, 
        string PatchID,
        PatchFilePath PatchFilePath,
        bool AddToHistory, 
        string? Checksum = null);

    private class LocationModifications
    {
        public LocationModifications(string location)
        {
            Location = location;
        }

        public string Location { get; }
        public Dictionary<string, FileModification> FileModifications { get; } = new();
    }

    private enum FileModificationType
    {
        Add,
        Remove,
    }

    private class PatchFileChanges
    {
        public PatchFileChanges(string newHistoryId)
        {
            NewHistoryID = newHistoryId;
        }

        public string NewHistoryID { get; }
        public List<PatchFilePath> AddedFiles { get; } = new();
        public List<string> AddedFileChecksums { get; } = new();
        public List<PatchFilePath> ReplacedFiles { get; } = new();
        public List<string> ReplacedFileChecksums { get; } = new();
        public List<PatchFilePath> RemovedFiles { get; } = new();
        public long TotalSize { get; set; }
    }

    #endregion
}