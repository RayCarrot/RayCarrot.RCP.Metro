using RayCarrot.RCP.Metro.ModLoader.Resource;

namespace RayCarrot.RCP.Metro.ModLoader;

// Why is the code for adding a file to the history so confusing? Well because we're trying to combine two steps into one! Ideally
// when you apply new mods it should do this:
// 1. Revert old changes
// 2. Apply new changes
// However this is rather inefficient. A lot of the time it will revert and then apply the same changes. So instead we try and combine
// them into one step. This is the reason for all the additional case checks we have to perform to avoid messing up the history.

public class FileModification
{
    #region Constructor

    public FileModification(
        FileType type,
        FileSource source,
        ModFilePath modFilePath,
        HistoryFileEntry? historyEntry = null,
        IModFileResource? resourceEntry = null)
    {
        Type = type;
        Source = source;
        ModFilePath = modFilePath;
        HistoryEntry = historyEntry;
        ResourceEntry = resourceEntry;
    }

    #endregion

    #region Logger

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    #endregion

    #region Private Properties

    private bool AddToHistory => Source != FileSource.History;
    private List<IFilePatch>? FilePatches { get; set; }

    #endregion

    #region Public Properties

    public FileType Type { get; }
    public FileSource Source { get; }
    public ModFilePath ModFilePath { get; }
    public HistoryFileEntry? HistoryEntry { get; }
    public IModFileResource? ResourceEntry { get; }

    #endregion

    #region Private Methods

    private void AddAddedFileToHistory(LibraryFileHistoryBuilder historyBuilder, bool fileExists, FileModificationStream fileStream)
    {
        // If the file was added previously we don't want to mark it as being replaced or else we'd 
        // be replacing the previously added file (which we know wasn't there originally). Instead
        // we keep it in the history as being added and thus overwrite the previously added file.
        // If the previously added file doesn't exist anymore that doesn't matter either.
        if (HistoryEntry?.Type is HistoryFileType.Add)
        {
            historyBuilder.AddAddedFile(ModFilePath);
        }
        // If the file was previously replaced or removed then the history has the file which was
        // originally removed. We want to keep this file in the history as this is the file we want
        // to add back when the mod is reverted. We copy this from the old history to the new
        // history and set it as being replaced. If a file now exists there or not doesn't matter.
        else if (HistoryEntry?.Type is HistoryFileType.Replace or HistoryFileType.Remove)
        {
            IModFileResource historyResourceEntry = HistoryEntry?.ResourceEntry ?? throw new Exception("Missing history resource entry");

            historyBuilder.AddReplacedFile(ModFilePath, historyResourceEntry);
        }
        // If the file was not previously modified by a mod we can store it in the history as
        // normal. If the file exists it gets replaced and we keep the original file. If the file
        // doesn't exist we just mark it as being added.
        else
        {
            if (fileExists)
            {
                historyBuilder.AddReplacedFile(ModFilePath, new VirtualModFileResource(ModFilePath, fileStream.Stream));
            }
            else
            {
                historyBuilder.AddAddedFile(ModFilePath);
            }
        }
    }

    private void AddRemovedFileToHistory(LibraryFileHistoryBuilder historyBuilder, bool fileExists, FileModificationStream fileStream)
    {
        // If the file was previously added we don't want to do anything. This means that the file
        // which currently exists there was added (not replaced) by a previous mod and we don't
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
            IModFileResource historyResourceEntry = HistoryEntry?.ResourceEntry ?? throw new Exception("Missing history resource entry");

            historyBuilder.AddRemovedFile(ModFilePath, historyResourceEntry);
        }
        // If the file was not previously modified by a mod and it exists then we save it
        // as being removed. If it does not exist then there's nothing to save as we can't
        // remove a file which doesn't exist.
        else if (fileExists)
        {
            historyBuilder.AddRemovedFile(ModFilePath, new VirtualModFileResource(ModFilePath, fileStream.Stream));
        }
    }

    private void ProcessFilePatches(LibraryFileHistoryBuilder historyBuilder, FileModificationStream fileStream)
    {
        // Ignore if there are no patches
        if (FilePatches == null || !FilePatches.Any())
            return;

        // If not already added to history (might have happened if file was added), then manually add here before applying patches
        if (!historyBuilder.HasAddedFile(ModFilePath))
            historyBuilder.AddReplacedFile(ModFilePath, new VirtualModFileResource(ModFilePath, fileStream.Stream));

        foreach (IFilePatch filePatch in FilePatches)
        {
            filePatch.PatchFile(fileStream.Stream);
        }
    }

    #endregion

    #region Public Methods

    public void AddFilePatch(IFilePatch filePatch)
    {
        // If the modification is to remove the file then there's no point in patching it
        if (Type == FileType.Remove)
            return;

        FilePatches ??= new List<IFilePatch>();

        // Add the patch
        FilePatches.Add(filePatch);
    }

    public bool HasFilePatches() => FilePatches?.Any() == true;

    public void ProcessFile(
        LibraryFileHistoryBuilder historyBuilder,
        bool fileExists,
        FileModificationStream fileStream)
    {
        if (Type == FileType.PatchOnly)
        {
            // The will happen if a file has patches to be applied to it but is not set to be added or removed
        }
        else if (Type == FileType.Add)
        {
            if (fileExists)
                Logger.Trace("Replacing file {0}", ModFilePath);
            else
                Logger.Trace("Adding file {0}", ModFilePath);

            // Optionally add the file to the history
            if (AddToHistory)
                AddAddedFileToHistory(historyBuilder, fileExists, fileStream);

            // Add/replace the file
            IModFileResource resourceEntry = ResourceEntry ?? throw new Exception("Missing resource entry");
            fileStream.Stream.SetLength(0);
            resourceEntry.CopyToStream(fileStream.Stream);

            fileExists = true;
        }
        else if (Type == FileType.Remove)
        {
            Logger.Trace("Removing file {0}", ModFilePath);

            // Optionally add the file to the history
            if (AddToHistory)
                AddRemovedFileToHistory(historyBuilder, fileExists, fileStream);

            // Remove the file
            fileStream.DeleteFile();

            fileExists = false;
        }
        else
        {
            throw new Exception($"The file modification type {Type} is not supported");
        }

        if (fileExists)
            ProcessFilePatches(historyBuilder, fileStream);
    }

    #endregion

    #region Data Types

    public record HistoryFileEntry(HistoryFileType Type, IModFileResource? ResourceEntry = null);

    public enum FileType { PatchOnly, Add, Remove }
    public enum FileSource { History, Mod, }
    public enum HistoryFileType { Add, Replace, Remove, }

    #endregion
}