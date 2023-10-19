using System.IO;
using RayCarrot.RCP.Metro.Games.Components;
using RayCarrot.RCP.Metro.ModLoader.Resource;

namespace RayCarrot.RCP.Metro.ModLoader.Library;

public class ModLibrary
{
    #region Constructor

    public ModLibrary(GameInstallation gameInstallation)
    {
        GameInstallation = gameInstallation;

        // Get paths
        LibraryDirectoryPath = gameInstallation.GetRequiredComponent<ModLibraryPathComponent>().CreateObject();
        LibraryMetadataFilePath = LibraryDirectoryPath + LibraryMetadataFileName;
        FileHistoryFilePath = LibraryDirectoryPath + HistoryFileName;
        FileHistoryDirectoryPath = LibraryDirectoryPath + HistoryDirectoryName;
        ModsDirectoryPath = LibraryDirectoryPath + ModsDirectoryName;
        ModManifestFilePath = LibraryDirectoryPath + ModsFileName;
    }

    #endregion

    #region Logger

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    #endregion

    #region Constants

    private const int LatestFormatVersion = 0;

    private const string LibraryMetadataFileName = "library.json";
    private const string HistoryDirectoryName = "file_history";
    private const string HistoryFileName = "file_history.json";
    private const string ModsDirectoryName = "mods";
    private const string ModsFileName = "mods.json";

    #endregion

    #region Private Properties

    private FileSystemPath LibraryDirectoryPath { get; }
    private FileSystemPath LibraryMetadataFilePath { get; }
    private FileSystemPath FileHistoryDirectoryPath { get; }
    private FileSystemPath FileHistoryFilePath { get; }
    private FileSystemPath ModsDirectoryPath { get; }
    private FileSystemPath ModManifestFilePath { get; }

    private bool HasInitializedLibrary { get; set; }

    #endregion

    #region Public Properties

    public GameInstallation GameInstallation { get; }
    public bool IsInitialized => LibraryDirectoryPath.DirectoryExists;

    #endregion

    #region Private Methods

    private FileSystemPath GetInstalledModPath(string modId)
    {
        return ModsDirectoryPath + modId;
    }

    private void InitializeLibrary()
    {
        // Always run through the initializing code once, even if it already exists. This is to ensure
        // everything is set up correctly and using the latest version.
        if (HasInitializedLibrary)
            return;

        // Create the root directory and set it to be hidden
        DirectoryInfo dirInfo = new(LibraryDirectoryPath);
        dirInfo.Create();
        dirInfo.Attributes |= FileAttributes.Hidden;

        // Write latest metadata
        JsonHelpers.SerializeToFile(new LibraryMetadata(LatestFormatVersion), LibraryMetadataFilePath);

        HasInitializedLibrary = true;

        Logger.Info("Initialized mod library");
    }

    #endregion

    #region Public Methods

    public void VerifyLibrary()
    {
        // Read metadata if it exists
        if (LibraryMetadataFilePath.FileExists)
        {
            LibraryMetadata metadata = JsonHelpers.DeserializeFromFile<LibraryMetadata>(LibraryMetadataFilePath);

            if (metadata.Format > LatestFormatVersion)
                throw new UnsupportedModLibraryFormatException($"Format version {metadata.Format} is higher than the latest supported version {LatestFormatVersion}");
        }
        else if (IsInitialized)
        {
            Logger.Warn("Mod library is initialized without a metadata file");
        }
    }

    public void DeleteLibrary()
    {
        Services.File.DeleteDirectory(LibraryDirectoryPath);
    }

    public void InstallMod(FileSystemPath sourcePath, string modId, bool keepSourceFiles)
    {
        InitializeLibrary();

        FileSystemPath modOutputPath = GetInstalledModPath(modId);

        if (keepSourceFiles)
            Services.File.CopyDirectory(sourcePath, modOutputPath, true, true);
        else
            Services.File.MoveDirectory(sourcePath, modOutputPath, true, true);
    }

    public void UninstallMod(string modId)
    {
        FileSystemPath modPath = GetInstalledModPath(modId);
        Services.File.DeleteDirectory(modPath);
    }

    public ModManifest ReadModManifest()
    {
        if (!ModManifestFilePath.FileExists)
            return new ModManifest(new Dictionary<string, ModManifestEntry>());

        return JsonHelpers.DeserializeFromFile<ModManifest>(ModManifestFilePath);
    }

    public Mod ReadInstalledMod(string modId, GameInstallation gameInstallation)
    {
        return new Mod(GetInstalledModPath(modId), gameInstallation);
    }

    public void WriteModManifest(ModManifest modManifest)
    {
        InitializeLibrary();

        JsonHelpers.SerializeToFile(modManifest, ModManifestFilePath);
    }

    public IModFileResource GetFileHistoryResource(ModFilePath modFilePath)
    {
        return new LibraryHistoryModFileResource(modFilePath, FileHistoryDirectoryPath + modFilePath.FullFilePath);
    }

    public LibraryFileHistory? ReadHistory()
    {
        if (!FileHistoryFilePath.FileExists)
            return null;

        return JsonHelpers.DeserializeFromFile<LibraryFileHistory>(FileHistoryFilePath);
    }

    public void ReplaceFileHistory(FileSystemPath sourcePath, bool keepSourceFiles)
    {
        InitializeLibrary();

        if (keepSourceFiles)
            Services.File.CopyDirectory(sourcePath, FileHistoryDirectoryPath, true, true);
        else
            Services.File.MoveDirectory(sourcePath, FileHistoryDirectoryPath, true, true);
    }

    public void WriteHistory(LibraryFileHistory fileHistory)
    {
        InitializeLibrary();

        JsonHelpers.SerializeToFile<LibraryFileHistory>(fileHistory, FileHistoryFilePath);
    }

    #endregion
}