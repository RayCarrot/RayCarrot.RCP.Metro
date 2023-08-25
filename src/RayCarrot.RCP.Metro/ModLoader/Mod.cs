using System.IO;
using System.Windows.Media.Imaging;
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

        // Get versions
        _validVersions = new HashSet<string> { DefaultVersion };
        foreach (FileSystemPath subDir in Directory.GetDirectories(modDirectoryPath))
            _validVersions.Add(subDir.Name);

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
    public const string DefaultVersion = "default";

    #endregion

    #region Private Fields

    private readonly Dictionary<string, List<IModFileResource>> _addedFiles;
    private readonly Dictionary<string, List<ModFilePath>> _removedFiles;
    private readonly HashSet<string> _validVersions;

    #endregion

    #region Public Properties

    public FileSystemPath ModDirectoryPath { get; }
    public ModMetadata Metadata { get; }
    public string[] Versions => _validVersions.ToArray();

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

    private FileSystemPath GetFilesPath(string version)
    {
        return ModDirectoryPath + version + FilesDirectoryName;
    }

    private FileSystemPath GetRemovedFilesFilePath(string version)
    {
        return ModDirectoryPath + version + RemovedFilesFileName;
    }

    private void VerifyVersion(string version)
    {
        if (!_validVersions.Contains(version))
            throw new InvalidOperationException($"The requested version {version} is undefined for this mod");
    }

    private Dictionary<string, List<IModFileResource>> CreateAddedFilesTable()
    {
        Dictionary<string, List<IModFileResource>> fileTable = new();

        foreach (string version in _validVersions)
        {
            List<IModFileResource> fileResources = new();
            fileTable[version] = fileResources;

            FileSystemPath filesPath = GetFilesPath(version);

            if (!filesPath.DirectoryExists) 
                continue;

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
        }

        return fileTable;
    }

    private Dictionary<string, List<ModFilePath>> CreateRemovedFilesTable()
    {
        Dictionary<string, List<ModFilePath>> fileTable = new();

        foreach (string version in _validVersions)
        {
            List<ModFilePath> filePaths = new();
            fileTable[version] = filePaths;

            FileSystemPath removedFilesFilePath = GetRemovedFilesFilePath(version);

            if (!removedFilesFilePath.FileExists)
                continue;

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
        }

        return fileTable;
    }

    #endregion

    #region Public Methods

    public IEnumerable<IModFileResource> GetAddedFiles(string version)
    {
        VerifyVersion(version);

        if (version == DefaultVersion)
        {
            return _addedFiles[DefaultVersion];
        }
        else
        {
            return _addedFiles[DefaultVersion].Concat(_addedFiles[version]);
        }
    }

    public IEnumerable<ModFilePath> GetRemovedFiles(string version)
    {
        VerifyVersion(version);

        if (version == DefaultVersion)
        {
            return _removedFiles[DefaultVersion];
        }
        else
        {
            return _removedFiles[DefaultVersion].Concat(_removedFiles[version]);
        }
    }

    public BitmapImage? GetThumbnail()
    {
        FileSystemPath thumbFilePath = ModDirectoryPath + ThumbnailFileName;

        if (!thumbFilePath.FileExists)
            return null;

        return new BitmapImage(new Uri(thumbFilePath));
    }

    #endregion
}