using System.IO;
using RayCarrot.RCP.Metro.ModLoader.Library;
using RayCarrot.RCP.Metro.ModLoader.Resource;

namespace RayCarrot.RCP.Metro.ModLoader;

public class LibraryFileHistoryBuilder : IDisposable
{
    #region Logger

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    #endregion

    #region Private Fields

    private readonly TempDirectory _tempDirectory = new(true);
    private readonly HashSet<string> _dirsToDeleteIfEmpty = new();

    private readonly HashSet<string> _files = new();

    private readonly List<ModFilePath> _addedFiles = new();
    private readonly List<ModFilePath> _replacedFiles = new();
    private readonly List<ModFilePath> _removedFiles = new();

    #endregion

    #region Private Methods

    private void AddFileToHistory(ModFilePath filePath, IModFileResource resource)
    {
        FileSystemPath destFilePath = _tempDirectory.TempPath + filePath.FullFilePath;

        // If the resource is from the previous history then we can just move the file. The old
        // history will anyway be overwritten, so no need to copy it. The only potential issue
        // with this is that your file history will be incomplete/corrupt if the process fails.
        // However that will probably cause other issues anyway, so we hope that doesn't happen...
        if (resource is LibraryHistoryModFileResource libraryHistoryModFileResource)
        {
            Services.File.MoveFile(libraryHistoryModFileResource.FilePath, destFilePath, true);
        }
        else
        {
            Directory.CreateDirectory(destFilePath.Parent);

            using Stream destStream = File.Create(destFilePath);
            resource.CopyToStream(destStream);
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

    public void AddAddedFile(ModFilePath filePath)
    {
        _addedFiles.Add(filePath);
        _files.Add(ModLoaderHelpers.NormalizePath(filePath.FullFilePath));
    }

    public void AddReplacedFile(ModFilePath filePath, IModFileResource resource)
    {
        _replacedFiles.Add(filePath);
        _files.Add(ModLoaderHelpers.NormalizePath(filePath.FullFilePath));
        AddFileToHistory(filePath, resource);
    }

    public void AddRemovedFile(ModFilePath filePath, IModFileResource resource)
    {
        _removedFiles.Add(filePath);
        _files.Add(ModLoaderHelpers.NormalizePath(filePath.FullFilePath));
        AddFileToHistory(filePath, resource);
    }

    public bool HasAddedFile(ModFilePath filePath)
    {
        return _files.Contains(ModLoaderHelpers.NormalizePath(filePath.FullFilePath));
    }

    // TODO: This feels a bit out of place in this class, but I don't know where
    //       else to place it without making things more convoluted...
    public void DeleteDirectoryIfEmpty(string dirPath)
    {
        _dirsToDeleteIfEmpty.Add(dirPath);
    }

    public void BuildFileHistory(ModLibrary library)
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