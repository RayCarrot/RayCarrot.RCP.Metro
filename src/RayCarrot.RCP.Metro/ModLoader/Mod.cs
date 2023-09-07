using System.IO;
using RayCarrot.RCP.Metro.ModLoader.Metadata;
using RayCarrot.RCP.Metro.ModLoader.Resource;

namespace RayCarrot.RCP.Metro.ModLoader;

public class Mod
{
    #region Constructor

    public Mod(FileSystemPath modDirectoryPath)
    {
        ModDirectoryPath = modDirectoryPath;

        Metadata = ReadMetadata();

        // Create file tables
        _addedFiles = CreateAddedFilesTable();
        _removedFiles = CreateRemovedFilesTable();
    }

    #endregion

    #region Constants

    public const string MetadataFileName = "metadata.jsonc";
    public const string ThumbnailFileName = "thumbnail.png";
    public const string FilesDirectoryName = "files";
    public const string RemovedFilesFileName = "removed_files.txt";

    public const int LatestFormatVersion = 0;

    #endregion

    #region Private Fields

    private readonly List<IModFileResource> _addedFiles;
    private readonly List<ModFilePath> _removedFiles;

    #endregion

    #region Public Properties

    public FileSystemPath ModDirectoryPath { get; }
    public ModMetadata Metadata { get; }

    #endregion

    #region Private Methods

    private ModMetadata ReadMetadata()
    {
        FileSystemPath metadataFilePath = ModDirectoryPath + MetadataFileName;

        if (!metadataFilePath.FileExists)
            throw new InvalidModException("The mod does not contain a metadata file");

        ModMetadata metadata;

        try
        {
            metadata = JsonHelpers.DeserializeFromFile<ModMetadata>(metadataFilePath);
        }
        catch (Exception ex)
        {
            throw new InvalidModException($"The mod metadata file is invalid. {ex.Message}", ex);
        }

        if (metadata.Format > LatestFormatVersion)
            throw new UnsupportedModFormatException($"Format version {metadata.Format} is higher than the latest supported version {LatestFormatVersion}");

        return metadata;
    }

    private List<IModFileResource> CreateAddedFilesTable()
    {
        List<IModFileResource> fileResources = new();

        FileSystemPath filesPath = ModDirectoryPath + FilesDirectoryName;

        if (!filesPath.DirectoryExists)
            return fileResources;

        if (Metadata.Archives == null || Metadata.Archives.Length == 0)
        {
            foreach (FileSystemPath file in Directory.EnumerateFiles(filesPath, "*", SearchOption.AllDirectories))
            {
                ModFilePath modFilePath = new(file - filesPath);
                fileResources.Add(new PhysicalModFileResource(modFilePath, file));
            }
        }
        else
        {
            foreach (FileSystemPath file in Directory.EnumerateFiles(filesPath, "*", SearchOption.AllDirectories))
            {
                string relativeFilePath = file - filesPath;

                bool inArchive = false;

                foreach (ModArchiveInfo archive in Metadata.Archives)
                {
                    if (relativeFilePath.StartsWith(archive.FilePath))
                    {
                        ModFilePath modFilePath = new(relativeFilePath.Substring(archive.FilePath.Length + 1), archive.FilePath, archive.Id);
                        fileResources.Add(new PhysicalModFileResource(modFilePath, file));
                        inArchive = true;
                        break;
                    }
                }

                if (!inArchive)
                {
                    ModFilePath modFilePath = new(relativeFilePath);
                    fileResources.Add(new PhysicalModFileResource(modFilePath, file));
                }
            }
        }

        return fileResources;
    }
    private List<ModFilePath> CreateRemovedFilesTable()
    {
        List<ModFilePath> filePaths = new();

        FileSystemPath removedFilesFilePath = ModDirectoryPath + RemovedFilesFileName;

        if (!removedFilesFilePath.FileExists)
            return filePaths;

        string[] removedFiles = File.ReadAllLines(removedFilesFilePath);

        if (Metadata.Archives == null || Metadata.Archives.Length == 0)
        {
            filePaths.AddRange(removedFiles.Select(x => new ModFilePath(x)));
        }
        else
        {
            foreach (string removedFile in removedFiles)
            {
                bool inArchive = false;

                foreach (ModArchiveInfo archive in Metadata.Archives)
                {
                    if (removedFile.StartsWith(archive.FilePath))
                    {
                        filePaths.Add(new ModFilePath(removedFile.Substring(archive.FilePath.Length + 1), archive.FilePath, archive.Id));
                        inArchive = true;
                        break;
                    }
                }

                if (!inArchive)
                    filePaths.Add(new ModFilePath(removedFile));
            }
        }

        return filePaths;
    }

    #endregion

    #region Public Methods

    public ReadOnlyCollection<IModFileResource> GetAddedFiles() => _addedFiles.AsReadOnly();
    public ReadOnlyCollection<ModFilePath> GetRemovedFiles() => _removedFiles.AsReadOnly();

    public FileSystemPath? GetThumbnailFilePath()
    {
        FileSystemPath thumbFilePath = ModDirectoryPath + ThumbnailFileName;

        if (!thumbFilePath.FileExists)
            return null;

        return thumbFilePath;
    }

    #endregion
}