using System.IO;
using RayCarrot.RCP.Metro.Patcher.Resource;

namespace RayCarrot.RCP.Metro.Patcher.Library;

public class PatchLibrary
{
    #region Constructor

    public PatchLibrary(GameInstallation gameInstallation)
    {
        GameInstallation = gameInstallation;

        // Get paths
        LibraryDirectoryPath = gameInstallation.InstallLocation.Directory + RootDirectoryName;
        LibraryMetadataFilePath = LibraryDirectoryPath + LibraryMetadataFileName;
        FileHistoryFilePath = LibraryDirectoryPath + HistoryFileName;
        FileHistoryDirectoryPath = LibraryDirectoryPath + HistoryDirectoryName;
        PatchManifestFilePath = LibraryDirectoryPath + PatchesFileName;
    }

    #endregion

    #region Constants

    private const int LatestFormatVersion = 0;

    private const string RootDirectoryName = ".rcp_mods";
    private const string LibraryMetadataFileName = "library.json";
    private const string HistoryDirectoryName = "file_history";
    private const string HistoryFileName = "file_history.json";
    private const string PatchesFileName = "mods.json";

    #endregion

    #region Private Properties

    private FileSystemPath LibraryDirectoryPath { get; }
    private FileSystemPath LibraryMetadataFilePath { get; }
    private FileSystemPath FileHistoryDirectoryPath { get; }
    private FileSystemPath FileHistoryFilePath { get; }
    private FileSystemPath PatchManifestFilePath { get; }

    private bool HasCreatedLibrary { get; set; }

    #endregion

    #region Public Properties

    public GameInstallation GameInstallation { get; }

    #endregion

    #region Private Methods

    private FileSystemPath GetInstalledPatchPath(string patchId)
    {
        return LibraryDirectoryPath + patchId;
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
                throw new UnsupportedPatchLibraryFormatException($"Format version {metadata.Format} is higher than the latest supported version {LatestFormatVersion}");
        }
        else if (LibraryDirectoryPath.DirectoryExists)
        {
            // TODO-UPDATE: Log error since this shouldn't happen, but continue anyway without throwing
        }
    }

    public PatchManifestEntry InstallPatch(FileSystemPath patchFile, string patchId)
    {
        CreateLibrary();

        FileSystemPath patchOutputPath = GetInstalledPatchPath(patchId);

        throw new NotImplementedException("Not implemented installing patch from file");

        long size = (long)patchOutputPath.GetSize().Bytes;

        return new PatchManifestEntry(patchId, size, false, InstalledPatch.DefaultVersion);
    }

    public void UninstallPatch(string patchId)
    {
        FileSystemPath patchPath = GetInstalledPatchPath(patchId);
        Services.File.DeleteDirectory(patchPath);
    }

    public PatchManifest ReadPatchManifest()
    {
        if (!PatchManifestFilePath.FileExists)
            return new PatchManifest(new Dictionary<string, PatchManifestEntry>());

        return JsonHelpers.DeserializeFromFile<PatchManifest>(PatchManifestFilePath);
    }

    public InstalledPatch ReadInstalledPatch(string patchId)
    {
        return new InstalledPatch(LibraryDirectoryPath + patchId);
    }

    public void WritePatchManifest(PatchManifest patchManifest)
    {
        CreateLibrary();

        JsonHelpers.SerializeToFile(patchManifest, PatchManifestFilePath);
    }

    public IPatchFileResource GetFileHistoryResource(PatchFilePath patchFilePath)
    {
        return new LibraryHistoryPatchFileResource(patchFilePath, FileHistoryDirectoryPath + patchFilePath.FullFilePath);
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