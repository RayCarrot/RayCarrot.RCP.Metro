using System.Diagnostics;
using System.IO;
using RayCarrot.RCP.Metro.Archive;
using RayCarrot.RCP.Metro.ModLoader.Resource;

namespace RayCarrot.RCP.Metro.ModLoader;

public class FileLocationModifications
{
    #region Constructor

    public FileLocationModifications(string location, IArchiveDataManager? archiveDataManager)
    {
        Location = location;
        ArchiveDataManager = archiveDataManager;
    }

    #endregion

    #region Logger

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    #endregion

    #region Private Fields

    private readonly Dictionary<string, FileModification> _fileModifications = new();

    #endregion

    #region Public Properties

    public string Location { get; }
    public IArchiveDataManager? ArchiveDataManager { get; }
    public int FilesCount => _fileModifications.Count;

    #endregion

    #region Private Methods

    private void ModifyPhysicalFiles(
        LibraryFileHistoryBuilder historyBuilder,
        FileSystemPath dirPath,
        Action<Progress>? progressCallback)
    {
        int fileIndex = 0;

        // Process each file modification
        foreach (FileModification modification in _fileModifications.Values)
        {
            ModFilePath modFilePath = modification.ModFilePath;
            FileSystemPath physicalFilePath = dirPath + modFilePath.FilePath;

            // Make sure the file is inside of the directory. We want to disallow going up the tree using ..\
            if (!Path.GetFullPath(physicalFilePath).StartsWith(dirPath))
            {
                Logger.Warn("File modification with path {0} is not valid", modFilePath.FilePath);
                continue;
            }

            modification.ProcessFile(
                historyBuilder: historyBuilder,
                fileExists: physicalFilePath.FileExists,
                getCurrentFile: () => File.OpenRead(physicalFilePath),
                addCurrentFile: x => ReplacePhysicalFile(physicalFilePath, x),
                deleteFile: () =>
                {
                    // Delete the file
                    physicalFilePath.DeleteFile();

                    // Delete the directory if it's empty
                    historyBuilder.DeleteDirectoryIfEmpty(physicalFilePath.Parent);
                });

            fileIndex++;
            progressCallback?.Invoke(new Progress(fileIndex, _fileModifications.Count));
        }
    }

    private void ReplacePhysicalFile(FileSystemPath filePath, Stream resource)
    {
        Directory.CreateDirectory(filePath.Parent);
        using Stream fileStream = File.OpenWrite(filePath);
        fileStream.SetLength(0);
        resource.CopyTo(fileStream);
    }

    private void ModifyArchive(
        LibraryFileHistoryBuilder historyBuilder,
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

                int initialModificationsCount = _fileModifications.Count;
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

                        string filePathKey = GetFilePathKey(manager.CombinePaths(file.Directory, file.FileName));

                        FileModification? modification = _fileModifications.TryGetValue(filePathKey);

                        if (modification == null)
                        {
                            // Keep the file and do nothing else
                            archiveFiles.Add(file);
                            continue;
                        }

                        _fileModifications.Remove(filePathKey);

                        modification.ProcessFile(
                            historyBuilder: historyBuilder,
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

                totalFilesIndex += initialModificationsCount - _fileModifications.Count;

                // Add files not already in the archive
                foreach (FileModification modification in _fileModifications.Values)
                {
                    progressCallback?.Invoke(new Progress(totalFilesIndex, totalFilesMax));
                    totalFilesIndex++;

                    modification.ProcessFile(
                        historyBuilder: historyBuilder,
                        fileExists: false,
                        getCurrentFile: () => throw new InvalidOperationException("Can't get a file which doesn't exist"),
                        addCurrentFile: x =>
                        {
                            // Get the file name and directory
                            string fileName = Path.GetFileName(modification.ModFilePath.FilePath);
                            string dir = Path.GetDirectoryName(modification.ModFilePath.FilePath)?.
                                Replace(Path.DirectorySeparatorChar, manager.PathSeparatorCharacter) ?? String.Empty;

                            // Create a new archive entry
                            object entry = manager.GetNewFileEntry(archive, dir, fileName);

                            Logger.Trace("Adding file {0}", modification.ModFilePath);

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

                Logger.Info("Processed modified archive files in {0} ms", sw.ElapsedMilliseconds);

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

                Logger.Info("Repacked modified archive files in {0} ms", sw.ElapsedMilliseconds);
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

    private void ReplaceArchiveFile(FileItem file, IArchiveDataManager manager, Stream resource)
    {
        Logger.Trace("Replacing archive file {0}/{1}", file.Directory, file.FileName);

        // Get the temp stream to store the pending import data
        file.SetPendingImport();

        // Encode the data to the pending import stream
        manager.EncodeFile(resource, file.PendingImport, file.ArchiveEntry,
            // TODO-UPDATE: We should be able to access the metadata now that we're mainly dealing with physical files
            new FileMetadata());

        // If no data was encoded we copy over the decoded data
        if (file.PendingImport.Length == 0)
            resource.CopyTo(file.PendingImport);
    }

    private string GetFilePathKey(string filePath) => ModLoaderHelpers.NormalizePath(filePath);

    #endregion

    #region Public Methods

    public void AddFileModification(
        FileModification.FileType type,
        FileModification.FileSource source,
        ModFilePath modFilePath,
        FileModification.HistoryFileEntry? historyEntry = null,
        IModFileResource? resourceEntry = null)
    {
        string filePathKey = GetFilePathKey(modFilePath.FilePath);

        // If this modification overrides an existing one we want to copy over the history entry
        if (historyEntry == null &&
            _fileModifications.TryGetValue(filePathKey, out FileModification? existingModification))
        {
            historyEntry = existingModification.HistoryEntry;
        }

        _fileModifications[filePathKey] = new FileModification(
            type: type,
            source: source,
            modFilePath: modFilePath,
            historyEntry: historyEntry,
            resourceEntry: resourceEntry);
    }

    public void ApplyModifications(LibraryFileHistoryBuilder historyBuilder, FileSystemPath gameDir, Action<Progress>? progressCallback)
    {
        if (Location == String.Empty)
        {
            ModifyPhysicalFiles(
                historyBuilder: historyBuilder,
                dirPath: gameDir,
                progressCallback: progressCallback);
        }
        else
        {
            IArchiveDataManager manager = ArchiveDataManager ?? throw new Exception($"No archive data manager for location {Location}");

            ModifyArchive(
                historyBuilder: historyBuilder,
                archiveFilePath: gameDir + Location,
                manager: manager,
                progressCallback: progressCallback);
        }
    }

    #endregion
}