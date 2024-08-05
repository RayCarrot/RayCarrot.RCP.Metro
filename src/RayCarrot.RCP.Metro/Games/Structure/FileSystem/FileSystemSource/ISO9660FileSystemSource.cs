using BinarySerializer.Disk.ISO9660;

namespace RayCarrot.RCP.Metro.Games.Structure;

public class ISO9660FileSystemSource : IFileSystemSource
{
    public ISO9660FileSystemSource(DiscImage iso)
    {
        Iso = iso;
        BasePath = iso.Offset?.File.AbsolutePath;
    }

    public DiscImage Iso { get; }
    public FileSystemPath BasePath { get; }

    public bool FileExists(string path)
    {
        return Iso.GetFile(path, false) != null;
    }

    public bool DirectoryExists(string path)
    {
        return Iso.GetDirectory(path, false) != null;
    }
}