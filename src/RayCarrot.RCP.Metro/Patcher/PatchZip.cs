using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.IO.Compression;
using System.Linq;
using NLog;

namespace RayCarrot.RCP.Metro.Patcher;

// TODO: Zip files can be a bit slow - maybe find better solution? We could create custom archive type
//       using BinarySerializer, but then we loose the ability to easily view/edit it.

public class PatchZip : IDisposable
{
    public PatchZip(FileSystemPath filePath, bool readOnly = false)
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

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    private readonly bool _readOnly;
    private ZipArchive? _zip;

    public FileSystemPath FilePath { get; }
    public bool CanRead => _zip != null;

    [MemberNotNull(nameof(_zip))]
    private void InitZipForWriting()
    {
        if (_zip is not null)
            return;

        FileAccess fileAccess = _readOnly ? FileAccess.Read : FileAccess.ReadWrite;
        ZipArchiveMode zipArchiveMode = _readOnly ? ZipArchiveMode.Read : ZipArchiveMode.Update;
        _zip = new ZipArchive(File.Open(FilePath, FileMode.CreateNew, fileAccess), zipArchiveMode);
    }

    private ZipArchiveEntry CreateZipEntry(string filePath)
    {
        InitZipForWriting();

        ZipArchiveEntry? existingEntry = _zip.GetEntry(filePath);
        existingEntry?.Delete();

        // Use fastest compression level. It doesn't loose that much space and is usually much faster. In a test
        // I made it went down from around 40 to 15 seconds while only loosing 10 MB gained space (out of 310).
        return _zip.CreateEntry(filePath, CompressionLevel.Fastest);
    }

    public void WriteStream(string filePath, Stream stream)
    {
        ZipArchiveEntry entry = CreateZipEntry(filePath);

        using Stream s = entry.Open();
        stream.CopyTo(s);
    }

    public void WriteJSON<T>(string filePath, T obj)
    {
        ZipArchiveEntry entry = CreateZipEntry(filePath);

        using Stream s = entry.Open();
        JsonHelpers.SerializeToStream(obj, s);
    }

    public void DeleteDirectory(string dirPath)
    {
        InitZipForWriting();

        foreach (ZipArchiveEntry entry in _zip.Entries.Where(x => x.FullName.StartsWith(dirPath)).ToArray())
            entry.Delete();
    }

    public Stream? OpenStream(string filePath)
    {
        if (_zip is null)
            throw new Exception("Can't open patch stream from a zip which has not yet been created");

        ZipArchiveEntry? entry = _zip.GetEntry(filePath);

        if (entry == null)
            return null;

        Stream stream = entry.Open();

        // Wrap the entry in a stream where we can set a fixed length if read-only. If we don't do this
        // then the length can't be read from the stream as the deflate stream does not support it.
        return _readOnly 
            ? new PatchZipFileEntryReadStream(stream, entry.Length) 
            : stream;
    }

    public void Apply()
    {
        Stopwatch s = Stopwatch.StartNew();
        Dispose();
        s.Stop();
        Logger.Trace("Repacked ZIP in {0} ms", s.ElapsedMilliseconds);
    }

    public void Dispose()
    {
        _zip?.Dispose();
        _zip = null;
    }
}