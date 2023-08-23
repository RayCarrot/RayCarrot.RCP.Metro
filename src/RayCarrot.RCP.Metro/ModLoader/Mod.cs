using System.IO;
using System.Windows.Media.Imaging;
using RayCarrot.RCP.Metro.ModLoader.FileInfo;
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
        FilesInfo = ReadFilesInfo();

        // Get valid versions
        _validVersions = new List<string> { DefaultVersion };
        if (FilesInfo.Versions != null)
            _validVersions.AddRange(FilesInfo.Versions);

        // Create file tables
        _addedFiles = CreateAddedFilesTable();
        _removedFiles = CreateRemovedFilesTable();
    }

    #endregion

    #region Constants

    public const string MetadataFileName = "metadata.jsonc";
    public const string FilesInfoFileName = "files.jsonc";
    public const string ThumbnailFileName = "thumbnail.png";
    public const string FilesDirectoryName = "files";

    public const int LatestFormatVersion = 0;
    public const string DefaultVersion = "default";

    #endregion

    #region Private Fields

    private readonly Dictionary<string, List<IModFileResource>> _addedFiles;
    private readonly Dictionary<string, List<ModFilePath>> _removedFiles;
    private readonly List<string> _validVersions;

    #endregion

    #region Public Properties

    public FileSystemPath ModDirectoryPath { get; }
    public ModMetadata Metadata { get; }
    public ModFilesInfo FilesInfo { get; }
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

    private ModFilesInfo ReadFilesInfo()
    {
        FileSystemPath filesInfoFilePath = ModDirectoryPath + FilesInfoFileName;

        // This file is optional, so we return an empty instance if it doesn't exist
        if (!filesInfoFilePath.FileExists)
            return new ModFilesInfo(null, null, null);

        ModFilesInfo filesInfo;

        try
        {
            filesInfo = JsonHelpers.DeserializeFromFile<ModFilesInfo>(filesInfoFilePath);
        }
        catch (Exception ex)
        {
            throw new InvalidModException($"The mod files info file is invalid. {ex.Message}", ex);
        }

        return filesInfo;
    }

    private FileSystemPath GetFilesPath(string version)
    {
        return ModDirectoryPath + FilesDirectoryName + version;
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

            if (FilesInfo.Archives == null || FilesInfo.Archives.Length == 0)
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

                    foreach (ModArchiveInfo archive in FilesInfo.Archives)
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
        Dictionary<string, List<ModFilePath>> fileTable = _validVersions.ToDictionary(x => x, _ => new List<ModFilePath>());

        if (FilesInfo.RemovedFiles == null)
            return fileTable;

        if (FilesInfo.Archives == null || FilesInfo.Archives.Length == 0)
        {
            foreach (string version in FilesInfo.RemovedFiles.Keys)
            {
                fileTable[version].AddRange(FilesInfo.RemovedFiles[version].Select(x => new ModFilePath(x)));
            }
        }
        else
        {
            foreach (string version in FilesInfo.RemovedFiles.Keys)
            {
                List<ModFilePath> fileList = fileTable[version];

                foreach (string removedFile in FilesInfo.RemovedFiles[version])
                {
                    bool inArchive = false;

                    foreach (ModArchiveInfo archive in FilesInfo.Archives)
                    {
                        if (removedFile.StartsWith(archive.FilePath))
                        {
                            fileList.Add(new ModFilePath(removedFile.Substring(archive.FilePath.Length + 1), archive.FilePath, archive.Id));
                            inArchive = true;
                            break;
                        }
                    }

                    if (!inArchive)
                        fileList.Add(new ModFilePath(removedFile));
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