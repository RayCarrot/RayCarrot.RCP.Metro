using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;

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

    public PatchManifest? ReadManifest()
    {
        if (!_zip.CanRead)
            return null;

        using Stream? s = _zip.OpenStream(ManifestFileName);

        if (s is null)
            throw new Exception("Patch does not contain a valid manifest file");

        return JsonHelpers.DeserializeFromStream<PatchManifest>(s);
    }

    public void WriteManifest(PatchManifest manifest)
    {
        _zip.WriteJSON(ManifestFileName, manifest);
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

    public void Apply() => _zip.Apply();

    public void Dispose()
    {
        _zip.Dispose();
    }

    #endregion

    #region Public Static Methods

    public static string GenerateID(params string?[] existingIDs)
    {
        string id;

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