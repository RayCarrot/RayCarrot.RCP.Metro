using System.IO;
using System.Windows.Media.Imaging;
using RayCarrot.RCP.Metro.Patcher.FileInfo;
using RayCarrot.RCP.Metro.Patcher.Metadata;
using RayCarrot.RCP.Metro.Patcher.Resource;

namespace RayCarrot.RCP.Metro.Patcher;

public class InstalledPatch
{
    #region Constructor

    public InstalledPatch(FileSystemPath patchDirectoryPath)
    {
        PatchDirectoryPath = patchDirectoryPath;

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

    private const int LatestFormatVersion = 0;

    private const string MetadataFileName = "metadata.jsonc";
    private const string FilesInfoFileName = "files.jsonc";
    private const string ThumbnailFileName = "thumbnail.png";
    private const string FilesDirectoryName = "thumbnail.png";

    public const string DefaultVersion = "default";

    #endregion

    #region Private Fields

    private readonly Dictionary<string, List<IPatchFileResource>> _addedFiles;
    private readonly Dictionary<string, List<PatchFilePath>> _removedFiles;
    private readonly List<string> _validVersions;

    #endregion

    #region Public Properties

    public FileSystemPath PatchDirectoryPath { get; }
    public PatchMetadata Metadata { get; }
    public PatchFilesInfo FilesInfo { get; }

    #endregion

    #region Private Methods

    private PatchMetadata ReadMetadata()
    {
        FileSystemPath metadataFilePath = PatchDirectoryPath + MetadataFileName;

        if (!metadataFilePath.FileExists)
            throw new InvalidPatchException("The patch does not contain a metadata file");

        PatchMetadata metadata;

        try
        {
            metadata = JsonHelpers.DeserializeFromFile<PatchMetadata>(metadataFilePath);
        }
        catch (Exception ex)
        {
            throw new InvalidPatchException($"The patch metadata file is invalid. {ex.Message}", ex);
        }

        if (metadata.Format > LatestFormatVersion)
            throw new UnsupportedPatchFormatException($"Format version {metadata.Format} is higher than the latest supported version {LatestFormatVersion}");

        return metadata;
    }

    private PatchFilesInfo ReadFilesInfo()
    {
        FileSystemPath filesInfoFilePath = PatchDirectoryPath + FilesInfoFileName;

        // This file is optional, so we return an empty instance if it doesn't exist
        if (!filesInfoFilePath.FileExists)
            return new PatchFilesInfo(null, null, null);

        PatchFilesInfo filesInfo;

        try
        {
            filesInfo = JsonHelpers.DeserializeFromFile<PatchFilesInfo>(filesInfoFilePath);
        }
        catch (Exception ex)
        {
            throw new InvalidPatchException($"The patch files info file is invalid. {ex.Message}", ex);
        }

        return filesInfo;
    }

    private FileSystemPath GetFilesPath(string version)
    {
        return PatchDirectoryPath + FilesDirectoryName + version;
    }

    private void VerifyVersion(string version)
    {
        if (!_validVersions.Contains(version))
            throw new InvalidOperationException($"The requested version {version} is undefined for this patch");
    }

    private Dictionary<string, List<IPatchFileResource>> CreateAddedFilesTable()
    {
        Dictionary<string, List<IPatchFileResource>> fileTable = new();

        foreach (string version in _validVersions)
        {
            List<IPatchFileResource> fileResources = new();
            fileTable[version] = fileResources;

            FileSystemPath filesPath = GetFilesPath(version);

            if (!filesPath.DirectoryExists) 
                continue;

            if (FilesInfo.Archives == null || FilesInfo.Archives.Length == 0)
            {
                foreach (FileSystemPath file in Directory.EnumerateFiles(filesPath, "*", SearchOption.AllDirectories))
                {
                    PatchFilePath patchFilePath = new(file - filesPath);
                    fileResources.Add(new PhysicalPatchFileResource(patchFilePath, file));
                }
            }
            else
            {
                foreach (FileSystemPath file in Directory.EnumerateFiles(filesPath, "*", SearchOption.AllDirectories))
                {
                    string relativeFilePath = file - filesPath;

                    bool inArchive = false;

                    foreach (PatchArchiveInfo archive in FilesInfo.Archives)
                    {
                        if (relativeFilePath.StartsWith(archive.FilePath))
                        {
                            PatchFilePath patchFilePath = new(relativeFilePath.Substring(archive.FilePath.Length + 1), archive.FilePath, archive.Id);
                            fileResources.Add(new PhysicalPatchFileResource(patchFilePath, file));
                            inArchive = true;
                            break;
                        }
                    }

                    if (!inArchive)
                    {
                        PatchFilePath patchFilePath = new(relativeFilePath);
                        fileResources.Add(new PhysicalPatchFileResource(patchFilePath, file));
                    }
                }
            }
        }

        return fileTable;
    }

    private Dictionary<string, List<PatchFilePath>> CreateRemovedFilesTable()
    {
        Dictionary<string, List<PatchFilePath>> fileTable = _validVersions.ToDictionary(x => x, _ => new List<PatchFilePath>());

        if (FilesInfo.RemovedFiles == null)
            return fileTable;

        if (FilesInfo.Archives == null || FilesInfo.Archives.Length == 0)
        {
            foreach (string version in FilesInfo.RemovedFiles.Keys)
            {
                fileTable[version].AddRange(FilesInfo.RemovedFiles[version].Select(x => new PatchFilePath(x)));
            }
        }
        else
        {
            foreach (string version in FilesInfo.RemovedFiles.Keys)
            {
                List<PatchFilePath> fileList = fileTable[version];

                foreach (string removedFile in FilesInfo.RemovedFiles[version])
                {
                    bool inArchive = false;

                    foreach (PatchArchiveInfo archive in FilesInfo.Archives)
                    {
                        if (removedFile.StartsWith(archive.FilePath))
                        {
                            fileList.Add(new PatchFilePath(removedFile.Substring(archive.FilePath.Length + 1), archive.FilePath, archive.Id));
                            inArchive = true;
                            break;
                        }
                    }

                    if (!inArchive)
                        fileList.Add(new PatchFilePath(removedFile));
                }
            }
        }

        return fileTable;
    }

    #endregion

    #region Public Methods

    public IEnumerable<IPatchFileResource> GetAddedFiles(string version)
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

    public IEnumerable<PatchFilePath> GetRemovedFiles(string version)
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
        FileSystemPath thumbFilePath = PatchDirectoryPath + ThumbnailFileName;

        if (!thumbFilePath.FileExists)
            return null;

        return new BitmapImage(new Uri(thumbFilePath));
    }

    #endregion
}