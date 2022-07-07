using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using NLog;

namespace RayCarrot.RCP.Metro.Archive;

/// <summary>
/// An archive patch (.ap)
/// </summary>
public class PatchFile : IDisposable
{
    #region Constructor

    public PatchFile(FileSystemPath filePath, bool readOnly = false)
    {
        _zip = new PatchZip(filePath, readOnly);
    }

    #endregion

    #region Logger

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    #endregion

    #region Constants

    private const string ManifestFileName = "manifest.json";
    public const int Version = 0;
    public const string FileExtensions = ".ap"; // Archive Patch

    #endregion

    #region Private Fields

    private readonly PatchZip _zip;

    #endregion

    #region Public Properties

    public FileSystemPath FilePath => _zip.FilePath;

    #endregion

    #region Private Methods

    private string GetPatchResourcePath(string resourceName, bool isNormalized)
    {
        if (!isNormalized)
            resourceName = NormalizeResourceName(resourceName);

        return $"resources/{resourceName}";
    }

    private string GetPatchAssetPath(string assetName) => $"assets/{assetName}";

    #endregion

    #region Public Methods

    public PatchManifest ReadManifest()
    {
        Logger.Info("Reading patch manifest");

        if (!_zip.CanRead)
            throw new Exception("The manifest can not be read from the patch");

        using Stream? s = _zip.OpenStream(ManifestFileName);

        if (s is null)
            throw new Exception("Patch does not contain a valid manifest file");

        PatchManifest manifest = JsonHelpers.DeserializeFromStream<PatchManifest>(s);

        Logger.Info("Read patch manifest with version {0} and ID {1}", manifest.PatchVersion, manifest.ID);

        return manifest;
    }

    public void WriteManifest(PatchManifest manifest)
    {
        _zip.WriteJSON(ManifestFileName, manifest);
        Logger.Info("Wrote patch manifest");
    }

    public Stream GetPatchResource(string resourceName, bool isNormalized)
    {
        string path = GetPatchResourcePath(resourceName, isNormalized);
        return _zip.OpenStream(path) ?? throw new Exception($"Resource with name {resourceName} was not found");
    }

    public Stream GetPatchAsset(string assetName)
    {
        string path = GetPatchAssetPath(assetName);
        return _zip.OpenStream(path) ?? throw new Exception($"Asset with name {assetName} was not found");
    }

    public void AddPatchResource(string resourceName, bool isNormalized, Stream stream)
    {
        _zip.WriteStream(GetPatchResourcePath(resourceName, isNormalized), stream);
    }

    public void AddPatchAsset(string assetName, Stream stream)
    {
        _zip.WriteStream(GetPatchAssetPath(assetName), stream);
    }

    /// <summary>
    /// Gets the normalized resource name for the resource file path
    /// </summary>
    /// <param name="filePath">The resource file path</param>
    /// <returns>The normalized resource name</returns>
    public string NormalizeResourceName(string filePath) => filePath.ToLowerInvariant().Replace('\\', '/');

    public void Apply()
    {
        _zip.Apply();
        Logger.Info("Applied patch file modifications");
    }

    public void Dispose()
    {
        _zip.Dispose();
    }

    #endregion

    #region Public Static Methods

    public static string GenerateID(params string?[] existingIDs)
    {
        string id;

        // We probably don't need to verify the ID doesn't conflict with an existing one,
        // but let's do so anyway just to be on the safe side
        do
        {
            id = Guid.NewGuid().ToString();
        } while (existingIDs.Contains(id));

        return id;
    }

    public static string CalculateChecksum(Stream stream)
    {
        using SHA256Managed sha = new();
        byte[] checksum = sha.ComputeHash(stream);
        return BitConverter.ToString(checksum);
    }

    #endregion
}