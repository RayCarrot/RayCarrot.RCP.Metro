using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using RayCarrot.RCP.Metro.Archive.OpenSpace;
using RayCarrot.RCP.Metro.Archive.UbiArt;

namespace RayCarrot.RCP.Metro.Archive;

/// <summary>
/// File item data for the Archive Explorer
/// </summary>
public class ArchiveFileItem : IDisposable
{
    static ArchiveFileItem()
    {
        // IDEA: Move this somewhere else?
        // Set the supported file types
        FileTypes = new IArchiveFileType[]
        {
            new ArchiveFileType_GF(),
            new ArchiveFileType_WAV(),
            new ArchiveFileType_Image(),
            new ArchiveFileType_DDSUbiArtTex(),
            new ArchiveFileType_GXTUbiArtTex(),
            new ArchiveFileType_GTXUbiArtTex(),
            new ArchiveFileType_PVRUbiArtTex(),
            new ArchiveFileType_GNFUbiArtTex(),
            new ArchiveFileType_Xbox360UbiArtTex(),
        };

        // Set default file type
        DefaultFileType = new ArchiveFileType_Default();
    }

    public ArchiveFileItem(IArchiveDataManager manager, string fileName, string directory, object archiveEntry)
    {
        Manager = manager;
        FileName = fileName;
        Directory = directory;
        ArchiveEntry = archiveEntry;
    }

    protected IArchiveDataManager Manager { get; }

    public string FileName { get; }
    public string Directory { get; }
    public object ArchiveEntry { get; }

    public Stream? PendingImport { get; protected set; }

    [MemberNotNullWhen(true, nameof(PendingImport))]
    public bool IsPendingImport => PendingImport != null;

    public FileExtension FileExtension => new FileExtension(FileName, multiple: true);

    public ArchiveFileStream GetFileData(IDisposable? generator)
    {
        if (!IsPendingImport && generator == null)
            throw new ArgumentNullException(nameof(generator), "A generator must be specified if there is no pending import");

        // Get the stream
        ArchiveFileStream stream = IsPendingImport 
            ? new ArchiveFileStream(PendingImport, FileName, false) 
            : new ArchiveFileStream(() => Manager.GetFileData(generator!, ArchiveEntry), FileName, true);
            
        // Seek to the beginning
        stream.SeekToBeginning();

        // Return the stream
        return stream;
    }

    [MemberNotNull(nameof(PendingImport))]
    public void SetPendingImport(Stream import)
    {
        PendingImport?.Dispose();
        PendingImport = import;
    }

    public IArchiveFileType GetFileType(ArchiveFileStream stream)
    {
        // Get types supported by the current manager
        IArchiveFileType[] types = FileTypes.Where(x => x.IsSupported(Manager)).ToArray();

        // First attempt to find matching file type based off of the file extension to avoid having to read the file
        IArchiveFileType? match = types.FirstOrDefault(x => x.IsOfType(FileExtension));

        // If no match, check the data
        if (match == null)
        {
            // Find a match from the stream data
            match = types.FirstOrDefault(x => x.IsOfType(FileExtension, stream, Manager));
        }

        // Return the type and set to default if still null
        return match ?? DefaultFileType;
    }

    private static IArchiveFileType[] FileTypes { get; }
    private static IArchiveFileType DefaultFileType { get; }

    public void Dispose()
    {
        PendingImport?.Dispose();
    }
}