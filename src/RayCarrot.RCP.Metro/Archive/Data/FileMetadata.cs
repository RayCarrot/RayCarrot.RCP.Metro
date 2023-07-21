using System.IO;

namespace RayCarrot.RCP.Metro.Archive;

public class FileMetadata
{
    public FileMetadata() { }
    public FileMetadata(FileSystemPath filePath)
    {
        LastModified = filePath.GetFileInfo().LastWriteTime;
    }

    public DateTimeOffset? LastModified { get; set; }

    public void ApplyToFile(FileSystemPath filePath)
    {
        if (LastModified != null)
            File.SetLastWriteTime(filePath, LastModified.Value.DateTime);
    }
}