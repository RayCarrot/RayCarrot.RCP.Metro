using System;
using System.IO;
using System.Runtime.Remoting;
using System.Security.Cryptography;
using NLog;

namespace RayCarrot.RCP.Metro.Patcher;

/// <summary>
/// A game patch (.gp)
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
    public const string FileExtension = ".gp"; // Game Patch

    #endregion

    #region Private Fields

    private readonly PatchZip _zip;

    #endregion

    #region Public Properties

    public FileSystemPath FilePath => _zip.FilePath;

    #endregion

    #region Private Methods

    private string GetPatchResourcePath(PatchFilePath resourcePath)
    {
        return $"resources/{resourcePath.NormalizedFullFilePath}";
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

    public Stream GetPatchResource(PatchFilePath resourcePath)
    {
        string path = GetPatchResourcePath(resourcePath);
        return _zip.OpenStream(path) ?? throw new Exception($"Resource with name {resourcePath} was not found");
    }

    public Stream GetPatchAsset(string assetName)
    {
        string path = GetPatchAssetPath(assetName);
        return _zip.OpenStream(path) ?? throw new Exception($"Asset with name {assetName} was not found");
    }

    public void AddPatchResource(PatchFilePath resourcePath, Stream stream)
    {
        _zip.WriteStream(GetPatchResourcePath(resourcePath), stream);
    }

    public void AddPatchAsset(string assetName, Stream stream)
    {
        _zip.WriteStream(GetPatchAssetPath(assetName), stream);
    }

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

    public static string GenerateID() => Guid.NewGuid().ToString();

    public static string CalculateChecksum(Stream stream)
    {
        using SHA256Managed sha = new();
        byte[] checksum = sha.ComputeHash(stream);
        return BitConverter.ToString(checksum);
    }

    #endregion
}