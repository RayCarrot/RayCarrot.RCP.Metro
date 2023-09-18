using System.IO;
using RayCarrot.RCP.Metro.ModLoader.Modules;
using RayCarrot.RCP.Metro.ModLoader.Resource;

namespace RayCarrot.RCP.Metro.ModLoader.Library;

public class ModLibrary
{
    #region Constructor

    public ModLibrary(GameInstallation gameInstallation)
    {
        GameInstallation = gameInstallation;

        // Get paths
        LibraryDirectoryPath = gameInstallation.InstallLocation.Directory + RootDirectoryName;
        LibraryMetadataFilePath = LibraryDirectoryPath + LibraryMetadataFileName;
        FileHistoryFilePath = LibraryDirectoryPath + HistoryFileName;
        FileHistoryDirectoryPath = LibraryDirectoryPath + HistoryDirectoryName;
        ModsDirectoryPath = LibraryDirectoryPath + ModsDirectoryName;
        ModManifestFilePath = LibraryDirectoryPath + ModsFileName;
    }

    #endregion

    #region Constants

    private const int LatestFormatVersion = 0;

    private const string RootDirectoryName = ".rcp_mods";
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

    private bool HasCreatedLibrary { get; set; }

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

    private void CreateLibrary()
    {
        // Always run through the creation code once, even if it already exists. This is to ensure
        // everything is set up correctly and using the latest version.
        if (HasCreatedLibrary)
            return;

        // Create the root directory and set it to be hidden
        DirectoryInfo dirInfo = new(LibraryDirectoryPath);
        dirInfo.Create();
        dirInfo.Attributes |= FileAttributes.Hidden;

        // Write latest metadata
        JsonHelpers.SerializeToFile(new LibraryMetadata(LatestFormatVersion), LibraryMetadataFilePath);

        HasCreatedLibrary = true;
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
        else if (LibraryDirectoryPath.DirectoryExists)
        {
            // TODO-UPDATE: Log error since this shouldn't happen, but continue anyway without throwing
        }
    }

    public void DeleteLibrary()
    {
        Services.File.DeleteDirectory(LibraryDirectoryPath);
    }

    public void InstallMod(FileSystemPath sourcePath, string modId, bool keepSourceFiles)
    {
        CreateLibrary();

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

    public Mod ReadInstalledMod(string modId, IReadOnlyCollection<ModModule> possibleModules)
    {
        return new Mod(GetInstalledModPath(modId), possibleModules);
    }

    public void WriteModManifest(ModManifest modManifest)
    {
        CreateLibrary();

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
        CreateLibrary();

        if (keepSourceFiles)
            Services.File.CopyDirectory(sourcePath, FileHistoryDirectoryPath, true, true);
        else
            Services.File.MoveDirectory(sourcePath, FileHistoryDirectoryPath, true, true);
    }

    public void WriteHistory(LibraryFileHistory fileHistory)
    {
        CreateLibrary();

        JsonHelpers.SerializeToFile<LibraryFileHistory>(fileHistory, FileHistoryFilePath);
    }

    #endregion
}