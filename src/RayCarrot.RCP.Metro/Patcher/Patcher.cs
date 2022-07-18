using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BinarySerializer;
using NLog;
using RayCarrot.RCP.Metro.Archive;

namespace RayCarrot.RCP.Metro.Patcher;

/// <summary>
/// The service for patching game files and updating a patch library
/// </summary>
public class Patcher
{
    #region Logger

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    #endregion

    #region Private Methods

    private string NormalizePath(string path) => path.ToLowerInvariant().Replace('\\', '/');

    private Dictionary<string, LocationModifications> GetFileModificationsPerLocation(Games game, PatchHistoryManifest? patchHistory, IEnumerable<PatchFile> enabledPatches)
    {
        Dictionary<string, LocationModifications> locationModifications = new();
        Dictionary<string, IArchiveDataManager> archiveDataManagers = new();

        void addModification(
            PatchFilePath patchFilePath, 
            FileModificationType type,
            string? patchID, // Null if from history instead of a patch
            bool addToHistory,
            PatchFileResourceEntry? resourceEntry = null)
        {
            string locationKey = NormalizePath(patchFilePath.Location);
            string filePathKey = NormalizePath(patchFilePath.FilePath);

            if (!locationModifications.ContainsKey(locationKey))
            {
                IArchiveDataManager? manager = null;

                if (patchFilePath.Location != String.Empty && patchFilePath.LocationID != String.Empty)
                {
                    if (!archiveDataManagers.ContainsKey(patchFilePath.LocationID))
                    {
                        // NOTE: In the future we'll want to use the location ID to get the corresponding
                        //       manager. This makes it so we can have one game support multiple archive
                        //       formats. But for now it doesn't matter, so we just get the default one.
                        archiveDataManagers.Add(patchFilePath.LocationID, game.GetGameInfo().GetArchiveDataManager);
                    }

                    manager = archiveDataManagers[patchFilePath.LocationID];
                }

                locationModifications.Add(locationKey, new LocationModifications(patchFilePath.Location, manager));
            }

            locationModifications[locationKey].FileModifications[filePathKey] = new FileModification(type, patchID, patchFilePath, addToHistory, resourceEntry);
        }

        // If a patch history exists then we start by reverting the changes. If any of these changes shouldn't
        // actually be reverted then that will be overridden when we go through the patches to apply.
        if (patchHistory != null)
        {
            Logger.Info("Getting file modifications from history");

            // Remove added files
            if (patchHistory.AddedFiles != null)
                foreach (PatchFilePath addedFile in patchHistory.AddedFiles)
                {
                    addModification(addedFile, FileModificationType.Remove, null, false);
                    Logger.Trace("File mod add -> remove: {0}", addedFile);
                }

            // Add back replaced files
            if (patchHistory.ReplacedFiles != null)
                foreach (PatchFilePath replacedFile in patchHistory.ReplacedFiles)
                {
                    addModification(replacedFile, FileModificationType.Add, null, false);
                    Logger.Trace("File mod replace -> add: {0}", replacedFile);
                }

            // Add back removed files
            if (patchHistory.RemovedFiles != null)
                foreach (PatchFilePath removedFile in patchHistory.RemovedFiles)
                {
                    addModification(removedFile, FileModificationType.Add, null, false);
                    Logger.Trace("File mod remove -> add: {0}", removedFile);
                }
        }

        Logger.Info("Getting file modifications from enabled patches");

        // Add modifications for each enabled patch. Reverse the order for the correct patch priority.
        foreach (PatchFile patch in enabledPatches.Reverse())
        {
            string id = patch.Metadata.ID;

            foreach (PatchFileResourceEntry addedFile in patch.AddedFiles)
            {
                addModification(addedFile.FilePath, FileModificationType.Add, id, true, addedFile);
                Logger.Trace("File mod add: {0}", addedFile);
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
                        PatchFileResourceEntry resourceEntry = modification.ResourceEntry ?? throw new Exception("Missing resource entry");

                        // If the file was added previously we don't want to mark it as being replaced or else we'd 
                        // be replacing the previously added file
                        if (prevAddedFiles?.Contains(filePathKey) == true)
                        {
                            fileChanges.AddedFiles.Add(patchFilePath);
                            fileChanges.AddedFileChecksums.Add(resourceEntry.Checksum);
                        }
                        // If the file was replaced previously we want to keep the originally removed file instead
                        // of the one it was replaced with before
                        else if (prevReplacedFiles?.Contains(filePathKey) == true)
                        {
                            fileChanges.ReplacedFiles.Add(patchFilePath);
                            fileChanges.ReplacedFileChecksums.Add(resourceEntry.Checksum);

                            fileChanges.KeepHistoryResource(patchFilePath);
                        }
                        else
                        {
                            SaveRemovedPhysicalFileInHistory(fileChanges, physicalFilePath, patchFilePath, resourceEntry.Checksum);
                        }
                    }

                    // Replace the file
                    using Stream resource = fileChanges.GetPatchResource(modification);
                    ReplacePhysicalFile(physicalFilePath, resource);
                }
                else
                {
                    Logger.Trace("Adding file {0}", patchFilePath);

                    // Replace the file
                    using (Stream resource = fileChanges.GetPatchResource(modification))
                        ReplacePhysicalFile(physicalFilePath, resource);

                    if (modification.AddToHistory)
                    {
                        PatchFileResourceEntry resourceEntry = modification.ResourceEntry ?? throw new Exception("Missing resource entry");

                        fileChanges.AddedFiles.Add(patchFilePath);
                        fileChanges.AddedFileChecksums.Add(resourceEntry.Checksum);
                    }
                }
            }
            else if (modification.Type == FileModificationType.Remove)
            {
                Logger.Trace("Removing file {0}", patchFilePath);

                if (modification.AddToHistory)
                    SaveRemovedPhysicalFileInHistory(fileChanges, physicalFilePath, patchFilePath);

                // Delete the file
                physicalFilePath.DeleteFile();
            }

            fileIndex++;
            progressCallback?.Invoke(new Progress(fileIndex, fileModifications.Count));
        }
    }

    private void ModifyArchive(
        PatchFileChanges fileChanges, 
        string locationKey,
        Dictionary<string, FileModification> fileModifications, 
        FileSystemPath archiveFilePath, 
        IArchiveDataManager manager, 
        PatchHistoryManifest? patchHistory,
        Action<Progress>? progressCallback)
    {
        // TODO: Instead of ignoring maybe we should create a new archive?
        if (!archiveFilePath.FileExists)
        {
            Logger.Warn("Archive {0} does not exist and its file modifications will be ignored", archiveFilePath);
            return;
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

                Stopwatch sw = Stopwatch.StartNew();

                int totalFilesMax = archiveData.Directories.Sum(x => x.Files.Length) + fileModifications.Values.Count(x => x.Type == FileModificationType.Add);
                totalFilesMax *= 2; // 0-50%
                int totalFilesIndex = 0;

                // Replace or remove existing files
                foreach (ArchiveDirectory dir in archiveData.Directories)
                {
                    foreach (FileItem file in dir.Files)
                    {
                        progressCallback?.Invoke(new Progress(totalFilesIndex, totalFilesMax));
                        totalFilesIndex++;

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
                                SaveRemovedArchiveFileInHistory(fileChanges, file, archiveData.Generator, patchFilePath);
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
                                PatchFileResourceEntry resourceEntry = modification.ResourceEntry ?? throw new Exception("Missing resource entry");
                                byte[] checksum = resourceEntry.Checksum;

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
                                    fileChanges.ReplacedFiles.Add(patchFilePath);
                                    fileChanges.ReplacedFileChecksums.Add(checksum);

                                    fileChanges.KeepHistoryResource(patchFilePath);
                                }
                                else
                                {
                                    SaveRemovedArchiveFileInHistory(fileChanges, file, archiveData.Generator, patchFilePath, checksum);
                                }
                            }

                            using Stream resource = fileChanges.GetPatchResource(modification);
                            ReplaceArchiveFile(file, manager, resource);
                        }
                    }
                }

                // Add files not already in the archive
                foreach (FileModification modification in fileModifications.Values.Where(x => x.Type == FileModificationType.Add))
                {
                    progressCallback?.Invoke(new Progress(totalFilesIndex, totalFilesMax));
                    totalFilesIndex++;

                    string fileName = Path.GetFileName(modification.PatchFilePath.FilePath);
                    string dir = Path.GetDirectoryName(modification.PatchFilePath.FilePath)?.
                        Replace(Path.DirectorySeparatorChar, manager.PathSeparatorCharacter) ?? String.Empty;

                    PatchFilePath patchFilePath = modification.PatchFilePath;
                    object entry = manager.GetNewFileEntry(archive, dir, fileName);

                    Logger.Trace("Adding file {0}", patchFilePath);

                    FileItem file = new(manager, fileName, dir, entry);

                    archiveFiles.Add(file);

                    using (Stream resource = fileChanges.GetPatchResource(modification))
                        ReplaceArchiveFile(file, manager, resource);

                    if (modification.AddToHistory)
                    {
                        PatchFileResourceEntry resourceEntry = modification.ResourceEntry ?? throw new Exception("Missing resource entry");

                        fileChanges.AddedFiles.Add(patchFilePath);
                        fileChanges.AddedFileChecksums.Add(resourceEntry.Checksum);
                    }
                }

                sw.Stop();

                Logger.Info($"Processed modified archive files in {sw.ElapsedMilliseconds} ms");

                // 50%
                progressCallback?.Invoke(new Progress(50, 100));

                sw.Restart();

                using ArchiveFileStream archiveOutputStream = new(File.OpenWrite(archiveOutputFile.TempPath),
                    archiveOutputFile.TempPath.Name, true);

                manager.WriteArchive(archiveData.Generator, archive, archiveOutputStream, archiveFiles, progressCallback: 
                    x => progressCallback?.Invoke(new Progress(x.Percentage * 0.5 + 50, 100))); // 50-100%

                sw.Stop();

                Logger.Info($"Repacked modified archive files in {sw.ElapsedMilliseconds} ms");
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
        PatchFileChanges fileChanges, 
        Stream fileStream, 
        PatchFilePath patchFilePath,
        byte[]? replacedFileChecksum = null)
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

        fileChanges.AddHistoryResource(patchFilePath, fileStream);

        Logger.Trace("Saved removed file {0} in history", patchFilePath);
    }

    private void SaveRemovedPhysicalFileInHistory(
        PatchFileChanges fileChanges,
        FileSystemPath filePath,
        PatchFilePath patchFilePath,
        byte[]? replacedFileChecksum = null)
    {
        using Stream fileStream = File.OpenRead(filePath);
        SaveRemovedFileInHistory(fileChanges, fileStream, patchFilePath, replacedFileChecksum);
    }

    private void SaveRemovedArchiveFileInHistory(
        PatchFileChanges fileChanges, 
        FileItem file, 
        IDisposable generator,
        PatchFilePath patchFilePath,
        byte[]? replacedFileChecksum = null)
    {
        using ArchiveFileStream fileData = file.GetDecodedFileData(generator);
        SaveRemovedFileInHistory(fileChanges, fileData.Stream, patchFilePath, replacedFileChecksum);
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

    // TODO-UPDATE: Error handling to avoid corruption

    public async Task ApplyAsync(
        Games game,
        PatchLibrary library,
        PatchHistoryManifest? patchHistory, 
        FileSystemPath gameDirectory,
        string[] patches,
        string[] enabledPatches,
        Action<Progress>? progressCallback = null)
    {
        Logger.Info("Applying patcher modifications with {0}/{1} enabled patches", enabledPatches.Length, patches.Length);

        using RCPContext context = new(library.DirectoryPath);

        foreach (string patchID in patches)
            context.AddFile(new LinearFile(context, library.GetPatchFileName(patchID)));

        // Read the patch files
        PatchFile[] patchFiles = patches.Select(x => FileFactory.Read<PatchFile>(context, library.GetPatchFileName(x))).ToArray();

        // Keep track of file changes
        using PatchFileChanges fileChanges = new(context, library, patchFiles);

        // Get the file modifications for each location
        Dictionary<string, LocationModifications> locationModifications =
            GetFileModificationsPerLocation(game, patchHistory, patchFiles.Where(x => enabledPatches.Contains(x.Metadata.ID)));

        // Progress:
        // 00-90%  -> Modifying files
        // 90-100% -> Applying library/history changes
        progressCallback?.Invoke(new Progress(0, 100));

        int locationIndex = 0;

        // Modify every location
        foreach (string locationKey in locationModifications.Keys)
        {
            Action<Progress>? locationProgressCallback = progressCallback == null
                ? null
                : x => progressCallback(new Progress(x.Percentage * 0.9 * ((locationIndex + 1) / locationModifications.Keys.Count), 100));

            // Physical
            if (locationKey == String.Empty)
            {
                ModifyPhysicalFiles(
                    fileChanges: fileChanges,
                    locationKey: locationKey,
                    fileModifications: locationModifications[locationKey].FileModifications,
                    dirPath: gameDirectory,
                    patchHistory: patchHistory,
                    progressCallback: locationProgressCallback);
            }
            // Archive
            else
            {
                IArchiveDataManager manager = locationModifications[locationKey].ArchiveDataManager ??
                                              throw new Exception($"No archive data manager for location {locationKey}");

                ModifyArchive(
                    fileChanges: fileChanges, 
                    locationKey: locationKey,
                    fileModifications: locationModifications[locationKey].FileModifications, 
                    archiveFilePath: gameDirectory + locationModifications[locationKey].Location, 
                    manager: manager, 
                    patchHistory: patchHistory,
                    progressCallback: locationProgressCallback);
            }

            locationIndex++;
        }

        foreach (var archivedLocations in locationModifications.
                     Where(x => x.Key != String.Empty && x.Value.ArchiveDataManager != null).
                     GroupBy(x => x.Value.ArchiveDataManager))
        {
            await archivedLocations.Key!.OnRepackedArchivesAsync(archivedLocations.Select(x => gameDirectory + x.Value.Location).ToArray());
        }

        progressCallback?.Invoke(new Progress(90, 100));

        // Remove old history and apply new history files
        fileChanges.ApplyNewHistory();

        // Create new history
        PatchHistoryManifest history = new(
            ModifiedDate: DateTime.Now,
            AddedFiles: fileChanges.AddedFiles.ToArray(),
            AddedFileChecksums: fileChanges.AddedFileChecksums.ToArray(),
            ReplacedFiles: fileChanges.ReplacedFiles.ToArray(),
            ReplacedFileChecksums: fileChanges.ReplacedFileChecksums.ToArray(),
            RemovedFiles: fileChanges.RemovedFiles.ToArray());

        // Write the library manifest
        library.WriteManifest(game, history, patches, enabledPatches);

        progressCallback?.Invoke(new Progress(100, 100));
    }

    #endregion

    #region Data Types

    private record FileModification(
        FileModificationType Type, 
        string? PatchID,
        PatchFilePath PatchFilePath,
        bool AddToHistory, 
        PatchFileResourceEntry? ResourceEntry = null);

    private class LocationModifications
    {
        public LocationModifications(string location, IArchiveDataManager? archiveDataManager)
        {
            Location = location;
            ArchiveDataManager = archiveDataManager;
        }

        public string Location { get; }
        public IArchiveDataManager? ArchiveDataManager { get; }
        public Dictionary<string, FileModification> FileModifications { get; } = new();
    }

    private enum FileModificationType
    {
        Add,
        Remove,
    }

    private class PatchFileChanges : IDisposable
    {
        public PatchFileChanges(Context context, PatchLibrary library, IEnumerable<PatchFile> patches)
        {
            Context = context;

            OldHistory = library.GetHistory();
            NewHistoryTempDir = new TempDirectory(true);
            NewHistory = new PatchHistory(NewHistoryTempDir.TempPath);
            PatchFiles = patches.ToDictionary(x => x.Metadata.ID, x => x);
        }

        private TempDirectory NewHistoryTempDir { get; }
        private Context Context { get; }
        private PatchHistory OldHistory { get; }
        private PatchHistory NewHistory { get; }
        private Dictionary<string, PatchFile> PatchFiles { get; }

        public List<PatchFilePath> AddedFiles { get; } = new();
        public List<byte[]> AddedFileChecksums { get; } = new();
        public List<PatchFilePath> ReplacedFiles { get; } = new();
        public List<byte[]> ReplacedFileChecksums { get; } = new();
        public List<PatchFilePath> RemovedFiles { get; } = new();

        public Stream GetPatchResource(FileModification modification)
        {
            // If there's no patch ID we read from history
            if (modification.PatchID == null)
                return OldHistory.ReadFile(modification.PatchFilePath);

            PatchFileResourceEntry resourceEntry = modification.ResourceEntry ?? throw new Exception("Missing resource entry");
            PatchFile patchFile = PatchFiles[modification.PatchID];

            return resourceEntry.ReadData(Context.Deserializer, patchFile.Offset);
        }

        public void AddHistoryResource(PatchFilePath resourcePath, Stream stream)
        {
            NewHistory.AddFile(resourcePath, stream);
        }

        public void KeepHistoryResource(PatchFilePath resourcePath)
        {
            OldHistory.MoveFile(resourcePath, NewHistory);
        }

        public void ApplyNewHistory()
        {
            // Replace old history with new one
            Services.File.MoveDirectory(NewHistory.DirectoryPath, OldHistory.DirectoryPath, true, true);
        }

        public void Dispose()
        {
            NewHistoryTempDir.Dispose();
        }
    }

    #endregion
}