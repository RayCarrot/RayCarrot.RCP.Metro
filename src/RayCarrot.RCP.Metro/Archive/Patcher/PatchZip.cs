﻿using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace RayCarrot.RCP.Metro.Archive;

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

        return _zip.CreateEntry(filePath);
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

        return _zip.GetEntry(filePath)?.Open();
    }

    public void Apply() => Dispose();

    public void Dispose()
    {
        _zip?.Dispose();
        _zip = null;
    }
}