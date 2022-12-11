using System.IO;

namespace RayCarrot.RCP.Metro.Patcher;

// Why is the code for adding a file to the history so confusing? Well because we're trying to combine two steps into one! Ideally
// when you apply new patches it should do this:
// 1. Revert old changes
// 2. Apply new changes
// However this is rather inefficient. A lot of the time it will revert and then apply the same changes. So instead we try and combine
// them into one step. This is the reason for all the additional case checks we have to perform to avoid messing up the history.

public class PatcherFileModification
{
    #region Constructor

    public PatcherFileModification(
        FileType type,
        FileSource source,
        PatchFilePath patchFilePath,
        HistoryFileEntry? historyEntry = null,
        PackagedResourceEntry? resourceEntry = null,
        PackagedResourceChecksum? checksum = null)
    {
        Type = type;
        Source = source;
        PatchFilePath = patchFilePath;
        HistoryEntry = historyEntry;
        ResourceEntry = resourceEntry;
        Checksum = checksum;
    }

    #endregion

    #region Logger

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    #endregion

    #region Private Properties

    private bool AddToHistory => Source != FileSource.History;

    #endregion

    #region Public Properties

    public FileType Type { get; }
    public FileSource Source { get; }
    public PatchFilePath PatchFilePath { get; }
    public HistoryFileEntry? HistoryEntry { get; }
    public PackagedResourceEntry? ResourceEntry { get; }
    public PackagedResourceChecksum? Checksum { get; }

    #endregion

    #region Private Methods

    private void AddAddedFileToHistory(PatcherFileChanges fileChanges, bool fileExists, Func<Stream> getCurrentFile)
    {
        PackagedResourceChecksum checksum = Checksum ?? throw new Exception("Missing resource checksum");

        // If the file was added previously we don't want to mark it as being replaced or else we'd 
        // be replacing the previously added file (which we know wasn't there originally). Instead
        // we keep it in the history as being added and thus overwrite the previously added file.
        // If the previously added file doesn't exist anymore that doesn't matter either.
        if (HistoryEntry?.Type is HistoryFileType.Add)
        {
            fileChanges.AddAddedFile(PatchFilePath, checksum);
        }
        // If the file was previously replaced or removed then the history has the file which was
        // originally removed. We want to keep this file in the history as this is the file we want
        // to add back when the patch is reverted. We copy this from the old history to the new
        // history and set it as being replaced. If a file now exists there or not doesn't matter.
        else if (HistoryEntry?.Type is HistoryFileType.Replace or HistoryFileType.Remove)
        {
            PackagedResourceEntry historyResourceEntry = HistoryEntry?.ResourceEntry ?? throw new Exception("Missing history resource entry");

            fileChanges.AddReplacedFile(PatchFilePath, checksum, historyResourceEntry);
        }
        // If the file was not previously modified by a patch we can store it in the history as
        // normal. If the file exists it gets replaced and we keep the original file. If the file
        // doesn't exist we just mark it as being added.
        else
        {
            if (fileExists)
            {
                using Stream fileStream = getCurrentFile();
                fileChanges.AddReplacedFile(PatchFilePath, checksum, fileChanges.CreateResourceEntry(fileStream));
            }
            else
            {
                fileChanges.AddAddedFile(PatchFilePath, checksum);
            }
        }
    }

    private void AddRemovedFileToHistory(PatcherFileChanges fileChanges, bool fileExists, Func<Stream> getCurrentFile)
    {
        // If the file was previously added we don't want to do anything. This means that the file
        // which currently exists there was added (not replaced) by a previous patch and we don't
        // have anything to save in the history. We don't want to mark it as being removed either
        // as there was no file there originally to remove (and thus no file to restore).
        if (HistoryEntry?.Type is HistoryFileType.Add)
        {
            // Do nothing
        }
        // If the file was previously replaced or removed that means there was a file there
        // originally. It is this file we want to mark as being removed and once again save
        // in the history. So we copy it from the old history to the new one and mark it as
        // being removed.
        else if (HistoryEntry?.Type is HistoryFileType.Replace or HistoryFileType.Remove)
        {
            PackagedResourceEntry historyResourceEntry = HistoryEntry?.ResourceEntry ?? throw new Exception("Missing history resource entry");

            fileChanges.AddRemovedFile(PatchFilePath, historyResourceEntry);
        }
        // If the file was not previously modified by a patch and it exists then we save it
        // as being removed. If it does not exist then there's nothing to save as we can't
        // remove a file which doesn't exist.
        else if (fileExists)
        {
            using Stream fileStream = getCurrentFile();
            fileChanges.AddRemovedFile(PatchFilePath, fileChanges.CreateResourceEntry(fileStream));
        }
    }

    #endregion

    #region Public Methods

    public void ProcessFile(
        PatcherFileChanges fileChanges,
        bool fileExists,
        Func<Stream> getCurrentFile,
        Action<Stream> addCurrentFile,
        Action deleteFile)
    {
        if (Type == FileType.Add)
        {
            if (fileExists)
                Logger.Trace("Replacing file {0}", PatchFilePath);
            else
                Logger.Trace("Adding file {0}", PatchFilePath);

            // Optionally add the file to the history
            if (AddToHistory)
                AddAddedFileToHistory(fileChanges, fileExists, getCurrentFile);

            // Add/replace the file
            PackagedResourceEntry resourceEntry = ResourceEntry ?? throw new Exception("Missing resource entry");
            using Stream resource = resourceEntry.ReadData(fileChanges.Context, true);
            addCurrentFile(resource);
        }
        else if (Type == FileType.Remove)
        {
            Logger.Trace("Removing file {0}", PatchFilePath);

            // Optionally add the file to the history
            if (AddToHistory)
                AddRemovedFileToHistory(fileChanges, fileExists, getCurrentFile);

            // Remove the file
            deleteFile();
        }
        else
        {
            throw new Exception($"The file modification type {Type} is not supported");
        }
    }

    #endregion

    #region Data Types

    public record HistoryFileEntry(HistoryFileType Type, PackagedResourceEntry? ResourceEntry = null);

    public enum FileType { Add, Remove, }
    public enum FileSource { History, Patch, }
    public enum HistoryFileType { Add, Replace, Remove, }

    #endregion
}