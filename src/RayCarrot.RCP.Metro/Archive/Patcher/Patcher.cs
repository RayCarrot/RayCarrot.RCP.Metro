using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NLog;

namespace RayCarrot.RCP.Metro.Archive;

/// <summary>
/// The service for patching an archive file and updating an Archive Patch Container
/// </summary>
public class Patcher
{
    #region Logger

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    #endregion

    #region Private Methods

    private Dictionary<string, FileModification> GetFileModifications(PatchContainerFile container, PatchHistoryManifest? patchHistory, IEnumerable<PatchManifest> enabledPatches)
    {
        Dictionary<string, FileModification> fileModifications = new();

        // If a patch history exists then we start by reverting the changes. If any of these changes shouldn't
        // actually be reverted then that will be overridden when we go through the patches to apply.
        if (patchHistory != null)
        {
            Logger.Info("Getting file modifications from history");

            string id = patchHistory.ID;

            // Remove added files
            if (patchHistory.AddedFiles != null)
                foreach (string addedFile in patchHistory.AddedFiles)
                {
                    fileModifications[PatchContainerFile.NormalizeResourceName(addedFile)] =
                        new FileModification(FileModificationType.Remove, id, addedFile, false);
                    Logger.Trace("File mod add -> remove: {0}", addedFile);
                }

            // Add back replaced files
            if (patchHistory.ReplacedFiles != null)
                foreach (string replacedFile in patchHistory.ReplacedFiles)
                {
                    fileModifications[PatchContainerFile.NormalizeResourceName(replacedFile)] =
                        new FileModification(FileModificationType.Add, id, replacedFile, false);
                    Logger.Trace("File mod replace -> add: {0}", replacedFile);
                }

            // Add back removed files
            if (patchHistory.RemovedFiles != null)
                foreach (string removedFile in patchHistory.RemovedFiles)
                {
                    fileModifications[PatchContainerFile.NormalizeResourceName(removedFile)] =
                        new FileModification(FileModificationType.Add, id, removedFile, false);
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
                    string addedFile = patch.AddedFiles[i];
                    string addedFileChecksum = patch.AddedFileChecksums[i];
                    fileModifications[PatchContainerFile.NormalizeResourceName(addedFile)] =
                        new FileModification(FileModificationType.Add, id, addedFile, true, addedFileChecksum);
                    Logger.Trace("File mod add: {0}", addedFile);
                }
            }
            else
            {
                Logger.Warn("Patch {0} added files array is null", id);
            }

            if (patch.RemovedFiles != null)
            {
                foreach (string removedFile in patch.RemovedFiles)
                {
                    fileModifications[PatchContainerFile.NormalizeResourceName(removedFile)] =
                        new FileModification(FileModificationType.Remove, id, removedFile, true);
                    Logger.Trace("File mod remove: {0}", removedFile);
                }
            }
            else
            {
                Logger.Warn("Patch {0} removed files array is null", id);
            }
        }

        Logger.Info("Got {0} file modifications", fileModifications.Count);

        return fileModifications;
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
            foreach (string addedFile in manifest.AddedFiles)
            {
                using Stream resourceStream = src.GetResource(addedFile, false);
                container.AddPatchResource(manifest.ID, addedFile, false, resourceStream);
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
        IArchiveDataManager manager, 
        PatchContainerFile container, 
        PatchHistoryManifest? patchHistory, 
        FileSystemPath archiveFilePath,
        PatchManifest[] patchManifests,
        string[] enabledPatches)
    {
        // TODO-UPDATE: In case of error the container will be corrupt. Perhaps we read the container as read-only and then when writing we copy to temp and write to that and then replace?

        Logger.Info("Applying patcher modifications with {0}/{1} enabled patches", enabledPatches.Length, patchManifests.Length);

        // The history gets re-created each time we save, so generate a new ID
        string newHistoryID = PatchFile.GenerateID(patchManifests.Select(x => x.ID).Append(patchHistory?.ID).ToArray());

        List<string> addedFiles = new();
        List<string> addedFileChecksums = new();
        List<string> replacedFiles = new();
        List<string> replacedFileChecksums = new();
        List<string> removedFiles = new();
        long totalSize = 0;

        // Local helper
        void saveRemovedFileInHistory(FileItem file, IDisposable generator, string filePath, string resourceName, string? replacedFileChecksum = null)
        {
            using ArchiveFileStream fileData = file.GetDecodedFileData(generator);

            if (replacedFileChecksum != null)
            {
                replacedFiles.Add(filePath);
                replacedFileChecksums.Add(replacedFileChecksum);
            }
            else
            {
                removedFiles.Add(filePath);
            }

            totalSize += fileData.Stream.Length;

            container.AddPatchResource(newHistoryID, resourceName, true, fileData.Stream);
            
            Logger.Trace("Saved removed file {0} in history", filePath);
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

                // The file modifications we want to perform
                Dictionary<string, FileModification> fileModifications = 
                    GetFileModifications(container, patchHistory, patchManifests.Where(x => enabledPatches.Contains(x.ID)));

                // The previously applied modifications
                string[]? prevAddedFiles = patchHistory?.AddedFiles?.Select(x => PatchContainerFile.NormalizeResourceName(x)).ToArray();
                string[]? prevReplacedFiles = patchHistory?.ReplacedFiles?.Select(x => PatchContainerFile.NormalizeResourceName(x)).ToArray();

                Logger.Info("Modifying archive");

                // Replace or remove existing files
                foreach (ArchiveDirectory dir in archiveData.Directories)
                {
                    foreach (FileItem file in dir.Files)
                    {
                        string filePath = manager.CombinePaths(file.Directory, file.FileName);
                        string resourceName = PatchContainerFile.NormalizeResourceName(filePath);

                        FileModification? modification = fileModifications.TryGetValue(resourceName);

                        if (modification is not null)
                            fileModifications.Remove(resourceName);

                        // Remove existing file
                        if (modification?.Type == FileModificationType.Remove)
                        {
                            Logger.Trace("Removing file {0}", filePath);

                            if (modification.AddToHistory)
                                saveRemovedFileInHistory(file, archiveData.Generator, filePath, resourceName);
                            continue;
                        }

                        archiveFiles.Add(file);

                        // Replace existing file
                        if (modification?.Type == FileModificationType.Add)
                        {
                            Logger.Trace("Replacing file {0}", filePath);

                            if (modification.AddToHistory)
                            {
                                string checksum = modification.Checksum ?? throw new Exception("Missing checksum");

                                // If the file was added previously we don't want to mark it as being replaced or else we'd 
                                // be replacing the previously added file
                                if (prevAddedFiles?.Any(x => x == resourceName) == true)
                                {
                                    addedFiles.Add(modification.FilePath);
                                    addedFileChecksums.Add(checksum);
                                }
                                // If the file was replaced previously we want to keep the originally removed file instead
                                // of the one it was replaced with before
                                else if (prevReplacedFiles?.Any(x => x == resourceName) == true)
                                {
                                    using Stream prevSavedFile = container.GetPatchResource(patchHistory!.ID, resourceName, true);

                                    replacedFiles.Add(filePath);
                                    replacedFileChecksums.Add(checksum);

                                    totalSize += prevSavedFile.Length;

                                    container.AddPatchResource(newHistoryID, resourceName, true, prevSavedFile);
                                }
                                else
                                {
                                    saveRemovedFileInHistory(file, archiveData.Generator, filePath, resourceName, checksum);
                                }
                            }

                            using Stream resource = container.GetPatchResource(modification.PatchID, resourceName, true);
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

                    Logger.Trace("Adding file {0}", filePath);

                    FileItem file = new(manager, fileName, dir, entry);

                    archiveFiles.Add(file);

                    using (Stream resource = container.GetPatchResource(modification.PatchID, filePath, false))
                        ReplaceArchiveFile(file, manager, resource);

                    if (modification.AddToHistory)
                    {
                        addedFiles.Add(filePath);
                        addedFileChecksums.Add(modification.Checksum ?? throw new Exception("Missing checksum"));
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
        Services.File.MoveFile(archiveOutputFile.TempPath, archiveFilePath, true);

        // Clear old history
        if (patchHistory != null)
            container.ClearPatchFiles(patchHistory.ID);

        // Create new history
        PatchHistoryManifest history = new(
            ID: newHistoryID,
            TotalSize: totalSize,
            ModifiedDate: DateTime.Now,
            AddedFiles: addedFiles.ToArray(),
            AddedFileChecksums: addedFileChecksums.ToArray(),
            ReplacedFiles: replacedFiles.ToArray(),
            ReplacedFileChecksums: replacedFileChecksums.ToArray(),
            RemovedFiles: removedFiles.ToArray());

        // Update the container manifest
        container.WriteManifest(history, patchManifests, enabledPatches);

        container.Apply();
    }

    #endregion

    #region Data Types

    private record FileModification(
        FileModificationType Type, 
        string PatchID, 
        string FilePath, 
        bool AddToHistory, 
        string? Checksum = null);

    private enum FileModificationType
    {
        Add,
        Remove,
    }

    #endregion
}