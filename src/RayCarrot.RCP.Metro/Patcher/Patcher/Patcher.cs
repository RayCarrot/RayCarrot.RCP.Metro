using System.Diagnostics;
using System.IO;
using BinarySerializer;
using RayCarrot.RCP.Metro.Archive;
using RayCarrot.RCP.Metro.Games.Components;

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

    private Dictionary<string, LocationModifications> GetFileModificationsPerLocation(
        GameInstallation gameInstallation, 
        PatchLibraryHistory? patchHistory, 
        IEnumerable<PatchFile> enabledPatches)
    {
        Dictionary<string, LocationModifications> locationModifications = new();
        Dictionary<string, IArchiveDataManager?> archiveDataManagers = new();

        // Local helper method for adding a file modification
        void addModification(
            PatcherFileModification.FileType type,
            PatcherFileModification.FileSource source,
            PatchFilePath patchFilePath,
            PatcherFileModification.HistoryFileEntry? historyEntry = null,
            PackagedResourceEntry? resourceEntry = null,
            PackagedResourceChecksum? checksum = null)
        {
            string locationKey = NormalizePath(patchFilePath.Location);
            string filePathKey = NormalizePath(patchFilePath.FilePath);

            // Add the location to the dictionary if it doesn't exist
            if (!locationModifications.ContainsKey(locationKey))
            {
                IArchiveDataManager? manager = null;
                string locationId = patchFilePath.LocationID;

                // If the location is not empty then we're in an archive. An empty location means
                // the physical game installation for which the archive manager is null.
                if (patchFilePath.Location != String.Empty && locationId != String.Empty)
                {
                    // Create the manager if it hasn't already been created
                    if (!archiveDataManagers.ContainsKey(locationId))
                    {
                        ArchiveComponent? archiveComponent = gameInstallation.GetComponents<ArchiveComponent>().
                            FirstOrDefault(x => x.Id == locationId);

                        archiveDataManagers.Add(locationId, archiveComponent?.CreateObject());

                        // This might happen if you multi-target games with different archive managers
                        if (archiveComponent == null)
                            Logger.Warn("Archive manager with id {0} wasn't found. Some file modifications will be skipped.", locationId);
                    }

                    // Get the manager for this id
                    manager = archiveDataManagers[locationId];

                    // If the manager is null we ignore this file since it can't be processed
                    if (manager == null)
                        return;
                }

                locationModifications.Add(locationKey, new LocationModifications(patchFilePath.Location, manager));
            }

            // If this modification overrides an existing one we want to copy over the history entry
            if (historyEntry == null && locationModifications[locationKey].FileModifications.ContainsKey(filePathKey))
            {
                PatcherFileModification existingModification = locationModifications[locationKey].FileModifications[filePathKey];
                historyEntry = existingModification.HistoryEntry;
            }

            locationModifications[locationKey].FileModifications[filePathKey] = new PatcherFileModification(
                type: type, 
                source: source,
                patchFilePath: patchFilePath, 
                historyEntry: historyEntry,
                resourceEntry: resourceEntry, 
                checksum: checksum);
        }

        // If a patch history exists then we start by reverting the changes. If any of these changes shouldn't
        // actually be reverted then that will be overridden when we go through the patches to apply.
        if (patchHistory != null)
        {
            Logger.Info("Getting file modifications from history");

            // Remove added files
            foreach (PatchFilePath addedFile in patchHistory.AddedFiles)
            {
                addModification(
                    type: PatcherFileModification.FileType.Remove,
                    source: PatcherFileModification.FileSource.History,
                    patchFilePath: addedFile, 
                    historyEntry: new PatcherFileModification.HistoryFileEntry(PatcherFileModification.HistoryFileType.Add));
                Logger.Trace("File mod add -> remove: {0}", addedFile);
            }

            // Add back replaced files
            for (var i = 0; i < patchHistory.ReplacedFiles.Length; i++)
            {
                PatchFilePath filePath = patchHistory.ReplacedFiles[i];
                PackagedResourceEntry resource = patchHistory.ReplacedFileResources[i];
                PackagedResourceChecksum checksum = patchHistory.ReplacedFileChecksums[i];

                addModification(
                    type: PatcherFileModification.FileType.Add, 
                    source: PatcherFileModification.FileSource.History,
                    patchFilePath: filePath, 
                    historyEntry: new PatcherFileModification.HistoryFileEntry(PatcherFileModification.HistoryFileType.Replace, resource),
                    resourceEntry: resource, 
                    checksum: checksum);
                Logger.Trace("File mod replace -> add: {0}", filePath);
            }

            // Add back removed files
            for (var i = 0; i < patchHistory.RemovedFiles.Length; i++)
            {
                PatchFilePath filePath = patchHistory.RemovedFiles[i];
                PackagedResourceEntry resource = patchHistory.RemovedFileResources[i];
                
                addModification(
                    type: PatcherFileModification.FileType.Add, 
                    source: PatcherFileModification.FileSource.History,
                    patchFilePath: filePath,
                    historyEntry: new PatcherFileModification.HistoryFileEntry(PatcherFileModification.HistoryFileType.Remove, resource),
                    resourceEntry: resource);
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
                addModification(
                    type: PatcherFileModification.FileType.Add, 
                    source: PatcherFileModification.FileSource.Patch,
                    patchFilePath: filePath, 
                    resourceEntry: resource, 
                    checksum: checksum);
                Logger.Trace("File mod add: {0}", filePath);
            }

            foreach (PatchFilePath removedFile in patch.RemovedFiles)
            {
                addModification(
                    type: PatcherFileModification.FileType.Remove,
                    source: PatcherFileModification.FileSource.Patch,
                    patchFilePath: removedFile);
                Logger.Trace("File mod remove: {0}", removedFile);
            }
        }

        Logger.Info("Got {0} file modifications", locationModifications.Count);

        return locationModifications;
    }

    private void ModifyPhysicalFiles(
        PatcherFileChanges fileChanges,
        Dictionary<string, PatcherFileModification> fileModifications,
        FileSystemPath dirPath,
        Action<Progress>? progressCallback)
    {
        int fileIndex = 0;

        // Process each file modification
        foreach (var fileModification in fileModifications)
        {
            PatcherFileModification modification = fileModification.Value;
            PatchFilePath patchFilePath = modification.PatchFilePath;
            FileSystemPath physicalFilePath = dirPath + patchFilePath.FilePath;

            // Make sure the file is inside of the directory. We want to disallow going up the tree using ..\
            if (!Path.GetFullPath(physicalFilePath).StartsWith(dirPath))
            {
                Logger.Warn("File modification with path {0} is not valid", patchFilePath.FilePath);
                continue;
            }

            modification.ProcessFile(
                fileChanges: fileChanges,
                fileExists: physicalFilePath.FileExists,
                getCurrentFile: () => File.OpenRead(physicalFilePath),
                addCurrentFile: x => ReplacePhysicalFile(physicalFilePath, x),
                deleteFile: () =>
                {
                    // Delete the file
                    physicalFilePath.DeleteFile();

                    // Delete the directory if it's empty
                    fileChanges.DeleteDirectoryIfEmpty(physicalFilePath.Parent);
                });

            fileIndex++;
            progressCallback?.Invoke(new Progress(fileIndex, fileModifications.Count));
        }
    }

    private void ModifyArchive(
        PatcherFileChanges fileChanges, 
        Dictionary<string, PatcherFileModification> fileModifications, 
        FileSystemPath archiveFilePath, 
        IArchiveDataManager manager, 
        Action<Progress>? progressCallback)
    {
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

                Logger.Info("Modifying archive");

                Stopwatch sw = Stopwatch.StartNew();

                int initialModificationsCount = fileModifications.Values.Count;
                int totalFilesMax = archiveData.Directories.Sum(x => x.Files.Length) + initialModificationsCount;
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

                        PatcherFileModification? modification = fileModifications.TryGetValue(filePathKey);

                        if (modification == null)
                        {
                            // Keep the file and do nothing else
                            archiveFiles.Add(file);
                            continue;
                        }

                        fileModifications.Remove(filePathKey);

                        modification.ProcessFile(
                            fileChanges: fileChanges,
                            fileExists: true,
                            // ReSharper disable once AccessToDisposedClosure
                            getCurrentFile: () => file.GetDecodedFileData(archiveData.Generator).Stream,
                            addCurrentFile: x =>
                            {
                                archiveFiles.Add(file);
                                ReplaceArchiveFile(file, manager, x);
                            },
                            deleteFile: () => { });
                    }
                }

                totalFilesIndex += initialModificationsCount - fileModifications.Values.Count;

                // Add files not already in the archive
                foreach (PatcherFileModification modification in fileModifications.Values)
                {
                    progressCallback?.Invoke(new Progress(totalFilesIndex, totalFilesMax));
                    totalFilesIndex++;

                    modification.ProcessFile(
                        fileChanges: fileChanges,
                        fileExists: false,
                        getCurrentFile: () => throw new InvalidOperationException("Can't get a file which doesn't exist"),
                        addCurrentFile: x =>
                        {
                            // Get the file name and directory
                            string fileName = Path.GetFileName(modification.PatchFilePath.FilePath);
                            string dir = Path.GetDirectoryName(modification.PatchFilePath.FilePath)?.
                                Replace(Path.DirectorySeparatorChar, manager.PathSeparatorCharacter) ?? String.Empty;

                            // Create a new archive entry
                            object entry = manager.GetNewFileEntry(archive, dir, fileName);

                            Logger.Trace("Adding file {0}", modification.PatchFilePath);

                            // Create a new file item
                            FileItem file = new(manager, fileName, dir, entry);

                            // Add the file
                            archiveFiles.Add(file);

                            // Set the stream
                            ReplaceArchiveFile(file, manager, x);
                        },
                        deleteFile: () => { });
                }

                sw.Stop();

                Logger.Info($"Processed modified archive files in {sw.ElapsedMilliseconds} ms");

                // 50%
                progressCallback?.Invoke(new Progress(50, 100));

                sw.Restart();

                using ArchiveFileStream archiveOutputStream = new(File.OpenWrite(archiveOutputFile.TempPath),
                    archiveOutputFile.TempPath.Name, true);

                manager.WriteArchive(
                    generator: archiveData.Generator, 
                    archive: archive, 
                    outputFileStream: archiveOutputStream, 
                    files: archiveFiles, 
                    progressCallback: x => progressCallback?.Invoke(new Progress(x.Percentage_100 * 0.5 + 50, 100)), // 50-100%
                    cancellationToken: CancellationToken.None);

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
        manager.EncodeFile(resource, file.PendingImport, file.ArchiveEntry, 
            // TODO: It would be good to set the file metadata here. But at the moment it's not possible since
            //       there is no metadata saved in .gp patch files. We only have access to the resource stream.
            new FileMetadata());

        // If no data was encoded we copy over the decoded data
        if (file.PendingImport.Length == 0)
            resource.CopyTo(file.PendingImport);
    }

    #endregion

    #region Public Methods

    public async Task<bool> ApplyAsync(
        GameInstallation gameInstallation,
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
        using PatcherFileChanges fileChanges = new(context);

        // Get the file modifications for each location
        Dictionary<string, LocationModifications> locationModifications =
            GetFileModificationsPerLocation(gameInstallation, libraryFile?.History, enabledPatchFiles);

        // Get the full path for the game directory. Needed when we check to make sure the relative path is a child of this.
        gameDirectory = Path.GetFullPath(gameDirectory);

        int totalFilesCount = locationModifications.Values.Sum(x => x.FileModifications.Count);
        int libraryRepackProgressLength = totalFilesCount / 3;
        double onRepackedProgressLength = locationModifications.
            Where(x => x.Key != String.Empty && x.Value.ArchiveDataManager != null).
            Select(x => x.Value.ArchiveDataManager!).
            Distinct().
            Select(x => x.GetOnRepackedArchivesProgressLength()).Sum() * totalFilesCount;
        Progress currentProgress = new(0, totalFilesCount + libraryRepackProgressLength + onRepackedProgressLength);

        progressCallback?.Invoke(currentProgress);

        Action<Progress>? getOperationProgressCallback(double length) => progressCallback == null
            ? null
            : x => progressCallback.Invoke(currentProgress.Add(x, length));

        bool success = true;

        // Modify every location
        foreach (string locationKey in locationModifications.Keys)
        {
            int locationFilesCount = locationModifications[locationKey].FileModifications.Count;

            try
            {
                // Physical
                if (locationKey == String.Empty)
                {
                    ModifyPhysicalFiles(
                        fileChanges: fileChanges,
                        fileModifications: locationModifications[locationKey].FileModifications,
                        dirPath: gameDirectory,
                        progressCallback: getOperationProgressCallback(locationFilesCount));
                }
                // Archive
                else
                {
                    IArchiveDataManager manager = locationModifications[locationKey].ArchiveDataManager ??
                                                  throw new Exception($"No archive data manager for location {locationKey}");

                    ModifyArchive(
                        fileChanges: fileChanges,
                        fileModifications: locationModifications[locationKey].FileModifications,
                        archiveFilePath: gameDirectory + locationModifications[locationKey].Location,
                        manager: manager,
                        progressCallback: getOperationProgressCallback(locationFilesCount));
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Applying file modification for location {0}", locationModifications[locationKey].Location);
                success = false;
            }

            currentProgress += locationFilesCount;
        }

        foreach (var archivedLocations in locationModifications.
                     Where(x => x.Key != String.Empty && x.Value.ArchiveDataManager != null).
                     GroupBy(x => x.Value.ArchiveDataManager))
        {
            double progressLength = archivedLocations.Key!.GetOnRepackedArchivesProgressLength() * totalFilesCount;
            await archivedLocations.Key!.OnRepackedArchivesAsync(archivedLocations.Select(x => gameDirectory + x.Value.Location).ToArray(), getOperationProgressCallback(progressLength));
            currentProgress += progressLength;
        }

        progressCallback?.Invoke(currentProgress);

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
        libraryFile.FormatVersion = PatchLibraryFile.LatestFormatVersion;
        libraryFile.GameId = gameInstallation.GameId;
        libraryFile.History = history;
        libraryFile.Patches = patches;

        // Write and pack the library file
        libraryFile.WriteAndPackResources(getOperationProgressCallback(libraryRepackProgressLength));

        progressCallback?.Invoke(currentProgress.Completed());

        return success;
    }

    #endregion

    #region Data Types

    private class LocationModifications
    {
        public LocationModifications(string location, IArchiveDataManager? archiveDataManager)
        {
            Location = location;
            ArchiveDataManager = archiveDataManager;
        }

        public string Location { get; }
        public IArchiveDataManager? ArchiveDataManager { get; }
        public Dictionary<string, PatcherFileModification> FileModifications { get; } = new();
    }

    #endregion
}