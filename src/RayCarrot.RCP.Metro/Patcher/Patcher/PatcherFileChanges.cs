using System;
using System.Collections.Generic;
using System.IO;
using BinarySerializer;
using NLog;

namespace RayCarrot.RCP.Metro.Patcher;

public class PatcherFileChanges : IDisposable
{
    public PatcherFileChanges(Context context)
    {
        Context = context;
    }

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    private List<TempFile> TempFiles { get; } = new();

    private List<PatchFilePath> AddedFiles { get; } = new();
    private List<PackagedResourceChecksum> AddedFileChecksums { get; } = new();
    private List<PatchFilePath> ReplacedFiles { get; } = new();
    private List<PackagedResourceChecksum> ReplacedFileChecksums { get; } = new();
    private List<PackagedResourceEntry> ReplacedFileResources { get; } = new();
    private List<PatchFilePath> RemovedFiles { get; } = new();
    private List<PackagedResourceEntry> RemovedFileResources { get; } = new();
    private HashSet<string> DirsToDeleteIfEmpty { get; } = new();

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

    public void DeleteDirectoryIfEmpty(string dirPath)
    {
        DirsToDeleteIfEmpty.Add(dirPath);
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

    public void Flush()
    {
        foreach (string dirPath in DirsToDeleteIfEmpty)
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

        DirsToDeleteIfEmpty.Clear();
    }

    public void Dispose()
    {
        TempFiles.DisposeAll();
        Flush();
    }
}