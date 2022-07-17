using System.IO;
using NLog;

namespace RayCarrot.RCP.Metro.Patcher;

/// <summary>
/// A game patch library. This is stored in the root of a game installation as a hidden folder and keeps track of the
/// applied patches and the original files which have been replaced (history) so that they can be restored.<br/>
/// Each patch contains a manifest with details as well as resources and assets. The resources are the added files while the
/// assets are things such as a thumbnail.
/// </summary>
public class PatchLibrary
{
    #region Constructor

    public PatchLibrary(FileSystemPath directoryPath, IFileManager fileManager)
    {
        DirectoryPath = directoryPath;
        FileManager = fileManager;
        ManifestFilePath = directoryPath + ManifestFileName;
    }

    #endregion

    #region Logger

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    #endregion

    #region Contants

    private const string ManifestFileName = "manifest.json";

    #endregion

    #region Public Properties

    public FileSystemPath DirectoryPath { get; }
    public FileSystemPath ManifestFilePath { get; }

    #endregion

    #region Services

    private IFileManager FileManager { get; }

    #endregion

    #region Private Methods

    private void EnsureDirectoryExists()
    {
        DirectoryInfo dirInfo = new(DirectoryPath);
        
        dirInfo.Create();
        dirInfo.Attributes |= FileAttributes.Hidden;
    }

    private FileSystemPath GetPatchFilePath(string patchID) => DirectoryPath + $"{patchID}{PatchFile.FileExtension}";
    private FileSystemPath GetHistoryPath() => DirectoryPath + "history";

    #endregion

    #region Public Methods

    public PatchLibraryManifest? ReadManifest()
    {
        Logger.Info("Reading patch library manifest");

        if (!ManifestFilePath.FileExists)
        {
            Logger.Info("The manifest file does not exist");
            return null;
        }

        PatchLibraryManifest manifest = JsonHelpers.DeserializeFromFile<PatchLibraryManifest>(ManifestFilePath);

        Logger.Info("Read patch library manifest with version {0}", manifest.LibraryVersion);

        return manifest;
    }
    public void WriteManifest(Games game, PatchHistoryManifest history, PatchManifest[] patches, string[]? enabledPatches)
    {
        EnsureDirectoryExists();

        PatchLibraryManifest libraryManifest = new(PatchLibraryManifest.LatestVersion, game, history, patches, enabledPatches);
        
        JsonHelpers.SerializeToFile(libraryManifest, ManifestFilePath);
        
        Logger.Info("Wrote patch library manifest");
    }

    public PatchFile ReadPatchFile(string patchID) => new(GetPatchFilePath(patchID), readOnly: true);
    public void AddPatch(FileSystemPath patchFile, string patchID, bool move)
    {
        EnsureDirectoryExists();

        FileSystemPath patchPath = GetPatchFilePath(patchID);

        if (move)
            FileManager.MoveFile(patchFile, patchPath, true);
        else
            FileManager.CopyFile(patchFile, patchPath, true);
    }
    public void RemovePatch(string patchID) => FileManager.DeleteFile(GetPatchFilePath(patchID));

    public PatchHistory GetHistory() => new(GetHistoryPath());

    #endregion

    #region Public Static Methods

    public static FileSystemPath GetLibraryDirectoryPath(FileSystemPath installDir) => installDir + ".patches";

    #endregion
}