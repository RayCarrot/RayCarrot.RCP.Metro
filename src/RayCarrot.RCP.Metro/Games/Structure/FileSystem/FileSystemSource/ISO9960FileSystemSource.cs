using BinarySerializer.Disk.ISO9960;

namespace RayCarrot.RCP.Metro.Games.Structure;

public class ISO9960FileSystemSource : IFileSystemSource
{
    public ISO9960FileSystemSource(ISO9960BinFile iso)
    {
        Iso = iso;
        BasePath = iso.Offset?.File.AbsolutePath;
    }

    public ISO9960BinFile Iso { get; }
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