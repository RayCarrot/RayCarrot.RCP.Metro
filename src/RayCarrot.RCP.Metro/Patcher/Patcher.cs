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

    private Dictionary<string, LocationModifications> GetFileModificationsPerLocation(Games game, PatchLibraryHistory? patchHistory, IEnumerable<PatchFile> enabledPatches)
    {
        Dictionary<string, LocationModifications> locationModifications = new();
        Dictionary<string, IArchiveDataManager> archiveDataManagers = new();

        void addModification(
            PatchFilePath patchFilePath, 
            FileModificationType type,
            bool addToHistory,
            PackagedResourceEntry? resourceEntry = null,
            PackagedResourceChecksum? checksum = null)
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

            locationModifications[locationKey].FileModifications[filePathKey] = new FileModification(type, patchFilePath, addToHistory, resourceEntry, checksum);
        }

        // If a patch history exists then we start by reverting the changes. If any of these changes shouldn't
        // actually be reverted then that will be overridden when we go through the patches to apply.
        if (patchHistory != null)
        {
            Logger.Info("Getting file modifications from history");

            // Remove added files
            foreach (PatchFilePath addedFile in patchHistory.AddedFiles)
            {
                addModification(addedFile, FileModificationType.Remove, false);
                Logger.Trace("File mod add -> remove: {0}", addedFile);
            }

            // Add back replaced files
            for (var i = 0; i < patchHistory.ReplacedFiles.Length; i++)
            {
                PatchFilePath filePath = patchHistory.ReplacedFiles[i];
                PackagedResourceEntry resource = patchHistory.ReplacedFileResources[i];
                PackagedResourceChecksum checksum = patchHistory.ReplacedFileChecksums[i];

                addModification(filePath, FileModificationType.Add, false, resource, checksum);
                Logger.Trace("File mod replace -> add: {0}", filePath);
            }

            // Add back removed files
            for (var i = 0; i < patchHistory.RemovedFiles.Length; i++)
            {
                PatchFilePath filePath = patchHistory.RemovedFiles[i];
                PackagedResourceEntry resource = patchHistory.RemovedFileResources[i];
                
                addModification(filePath, FileModificationType.Add, false, resource);
                Logger.Trace("File mod remove -> add: {0}", filePath);
            }
        }

        Logger.Info("Getting file modifications from enabled patches");

        // Add modifications for each enabled patch. Reverse the order for the correct patch priority.
        foreach (PatchFile patch in enabledPatches.Reverse())
        {
            for (var i = 0; i < patch.AddedFileResources.Length; i++)
            {
                PatchFilePath filePath = patch.AddedFiles[i];
                PackagedResourceChecksum checksum = patch.AddedFileChecksums[i];
                PackagedResourceEntry resource = patch.AddedFileResources[i];
                addModification(filePath, FileModificationType.Add, true, resource, checksum);
                Logger.Trace("File mod add: {0}", filePath);
            }

            if (patch.RemovedFiles != null)
            {
                foreach (PatchFilePath removedFile in patch.RemovedFiles)
                {
                    addModification(removedFile, FileModificationType.Remove, true);
                    Logger.Trace("File mod remove: {0}", removedFile);
                }
            }
            else
            {
                Logger.Warn("Patch {0} removed files array is null", patch.Metadata.ID);
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
        PatchLibraryHistory? patchHistory,
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
                        PackagedResourceChecksum checksum = modification.Checksum ?? throw new Exception("Missing resource checksum");

                        // If the file was added previously we don't want to mark it as being replaced or else we'd 
                        // be replacing the previously added file
                        if (prevAddedFiles?.Contains(filePathKey) == true)
                        {
                            fileChanges.AddAddedFile(patchFilePath, checksum);
                        }
                        // If the file was replaced previously we want to keep the originally removed file instead
                        // of the one it was replaced with before
                        else if (prevReplacedFiles?.Contains(filePathKey) == true)
                        {
                            PackagedResourceEntry resourceEntry = modification.ResourceEntry ?? throw new Exception("Missing resource entry");

                            fileChanges.AddReplacedFile(patchFilePath, checksum, resourceEntry);
                        }
                        else
                        {
                            SaveRemovedPhysicalFileInHistory(fileChanges, physicalFilePath, patchFilePath, checksum);
                        }
                    }

                    // Replace the file
                    using Stream resource = modification.GetPatchResource(fileChanges.Context);
                    ReplacePhysicalFile(physicalFilePath, resource);
                }
                else
                {
                    Logger.Trace("Adding file {0}", patchFilePath);

                    // Replace the file
                    using (Stream resource = modification.GetPatchResource(fileChanges.Context))
                        ReplacePhysicalFile(physicalFilePath, resource);

                    if (modification.AddToHistory)
                    {
                        PackagedResourceChecksum checksum = modification.Checksum ?? throw new Exception("Missing resource checksum");

                        fileChanges.AddAddedFile(patchFilePath, checksum);
                    }
                }
            }
            else if (modification.Type == FileModificationType.Remove)
            {
                Logger.Trace("Removing file {0}", patchFilePath);

                if (modification.AddToHistory)
                    SaveRemovedPhysicalFileInHistory(fileChanges, physicalFilePath, patchFilePath);

                // TODO-UPDATE: This might end up leaving a lot of empty folders - delete folder if empty?
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
        PatchLibraryHistory? patchHistory,
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
                                PackagedResourceChecksum checksum = modification.Checksum ?? throw new Exception("Missing resource checksum");

                                // If the file was added previously we don't want to mark it as being replaced or else we'd 
                                // be replacing the previously added file
                                if (prevAddedFiles?.Contains(filePathKey) == true)
                                {
                                    fileChanges.AddAddedFile(patchFilePath, checksum);
                                }
                                // If the file was replaced previously we want to keep the originally removed file instead
                                // of the one it was replaced with before
                                else if (prevReplacedFiles?.Contains(filePathKey) == true)
                                {
                                    PackagedResourceEntry resourceEntry = modification.ResourceEntry ?? throw new Exception("Missing resource entry");

                                    fileChanges.AddReplacedFile(patchFilePath, checksum, resourceEntry);
                                }
                                else
                                {
                                    SaveRemovedArchiveFileInHistory(fileChanges, file, archiveData.Generator, patchFilePath, checksum);
                                }
                            }

                            using Stream resource = modification.GetPatchResource(fileChanges.Context);
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

                    using (Stream resource = modification.GetPatchResource(fileChanges.Context))
                        ReplaceArchiveFile(file, manager, resource);

                    if (modification.AddToHistory)
                    {
                        PackagedResourceChecksum checksum = modification.Checksum ?? throw new Exception("Missing resource checksum");

                        fileChanges.AddAddedFile(patchFilePath, checksum);
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
        PackagedResourceChecksum? replacedFileChecksum = null)
    {
        PackagedResourceEntry resource = fileChanges.CreateResourceEntry(fileStream);

        if (replacedFileChecksum != null)
            fileChanges.AddReplacedFile(patchFilePath, replacedFileChecksum, resource);
        else
            fileChanges.AddRemovedFile(patchFilePath, resource);

        Logger.Trace("Saved removed file {0} in history", patchFilePath);
    }

    private void SaveRemovedPhysicalFileInHistory(
        PatchFileChanges fileChanges,
        FileSystemPath filePath,
        PatchFilePath patchFilePath,
        PackagedResourceChecksum? replacedFileChecksum = null)
    {
        using Stream fileStream = File.OpenRead(filePath);
        SaveRemovedFileInHistory(fileChanges, fileStream, patchFilePath, replacedFileChecksum);
    }

    private void SaveRemovedArchiveFileInHistory(
        PatchFileChanges fileChanges, 
        FileItem file, 
        IDisposable generator,
        PatchFilePath patchFilePath,
        PackagedResourceChecksum? replacedFileChecksum = null)
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

    public async Task<bool> ApplyAsync(
        Games game,
        PatchLibrary library,
        FileSystemPath gameDirectory,
        PatchLibraryPatchEntry[] patches,
        Action<Progress>? progressCallback = null)
    {
        Logger.Info("Applying patcher modifications with {0}/{1} enabled patches", patches.Count(x => x.IsEnabled), patches.Length);

        using RCPContext context = new(library.DirectoryPath);

        // Read the patch files for the enabled patches
        PatchFile[] enabledPatchFiles = patches.
            Where(x => x.IsEnabled).
            Select(x => context.ReadRequiredFileData<PatchFile>(library.GetPatchFileName(x.ID), removeFileWhenComplete: false)).
            ToArray();

        // Read any existing patch history
        PatchLibraryFile? libraryFile = context.ReadFileData<PatchLibraryFile>(library.LibraryFileName, removeFileWhenComplete: false);

        // Keep track of file changes
        using PatchFileChanges fileChanges = new(context);

        // Get the file modifications for each location
        Dictionary<string, LocationModifications> locationModifications =
            GetFileModificationsPerLocation(game, libraryFile?.History, enabledPatchFiles);

        int maxProgress = locationModifications.Count + 1;
        int progressIndex = 0;

        progressCallback?.Invoke(new Progress(progressIndex, maxProgress));

        Action<Progress>? operationProgressCallback = progressCallback == null
            ? null
            : x => progressCallback?.Invoke(new Progress(progressIndex + x.Percentage / 100, maxProgress));

        bool success = true;

        // Modify every location
        foreach (string locationKey in locationModifications.Keys)
        {
            try
            {
                // Physical
                if (locationKey == String.Empty)
                {
                    ModifyPhysicalFiles(
                        fileChanges: fileChanges,
                        locationKey: locationKey,
                        fileModifications: locationModifications[locationKey].FileModifications,
                        dirPath: gameDirectory,
                        patchHistory: libraryFile?.History,
                        progressCallback: operationProgressCallback);
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
                        patchHistory: libraryFile?.History,
                        progressCallback: operationProgressCallback);
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Applying file modification for location {0}", locationModifications[locationKey].Location);
                success = false;
            }

            progressIndex++;
        }

        foreach (var archivedLocations in locationModifications.
                     Where(x => x.Key != String.Empty && x.Value.ArchiveDataManager != null).
                     GroupBy(x => x.Value.ArchiveDataManager))
        {
            await archivedLocations.Key!.OnRepackedArchivesAsync(archivedLocations.Select(x => gameDirectory + x.Value.Location).ToArray());
        }

        progressCallback?.Invoke(new Progress(progressIndex, maxProgress));

        // Create the library file if it didn't already exist
        if (libraryFile == null)
        {
            libraryFile = new PatchLibraryFile();
            LinearFile file = context.AddFile(new LinearFile(context, library.LibraryFileName));
            libraryFile.Init(file.StartPointer);
        }

        // Create new history
        PatchLibraryHistory history = fileChanges.CreateHistory();

        // Update the library file
        libraryFile.Version = PatchLibraryFile.LatestVersion;
        libraryFile.Game = game;
        libraryFile.History = history;
        libraryFile.Patches = patches;

        // Write and pack the library file
        libraryFile.WriteAndPackResources(operationProgressCallback);

        progressCallback?.Invoke(new Progress(maxProgress, maxProgress));

        return success;
    }

    #endregion

    #region Data Types

    public record PatcherResult(bool Suceeded, string ErrorMessage);

    private record FileModification(
        FileModificationType Type,
        PatchFilePath PatchFilePath,
        bool AddToHistory,
        PackagedResourceEntry? ResourceEntry = null,
        PackagedResourceChecksum? Checksum = null)
    {
        public Stream GetPatchResource(Context context)
        {
            PackagedResourceEntry resourceEntry = ResourceEntry ?? throw new Exception("Missing resource entry");
            return resourceEntry.ReadData(context, true);
        }
    }

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
        public PatchFileChanges(Context context)
        {
            Context = context;
        }

        private List<TempFile> TempFiles { get; } = new();

        private List<PatchFilePath> AddedFiles { get; } = new();
        private List<PackagedResourceChecksum> AddedFileChecksums { get; } = new();
        private List<PatchFilePath> ReplacedFiles { get; } = new();
        private List<PackagedResourceChecksum> ReplacedFileChecksums { get; } = new();
        private List<PackagedResourceEntry> ReplacedFileResources { get; } = new();
        private List<PatchFilePath> RemovedFiles { get; } = new();
        private List<PackagedResourceEntry> RemovedFileResources { get; } = new();

        public Context Context { get; }

        public void AddAddedFile(PatchFilePath filePath, PackagedResourceChecksum checksum)
        {
            AddedFiles.Add(filePath);
            AddedFileChecksums.Add(checksum);
        }

        public void AddReplacedFile(PatchFilePath filePath, PackagedResourceChecksum checksum, PackagedResourceEntry resource)
        {
            ReplacedFiles.Add(filePath);
            ReplacedFileChecksums.Add(checksum);
            ReplacedFileResources.Add(resource);
        }

        public void AddRemovedFile(PatchFilePath filePath, PackagedResourceEntry resource)
        {
            RemovedFiles.Add(filePath);
            RemovedFileResources.Add(resource);
        }

        public PackagedResourceEntry CreateResourceEntry(Stream stream)
        {
            TempFile tempFile = new(false);
            TempFiles.Add(tempFile);

            using Stream tempFileStream = File.Create(tempFile.TempPath);
            stream.CopyTo(tempFileStream);

            PackagedResourceEntry resource = new();
            resource.SetPendingImport(() => File.OpenRead(tempFile.TempPath), false);
            return resource;
        }

        public PatchLibraryHistory CreateHistory() => new()
        {
            ModifiedDate = DateTime.Now,
            AddedFiles = AddedFiles.ToArray(),
            AddedFileChecksums = AddedFileChecksums.ToArray(),
            ReplacedFiles = ReplacedFiles.ToArray(),
            ReplacedFileChecksums = ReplacedFileChecksums.ToArray(),
            ReplacedFileResources = ReplacedFileResources.ToArray(),
            RemovedFiles = RemovedFiles.ToArray(),
            RemovedFileResources = RemovedFileResources.ToArray(),
        };

        public void Dispose()
        {
            TempFiles.DisposeAll();
        }
    }

    #endregion
}