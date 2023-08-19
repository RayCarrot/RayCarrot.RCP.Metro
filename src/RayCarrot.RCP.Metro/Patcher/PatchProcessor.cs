﻿using RayCarrot.RCP.Metro.Archive;
using RayCarrot.RCP.Metro.Games.Components;
using RayCarrot.RCP.Metro.Patcher.Library;
using RayCarrot.RCP.Metro.Patcher.Resource;

namespace RayCarrot.RCP.Metro.Patcher;

/// <summary>
/// The service for patching game files and updating a patch library
/// </summary>
public class PatchProcessor
{
    #region Constructor

    public PatchProcessor(GameInstallation gameInstallation)
    {
        _gameInstallation = gameInstallation;
    }

    #endregion

    #region Logger

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    #endregion

    #region Private Fields

    private readonly Dictionary<string, IArchiveDataManager?> _archiveDataManagers = new();
    private readonly Dictionary<string, FileLocationModifications> _locationModifications = new();
    private readonly GameInstallation _gameInstallation;

    #endregion

    #region Private Methods

    private FileLocationModifications? GetLocationModifications(string location)
    {
        return _locationModifications.TryGetValue(PatcherHelpers.NormalizePath(location));
    }

    private FileLocationModifications? AddLocationModifications(string location, string locationId)
    {
        IArchiveDataManager? manager = null;

        // If the location is not empty then we're in an archive. An empty location means
        // the physical game installation for which the archive manager is null.
        if (location != String.Empty && locationId != String.Empty)
        {
            // Create the manager if it hasn't already been created
            if (!_archiveDataManagers.ContainsKey(locationId))
            {
                // Find registered archive manager with the matching id
                ArchiveComponent? archiveComponent = _gameInstallation.
                    GetComponents<ArchiveComponent>().
                    FirstOrDefault(x => x.Id == locationId);

                // Cache the archive manager
                _archiveDataManagers.Add(locationId, archiveComponent?.CreateObject());

                // This might happen if you multi-target games with different archive managers
                if (archiveComponent == null)
                    Logger.Warn("Archive manager with id {0} wasn't found. Some file modifications will be skipped.", locationId);
            }

            // Get the manager for this id
            manager = _archiveDataManagers[locationId];

            // If the manager is null we ignore this file since it can't be processed
            if (manager == null)
                return null;
        }

        FileLocationModifications fileLocationModifications = new(location, manager);
        _locationModifications.Add(PatcherHelpers.NormalizePath(location), fileLocationModifications);
        return fileLocationModifications;
    }

    private void AddModification(
        FileModification.FileType type,
        FileModification.FileSource source,
        PatchFilePath patchFilePath,
        FileModification.HistoryFileEntry? historyEntry = null,
        IPatchFileResource? resourceEntry = null)
    {
        // Get the location to add the modification to
        FileLocationModifications? locationModifications = GetLocationModifications(patchFilePath.Location);
        if (locationModifications == null)
        {
            // Add the location if null
            locationModifications = AddLocationModifications(patchFilePath.Location, patchFilePath.LocationID);

            // If it's still null then we can't modify this location and we return
            if (locationModifications == null)
                return;
        }

        locationModifications.AddFileModification(type, source, patchFilePath, historyEntry, resourceEntry);
    }

    private void AddModificationsFromHistory(PatchLibrary library, LibraryFileHistory fileHistory)
    {
        // Remove added files
        foreach (PatchFilePath addedFile in fileHistory.AddedFiles)
        {
            AddModification(
                type: FileModification.FileType.Remove,
                source: FileModification.FileSource.History,
                patchFilePath: addedFile,
                historyEntry: new FileModification.HistoryFileEntry(FileModification.HistoryFileType.Add));
            Logger.Trace("File mod add -> remove: {0}", addedFile);
        }

        // Add back replaced files
        foreach (PatchFilePath replacedFile in fileHistory.ReplacedFiles)
        {
            IPatchFileResource resource = library.GetFileHistoryResource(replacedFile);

            AddModification(
                type: FileModification.FileType.Add,
                source: FileModification.FileSource.History,
                patchFilePath: replacedFile,
                historyEntry: new FileModification.HistoryFileEntry(FileModification.HistoryFileType.Replace, resource),
                resourceEntry: resource);
            Logger.Trace("File mod replace -> add: {0}", replacedFile);
        }

        // Add back removed files
        foreach (PatchFilePath removedFile in fileHistory.RemovedFiles)
        {
            IPatchFileResource resource = library.GetFileHistoryResource(removedFile);

            AddModification(
                type: FileModification.FileType.Add,
                source: FileModification.FileSource.History,
                patchFilePath: removedFile,
                historyEntry: new FileModification.HistoryFileEntry(FileModification.HistoryFileType.Remove, resource),
                resourceEntry: resource);
            Logger.Trace("File mod remove -> add: {0}", removedFile);
        }
    }

    private void AddModificationsFromPatch(InstalledPatch patch, string version)
    {
        foreach (IPatchFileResource addedFile in patch.GetAddedFiles(version))
        {
            AddModification(
                type: FileModification.FileType.Add,
                source: FileModification.FileSource.Patch,
                patchFilePath: addedFile.Path,
                resourceEntry: addedFile);
            Logger.Trace("File mod add: {0}", addedFile.Path);
        }

        // TODO-UPDATE: Pass in correct version
        foreach (PatchFilePath removedFile in patch.GetRemovedFiles(version))
        {
            AddModification(
                type: FileModification.FileType.Remove,
                source: FileModification.FileSource.Patch,
                patchFilePath: removedFile);
            Logger.Trace("File mod remove: {0}", removedFile);
        }
    }

    private IEnumerable<FileLocationModifications> GetLocationModifications() => _locationModifications.Values;

    #endregion

    #region Public Methods

    public async Task<bool> ApplyAsync(
        PatchLibrary library,
        Action<Progress>? progressCallback = null)
    {
        // Reset fields
        _archiveDataManagers.Clear();
        _locationModifications.Clear();

        // Get data
        FileSystemPath gameDir = _gameInstallation.InstallLocation.Directory;
        PatchManifest patchManifest = library.ReadPatchManifest();

        Logger.Info("Applying patcher modifications with {0} enabled patches", patchManifest.Patches.Count);

        // Read the patches
        List<(PatchManifestEntry Entry, InstalledPatch Patch)> patches = new(patchManifest.Patches.Count);
        foreach (PatchManifestEntry patchEntry in patchManifest.Patches.Values)
            patches.Add((patchEntry, library.ReadInstalledPatch(patchEntry.Id)));

        // Keep track of file changes
        using LibraryFileHistoryBuilder historyBuilder = new();

        // If a patch history exists then we start by reverting the changes. If any of these changes shouldn't
        // actually be reverted then that will be overridden when we go through the patches to apply.
        LibraryFileHistory? history = library.ReadHistory();
        if (history != null)
        {
            Logger.Info("Getting file modifications from history");

            AddModificationsFromHistory(library, history);
        }

        Logger.Info("Getting file modifications from enabled patches");

        // Add modifications for each enabled patch. Reverse the order for the correct patch priority.
        foreach ((PatchManifestEntry Entry, InstalledPatch Patch) p in 
                 ((IEnumerable<(PatchManifestEntry Entry, InstalledPatch Patch)>)patches).Reverse())
        {
            AddModificationsFromPatch(p.Patch, p.Entry.Version ?? InstalledPatch.DefaultVersion);
        }

        Logger.Info("Finished retrieving file modifications");

        // Calculate progress lengths
        int totalFilesCount = GetLocationModifications().Sum(x => x.FilesCount);
        double onRepackedProgressLength = GetLocationModifications().
            Where(x => x.Location != String.Empty && x.ArchiveDataManager != null).
            Select(x => x.ArchiveDataManager!).
            Distinct().
            Select(x => x.GetOnRepackedArchivesProgressLength()).Sum() * totalFilesCount;
        Progress currentProgress = new(0, totalFilesCount + onRepackedProgressLength);

        // Start
        progressCallback?.Invoke(currentProgress);

        Action<Progress>? getOperationProgressCallback(double length) => progressCallback == null
            ? null
            : x => progressCallback.Invoke(currentProgress.Add(x, length));

        bool success = true;

        // Modify every location
        foreach (FileLocationModifications locationModifications in GetLocationModifications())
        {
            try
            {
                locationModifications.ApplyModifications(historyBuilder, gameDir, getOperationProgressCallback(locationModifications.FilesCount));
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Applying file modification for location {0}", locationModifications.Location);
                success = false;
            }

            currentProgress += locationModifications.FilesCount;
        }

        foreach (var archivedLocations in GetLocationModifications().
                     Where(x => x.Location != String.Empty && x.ArchiveDataManager != null).
                     GroupBy(x => x.ArchiveDataManager))
        {
            double progressLength = archivedLocations.Key!.GetOnRepackedArchivesProgressLength() * totalFilesCount;
            await archivedLocations.Key!.OnRepackedArchivesAsync(archivedLocations.Select(x => gameDir + x.Location).ToArray(), getOperationProgressCallback(progressLength));
            currentProgress += progressLength;
        }

        // Write the new file history
        historyBuilder.BuildFileHistory(library);

        // Complete!
        progressCallback?.Invoke(currentProgress.Completed());

        return success;
    }

    #endregion
}