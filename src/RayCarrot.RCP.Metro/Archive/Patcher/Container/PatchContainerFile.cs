using System;
using System.IO;
using NLog;

namespace RayCarrot.RCP.Metro.Archive;

/// <summary>
/// An archive patch container (.apc). This is stored alongside an archive file and keeps track of the applied patches and the original
/// files which have been replaced so that they can be restored. Each patch contains a manifest with details as well as
/// resources and assets. The resources are the added files while the assets are things such as a thumbnail.
/// </summary>
public class PatchContainerFile : IDisposable
{
    #region Constructor

    public PatchContainerFile(FileSystemPath filePath, bool readOnly = false)
    {
        _zip = new PatchZip(filePath, readOnly);
    }

    #endregion

    #region Logger

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    #endregion

    #region Contants

    private const string ManifestFileName = "manifest.json";
    public const int Version = 0;
    public const string FileExtensions = ".apc"; // Archive Patch Container

    #endregion

    #region Private Fields

    private readonly PatchZip _zip;

    #endregion

    #region Public Properties

    public FileSystemPath FilePath => _zip.FilePath;

    #endregion

    #region Private Methods

    private string GetPatchResourcePath(string patchID, string resourceName, bool isNormalized)
    {
        if (!isNormalized)
            resourceName = NormalizeResourceName(resourceName);

        return $"{patchID}/resources/{resourceName}";
    }
    private string GetPatchAssetPath(string patchID, string assetName) => $"{patchID}/assets/{assetName}";

    #endregion

    #region Public Methods

    public PatchContainerManifest? ReadManifest()
    {
        Logger.Info("Reading patch container manifest");

        if (!_zip.CanRead)
        {
            Logger.Info("Can't read patch container manifest due to the zip not being readable");
            return null;
        }

        using Stream? s = _zip.OpenStream(ManifestFileName);

        if (s is null)
            throw new Exception("Container does not contain a valid manifest file");

        PatchContainerManifest manifest = JsonHelpers.DeserializeFromStream<PatchContainerManifest>(s);

        Logger.Info("Read patch container manifest with version {0}", manifest.ContainerVersion);

        return manifest;
    }

    public void WriteManifest(PatchHistoryManifest history, PatchManifest[] patches, string[]? enabledPatches)
    {
        PatchContainerManifest containerManifest = new(Version, history, patches, enabledPatches);
        _zip.WriteJSON(ManifestFileName, containerManifest);
        Logger.Info("Wrote patch container manifest");
    }

    public Stream GetPatchResource(string patchID, string resourceName, bool isNormalized)
    {
        string path = GetPatchResourcePath(patchID, resourceName, isNormalized);
        return _zip.OpenStream(path) ?? throw new Exception($"Resource with ID {patchID} and name {resourceName} was not found");
    }

    public Stream GetPatchAsset(string patchID, string assetName)
    {
        string path = GetPatchAssetPath(patchID, assetName);
        return _zip.OpenStream(path) ?? throw new Exception($"Asset with name {assetName} was not found");
    }

    public void ClearPatchFiles(string patchID)
    {
        _zip.DeleteDirectory(patchID);
        Logger.Info("Cleared patch files for patch {0}", patchID);
    }

    public void AddPatchResource(string patchID, string resourceName, bool isNormalized, Stream stream)
    {
        _zip.WriteStream(GetPatchResourcePath(patchID, resourceName, isNormalized), stream);
    }

    public void AddPatchAsset(string patchID, string assetName, Stream stream)
    {
        _zip.WriteStream(GetPatchAssetPath(patchID, assetName), stream);
    }

    public void Apply()
    {
        _zip.Apply();
        Logger.Info("Applied patch container file modifications");
    }

    public void Dispose()
    {
        _zip.Dispose();
    }

    #endregion

    #region Public Static Methods

    /// <summary>
    /// Gets the normalized resource name for the resource file path
    /// </summary>
    /// <param name="filePath">The resource file path</param>
    /// <returns>The normalized resource name</returns>
    public static string NormalizeResourceName(string filePath) => filePath.ToLowerInvariant().Replace('\\', '/');

    public static FileSystemPath GetContainerFilePath(FileSystemPath archiveFilePath) =>
        archiveFilePath.AppendFileExtension(new FileExtension(FileExtensions));

    #endregion
}