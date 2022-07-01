using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;

namespace RayCarrot.RCP.Metro.Archive;

public class PatchContainer : IDisposable
{
    // TODO-UPDATE: Logging

    public PatchContainer(FileSystemPath archiveFilePath, bool readOnly = false)
    {
        ArchiveFilePath = archiveFilePath;
        PatchContainerFilePath = archiveFilePath.AppendFileExtension(new FileExtension(ContainerFileExtensions));

        _readOnly = readOnly;

        if (PatchContainerFilePath.FileExists)
        {
            var fileAccess = readOnly ? FileAccess.Read : FileAccess.ReadWrite;
            var zipArchiveMode = readOnly ? ZipArchiveMode.Read : ZipArchiveMode.Update;
            _zip = new ZipArchive(File.Open(PatchContainerFilePath, FileMode.Open, fileAccess), zipArchiveMode);
        }
    }

    private const string ContainerFileExtensions = ".apc"; // Archive Patch Container
    private const string PatchFileExtensions = ".ap"; // Archive Patch
    private const string ContainerManifestFileName = "Manifest.json";
    private const string PatchManifestFileName = "Manifest.json";
    private const int LatestContainerVersion = 0;

    private readonly bool _readOnly;
    private ZipArchive? _zip;

    public FileSystemPath ArchiveFilePath { get; }
    public FileSystemPath PatchContainerFilePath { get; }

    public int ContainerVersion => LatestContainerVersion;

    private static string GetFullResourcePath(string patchID, string resourceName) => $"{patchID}/resources/{resourceName}";
    private static string GetThumbnailPath(string patchID) => $"{patchID}/thumb";

    [MemberNotNull(nameof(_zip))]
    private void InitZipForWriting()
    {
        if (_zip is not null) 
            return;
        
        FileAccess fileAccess = _readOnly ? FileAccess.Read : FileAccess.ReadWrite;
        ZipArchiveMode zipArchiveMode = _readOnly ? ZipArchiveMode.Read : ZipArchiveMode.Update;
        _zip = new ZipArchive(File.Open(PatchContainerFilePath, FileMode.CreateNew, fileAccess), zipArchiveMode);
    }

    public PatchManifest? ReadManifest()
    {
        if (_zip is null)
            return null;

        ZipArchiveEntry? entry = _zip.GetEntry(ContainerManifestFileName);

        if (entry is null)
            throw new Exception("Container does not contain a valid manifest file");

        using Stream s = entry.Open();
        return JsonHelpers.DeserializeFromStream<PatchManifest>(s);
    }

    public void WriteManifest(PatchHistoryManifest history, PatchManifestItem[] patches)
    {
        PatchManifest manifest = new(history, patches, LatestContainerVersion);
        
        InitZipForWriting();

        ZipArchiveEntry entry = _zip.GetEntry(ContainerManifestFileName) ?? _zip.CreateEntry(ContainerManifestFileName);
        
        using Stream s = entry.Open();
        JsonHelpers.SerializeToStream(manifest, s);
    }

    public Stream GetPatchResource(string patchID, string resourceName)
    {
        string path = GetFullResourcePath(patchID, resourceName);

        if (_zip is null)
            throw new Exception("Can't retrieve resource from a container which has not yet been created");

        return _zip.GetEntry(path)?.Open() ?? throw new Exception($"Resource with ID {patchID} and name {resourceName} was not found");
    }

    public Stream? GetPatchThumbnail(string patchID)
    {
        string path = GetThumbnailPath(patchID);

        if (_zip is null)
            throw new Exception("Can't retrieve thumbnail from a container which has not yet been created");

        return _zip.GetEntry(path)?.Open();
    }

    public void ClearResources(string patchID)
    {
        InitZipForWriting();

        foreach (ZipArchiveEntry entry in _zip.Entries.Where(x => x.FullName.StartsWith(patchID)).ToArray())
            entry.Delete();
    }

    public void AddResource(string patchID, string resourceName, Stream stream)
    {
        InitZipForWriting();

        ZipArchiveEntry entry = _zip.CreateEntry(GetFullResourcePath(patchID, resourceName));
        using Stream fileStream = entry.Open();
        stream.CopyTo(fileStream);
    }

    public string CalculateChecksum(Stream stream)
    {
        using SHA256Managed sha = new SHA256Managed();
        byte[] checksum = sha.ComputeHash(stream);
        return BitConverter.ToString(checksum);
    }

    public string GetNewPatchID(string?[] existingIDs)
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
    public string GetResourceName(string filePath) => filePath.ToLowerInvariant().Replace('\\', '/');

    public void Dispose()
    {
        _zip?.Dispose();
    }
}