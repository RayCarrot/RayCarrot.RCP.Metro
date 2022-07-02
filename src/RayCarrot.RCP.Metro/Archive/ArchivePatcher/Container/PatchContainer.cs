using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;

namespace RayCarrot.RCP.Metro.Archive;

/// <summary>
/// An archive patch container (.apc). This is stored alongside an archive file and keeps track of the applied patches and the original
/// files which have been replaced so that they can be restored. Each patch contains a manifest with details as well as
/// resources and assets. The resources are the added files while the assets are things such as a thumbnail.
/// </summary>
public class PatchContainer : IDisposable
{
    // TODO-UPDATE: Logging

    #region Constructor

    public PatchContainer(FileSystemPath filePath, bool readOnly = false)
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

    #region Contants

    private const string ManifestFileName = "manifest.json";
    public const int Version = 0;
    public const string FileExtensions = ".apc"; // Archive Patch Container

    #endregion

    #region Private Fields

    private readonly bool _readOnly;
    private ZipArchive? _zip;

    #endregion

    #region Public Properties

    public FileSystemPath FilePath { get; }

    #endregion

    #region Private Methods

    private string GetPatchResourcePath(string patchID, string resourceName, bool isNormalized)
    {
        if (!isNormalized)
            resourceName = NormalizeResourceName(resourceName);

        return $"{patchID}/resources/{resourceName}";
    }
    private string GetPatchAssetPath(string patchID, string assetName) => $"{patchID}/assets/{assetName}";

    [MemberNotNull(nameof(_zip))]
    private void InitZipForWriting()
    {
        if (_zip is not null)
            return;

        FileAccess fileAccess = _readOnly ? FileAccess.Read : FileAccess.ReadWrite;
        ZipArchiveMode zipArchiveMode = _readOnly ? ZipArchiveMode.Read : ZipArchiveMode.Update;
        _zip = new ZipArchive(File.Open(FilePath, FileMode.CreateNew, fileAccess), zipArchiveMode);
    }

    private ZipArchiveEntry CreateZipEntry(string path)
    {
        InitZipForWriting();

        ZipArchiveEntry? existingEntry = _zip.GetEntry(path);
        existingEntry?.Delete();

        return _zip.CreateEntry(path);
    }

    private void WriteManifest(PatchContainerManifest containerManifest)
    {
        ZipArchiveEntry entry = CreateZipEntry(ManifestFileName);

        using Stream s = entry.Open();
        JsonHelpers.SerializeToStream(containerManifest, s);
    }

    private void WriteFile(string fullPath, Stream stream)
    {
        ZipArchiveEntry entry = CreateZipEntry(fullPath);
        using Stream fileStream = entry.Open();
        stream.CopyTo(fileStream);
    }

    #endregion

    #region Public Methods

    public PatchContainerManifest? ReadManifest()
    {
        if (_zip is null)
            return null;

        ZipArchiveEntry? entry = _zip.GetEntry(ManifestFileName);

        if (entry is null)
            throw new Exception("Container does not contain a valid manifest file");

        using Stream s = entry.Open();
        return JsonHelpers.DeserializeFromStream<PatchContainerManifest>(s);
    }

    public void WriteManifest(PatchHistoryManifest history, PatchManifest[] patches, string[]? enabledPatches)
    {
        PatchContainerManifest containerManifest = new(history, patches, enabledPatches, Version);
        WriteManifest(containerManifest);
    }

    public Stream GetPatchResource(string patchID, string resourceName, bool isNormalized)
    {
        string path = GetPatchResourcePath(patchID, resourceName, isNormalized);

        if (_zip is null)
            throw new Exception("Can't retrieve resource from a container which has not yet been created");

        return _zip.GetEntry(path)?.Open() ?? throw new Exception($"Resource with ID {patchID} and name {resourceName} was not found");
    }

    public Stream GetPatchAsset(string patchID, string assetName)
    {
        string path = GetPatchAssetPath(patchID, assetName);

        if (_zip is null)
            throw new Exception("Can't retrieve asset from a container which has not yet been created");

        return _zip.GetEntry(path)?.Open() ?? throw new Exception($"Asset with name {assetName} was not found");
    }

    public void ClearPatchFiles(string patchID)
    {
        InitZipForWriting();

        foreach (ZipArchiveEntry entry in _zip.Entries.Where(x => x.FullName.StartsWith(patchID)).ToArray())
            entry.Delete();
    }

    public void AddPatchResource(string patchID, string resourceName, bool isNormalized, Stream stream)
    {
        WriteFile(GetPatchResourcePath(patchID, resourceName, isNormalized), stream);
    }

    public void AddPatchAsset(string patchID, string assetName, Stream stream)
    {
        WriteFile(GetPatchAssetPath(patchID, assetName), stream);
    }

    public string CalculateChecksum(Stream stream)
    {
        using SHA256Managed sha = new();
        byte[] checksum = sha.ComputeHash(stream);
        return BitConverter.ToString(checksum);
    }

    public string GenerateNewPatchID(params string?[] existingIDs)
    {
        string id;

        do
        {
            id = Guid.NewGuid().ToString();
        } while (existingIDs.Contains(id));

        return id;
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