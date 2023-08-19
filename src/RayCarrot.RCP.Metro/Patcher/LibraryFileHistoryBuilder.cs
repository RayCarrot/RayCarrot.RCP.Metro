using System.IO;
using RayCarrot.RCP.Metro.Patcher.Library;
using RayCarrot.RCP.Metro.Patcher.Resource;

namespace RayCarrot.RCP.Metro.Patcher;

public class LibraryFileHistoryBuilder : IDisposable
{
    #region Logger

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    #endregion

    #region Private Fields

    private readonly TempDirectory _tempDirectory = new(true);
    private readonly HashSet<string> _dirsToDeleteIfEmpty = new();

    private readonly List<PatchFilePath> _addedFiles = new();
    private readonly List<PatchFilePath> _replacedFiles = new();
    private readonly List<PatchFilePath> _removedFiles = new();

    #endregion

    #region Private Methods

    private void AddFileToHistory(PatchFilePath filePath, IPatchFileResource resource)
    {
        FileSystemPath destFilePath = _tempDirectory.TempPath + filePath.FullFilePath;

        // If the resource is from the previous history then we can just move the file. The old
        // history will anyway be overwritten, so no need to copy it. The only potential issue
        // with this is that your file history will be incomplete/corrupt if the process fails.
        // However that will probably cause other issues anyway, so we hope that doesn't happen...
        if (resource is LibraryHistoryPatchFileResource libraryHistoryPatchFileResource)
        {
            Services.File.MoveFile(libraryHistoryPatchFileResource.FilePath, destFilePath, true);
        }
        else
        {
            Directory.CreateDirectory(destFilePath.Parent);

            using Stream resourceStream = resource.Read();
            using Stream destStream = File.Create(destFilePath);
            resourceStream.CopyTo(destStream);
        }
    }

    private void DeleteEmptyDirectories()
    {
        foreach (string dirPath in _dirsToDeleteIfEmpty)
        {
            try
            {
                if (!Directory.Exists(dirPath))
                    continue;

                string? currentDirPath = dirPath;

                while (true)
                {
                    bool isEmpty = !Directory.EnumerateFileSystemEntries(currentDirPath, "*", SearchOption.AllDirectories).Any();

                    if (!isEmpty)
                        break;

                    Directory.Delete(currentDirPath);
                    currentDirPath = Path.GetDirectoryName(currentDirPath);

                    if (currentDirPath == null)
                        break;
                }
            }
            catch (Exception ex)
            {
                Logger.Warn(ex, "Removing empty directories");
            }
        }

        _dirsToDeleteIfEmpty.Clear();
    }

    #endregion

    #region Public Methods

    public void AddAddedFile(PatchFilePath filePath)
    {
        _addedFiles.Add(filePath);
    }

    public void AddReplacedFile(PatchFilePath filePath, IPatchFileResource resource)
    {
        _replacedFiles.Add(filePath);
        AddFileToHistory(filePath, resource);
    }

    public void AddRemovedFile(PatchFilePath filePath, IPatchFileResource resource)
    {
        _removedFiles.Add(filePath);
        AddFileToHistory(filePath, resource);
    }

    // TODO: This feels a bit out of place in this class, but I don't know where
    //       else to place it without making things more convoluted...
    public void DeleteDirectoryIfEmpty(string dirPath)
    {
        _dirsToDeleteIfEmpty.Add(dirPath);
    }

    public void BuildFileHistory(PatchLibrary library)
    {
        // Replace old file history with newly created one
        library.ReplaceFileHistory(_tempDirectory.TempPath, false);

        // Write out the history
        library.WriteHistory(new LibraryFileHistory(
            AddedFiles: _addedFiles.ToArray(),
            ReplacedFiles: _replacedFiles.ToArray(),
            RemovedFiles: _removedFiles.ToArray()));

        // Delete empty directories
        DeleteEmptyDirectories();
    }

    public void Dispose()
    {
        _tempDirectory.Dispose();
    }

    #endregion
}