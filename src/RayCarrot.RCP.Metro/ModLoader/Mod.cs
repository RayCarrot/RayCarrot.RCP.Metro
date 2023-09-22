using System.IO;
using RayCarrot.RCP.Metro.ModLoader.Metadata;
using RayCarrot.RCP.Metro.ModLoader.Modules;
using RayCarrot.RCP.Metro.ModLoader.Resource;

namespace RayCarrot.RCP.Metro.ModLoader;

public class Mod
{
    #region Constructor

    public Mod(FileSystemPath modDirectoryPath, IReadOnlyCollection<ModModule> possibleModules)
    {
        ModDirectoryPath = modDirectoryPath;

        Metadata = ReadMetadata();

        // Get the modules used by the mod
        _supportedModules = new Dictionary<string, ModModule>();
        _unsupportedModules = new List<string>();
        foreach (FileSystemPath modDir in Directory.GetDirectories(modDirectoryPath))
        {
            string name = modDir.Name;
            ModModule? module = possibleModules.FirstOrDefault(x => x.Id == name);

            if (module == null)
                _unsupportedModules.Add(name);
            else
                _supportedModules[module.Id] = module;
        }

        // Create file tables
        _addedFiles = _supportedModules.Values.SelectMany(x => x.GetAddedFiles(this, GetModulePath(x))).ToList();
        _removedFiles = _supportedModules.Values.SelectMany(x => x.GetRemovedFiles(this, GetModulePath(x))).ToList();
    }

    #endregion

    #region Constants

    public const string MetadataFileName = "metadata.jsonc";
    public const string ThumbnailFileName = "thumbnail.png";

    public const int LatestFormatVersion = 0;

    #endregion

    #region Private Fields

    private readonly Dictionary<string, ModModule> _supportedModules;
    private readonly List<string> _unsupportedModules;
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

    private FileSystemPath GetModulePath(ModModule module)
    {
        return ModDirectoryPath + module.Id;
    }

    #endregion

    #region Public Methods

    public ReadOnlyCollection<IModFileResource> GetAddedFiles() => _addedFiles.AsReadOnly();
    public ReadOnlyCollection<ModFilePath> GetRemovedFiles() => _removedFiles.AsReadOnly();
    public ReadOnlyCollection<ModModule> GetSupportedModules() => _supportedModules.Values.ToList().AsReadOnly();
    public ReadOnlyCollection<string> GetUnsupportedModules() => _unsupportedModules.AsReadOnly();

    public FileSystemPath? GetThumbnailFilePath()
    {
        FileSystemPath thumbFilePath = ModDirectoryPath + ThumbnailFileName;

        if (!thumbFilePath.FileExists)
            return null;

        return thumbFilePath;
    }

    #endregion
}