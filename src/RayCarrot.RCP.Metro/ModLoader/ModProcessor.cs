using RayCarrot.RCP.Metro.Archive;
using RayCarrot.RCP.Metro.Games.Components;
using RayCarrot.RCP.Metro.ModLoader.Library;
using RayCarrot.RCP.Metro.ModLoader.Modules;
using RayCarrot.RCP.Metro.ModLoader.Resource;

namespace RayCarrot.RCP.Metro.ModLoader;

/// <summary>
/// The service for applying game mods and updating a mod library
/// </summary>
public class ModProcessor
{
    #region Constructor

    public ModProcessor(GameInstallation gameInstallation)
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
        return _locationModifications.TryGetValue(ModLoaderHelpers.NormalizePath(location));
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
        _locationModifications.Add(ModLoaderHelpers.NormalizePath(location), fileLocationModifications);
        return fileLocationModifications;
    }

    private void AddModification(
        FileModification.FileType type,
        FileModification.FileSource source,
        ModFilePath modFilePath,
        FileModification.HistoryFileEntry? historyEntry = null,
        IModFileResource? resourceEntry = null)
    {
        // Get the location to add the modification to
        FileLocationModifications? locationModifications = GetLocationModifications(modFilePath.Location);
        if (locationModifications == null)
        {
            // Add the location if null
            locationModifications = AddLocationModifications(modFilePath.Location, modFilePath.LocationID);

            // If it's still null then we can't modify this location and we return
            if (locationModifications == null)
                return;
        }

        locationModifications.AddFileModification(type, source, modFilePath, historyEntry, resourceEntry);
    }

    private void AddModificationsFromHistory(ModLibrary library, LibraryFileHistory fileHistory)
    {
        // Remove added files
        foreach (ModFilePath addedFile in fileHistory.AddedFiles)
        {
            AddModification(
                type: FileModification.FileType.Remove,
                source: FileModification.FileSource.History,
                modFilePath: addedFile,
                historyEntry: new FileModification.HistoryFileEntry(FileModification.HistoryFileType.Add));
            Logger.Trace("File mod add -> remove: {0}", addedFile);
        }

        // Add back replaced files
        foreach (ModFilePath replacedFile in fileHistory.ReplacedFiles)
        {
            IModFileResource resource = library.GetFileHistoryResource(replacedFile);

            AddModification(
                type: FileModification.FileType.Add,
                source: FileModification.FileSource.History,
                modFilePath: replacedFile,
                historyEntry: new FileModification.HistoryFileEntry(FileModification.HistoryFileType.Replace, resource),
                resourceEntry: resource);
            Logger.Trace("File mod replace -> add: {0}", replacedFile);
        }

        // Add back removed files
        foreach (ModFilePath removedFile in fileHistory.RemovedFiles)
        {
            IModFileResource resource = library.GetFileHistoryResource(removedFile);

            AddModification(
                type: FileModification.FileType.Add,
                source: FileModification.FileSource.History,
                modFilePath: removedFile,
                historyEntry: new FileModification.HistoryFileEntry(FileModification.HistoryFileType.Remove, resource),
                resourceEntry: resource);
            Logger.Trace("File mod remove -> add: {0}", removedFile);
        }
    }

    private void AddModificationsFromMod(Mod mod)
    {
        foreach (IModFileResource addedFile in mod.GetAddedFiles())
        {
            AddModification(
                type: FileModification.FileType.Add,
                source: FileModification.FileSource.Mod,
                modFilePath: addedFile.Path,
                resourceEntry: addedFile);
            Logger.Trace("File mod add: {0}", addedFile.Path);
        }

        // TODO-UPDATE: Pass in correct version
        foreach (ModFilePath removedFile in mod.GetRemovedFiles())
        {
            AddModification(
                type: FileModification.FileType.Remove,
                source: FileModification.FileSource.Mod,
                modFilePath: removedFile);
            Logger.Trace("File mod remove: {0}", removedFile);
        }
    }

    private IEnumerable<FileLocationModifications> GetLocationModifications() => _locationModifications.Values;

    #endregion

    #region Public Methods

    public async Task<bool> ApplyAsync(
        ModLibrary library,
        IReadOnlyCollection<ModModule> possibleModules,
        Action<Progress>? progressCallback = null)
    {
        // Reset fields
        _archiveDataManagers.Clear();
        _locationModifications.Clear();

        // Get data
        FileSystemPath gameDir = _gameInstallation.InstallLocation.Directory;
        ModManifest modManifest = library.ReadModManifest();

        // Read the mods
        List<(ModManifestEntry Entry, Mod Mod)> enabledMods = new();
        foreach (ModManifestEntry modEntry in modManifest.Mods.Values.Where(x => x.IsEnabled))
            enabledMods.Add((modEntry, library.ReadInstalledMod(modEntry.Id, possibleModules)));

        Logger.Info("Applying modifications with {0} enabled mods", enabledMods.Count);

        // Keep track of file changes
        using LibraryFileHistoryBuilder historyBuilder = new();

        // If a mod history exists then we start by reverting the changes. If any of these changes shouldn't
        // actually be reverted then that will be overridden when we go through the mods to apply.
        LibraryFileHistory? history = library.ReadHistory();
        if (history != null)
        {
            Logger.Info("Getting file modifications from history");

            AddModificationsFromHistory(library, history);
        }

        Logger.Info("Getting file modifications from enabled mods");

        // Add modifications for each enabled mod. Reverse the order for the correct mod priority.
        foreach ((ModManifestEntry Entry, Mod Mod) p in 
                 ((IEnumerable<(ModManifestEntry Entry, Mod Mod)>)enabledMods).Reverse())
        {
            AddModificationsFromMod(p.Mod);
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