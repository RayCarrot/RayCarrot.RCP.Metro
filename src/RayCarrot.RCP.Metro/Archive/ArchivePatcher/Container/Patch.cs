using System;
using System.IO;
using System.IO.Compression;

namespace RayCarrot.RCP.Metro.Archive;

/// <summary>
/// An archive patch (.ap)
/// </summary>
public class Patch : IDisposable
{
    #region Constructor

    public Patch(FileSystemPath filePath, bool readOnly = false)
    {
        FilePath = filePath;
        _readOnly = readOnly;

        if (FilePath.FileExists)
        {
            var fileAccess = readOnly ? FileAccess.Read : FileAccess.ReadWrite;
            var zipArchiveMode = readOnly ? ZipArchiveMode.Read : ZipArchiveMode.Update;
            _zip = new ZipArchive(File.Open(FilePath, FileMode.Open, fileAccess), zipArchiveMode);
        }
    }

    #endregion

    #region Constants

    private const string ManifestFileName = "manifest.json";
    public const int Version = 0;
    public const string FileExtensions = ".ap"; // Archive Patch

    #endregion

    #region Private Fields

    private readonly bool _readOnly;
    private ZipArchive? _zip;

    #endregion

    #region Public Properties

    public FileSystemPath FilePath { get; }

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
        if (_zip is null)
            return null;

        // Get the manifest entry
        ZipArchiveEntry? entry = _zip.GetEntry(ManifestFileName);

        if (entry is null)
            throw new Exception("Patch does not contain a valid manifest file");

        using Stream s = entry.Open();
        return JsonHelpers.DeserializeFromStream<PatchManifest>(s);
    }

    public Stream GetPatchResource(string resourceName, bool isNormalized)
    {
        string path = GetPatchResourcePath(resourceName, isNormalized);

        if (_zip is null)
            throw new Exception("Can't retrieve resource from a patch which has not yet been created");

        return _zip.GetEntry(path)?.Open() ?? throw new Exception($"Resource with name {resourceName} was not found");
    }

    public Stream GetPatchAsset(string assetName)
    {
        string path = GetPatchAssetPath(assetName);

        if (_zip is null)
            throw new Exception("Can't retrieve asset from a patch which has not yet been created");

        return _zip.GetEntry(path)?.Open() ?? throw new Exception($"Asset with name {assetName} was not found");
    }

    /// <summary>
    /// Gets the normalized resource name for the resource file path
    /// </summary>
    /// <param name="filePath">The resource file path</param>
    /// <returns>The normalized resource name</returns>
    public string NormalizeResourceName(string filePath) => filePath.ToLowerInvariant().Replace('\\', '/');

    public void Dispose()
    {
        _zip?.Dispose();
    }

    #endregion
}