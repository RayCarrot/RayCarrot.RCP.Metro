using System.IO;
using System.IO.Compression;

namespace RayCarrot.RCP.Metro.Patcher;

public class PatchHistory
{
    public PatchHistory(FileSystemPath directoryPath)
    {
        DirectoryPath = directoryPath;
    }

    public FileSystemPath DirectoryPath { get; }

    public bool CompressFiles => true;

    private FileSystemPath GetHistoryResourceFilePath(PatchFilePath resourcePath) => DirectoryPath + resourcePath.FullFilePath;

    public Stream ReadFile(PatchFilePath resourcePath)
    {
        FileSystemPath filePath = GetHistoryResourceFilePath(resourcePath);

        if (!CompressFiles)
            return File.OpenRead(filePath);

        using Stream fileStream = File.OpenRead(filePath);
        
        using DeflateStream deflateStream = new(fileStream, CompressionMode.Decompress);
        
        // TODO-UPDATE: Will this cause big memory usage?
        // Decompress to a memory stream or else we can't access the length
        MemoryStream memStream = new();
        deflateStream.CopyTo(memStream);
        memStream.Position = 0;
        return memStream;
    }

    public void AddFile(PatchFilePath resourcePath, Stream stream)
    {
        FileSystemPath filePath = GetHistoryResourceFilePath(resourcePath);
        Directory.CreateDirectory(filePath.Parent);
        using Stream fileStream = File.Create(filePath);

        if (CompressFiles)
        {
            using DeflateStream deflateStream = new(fileStream, CompressionLevel.Fastest);
            stream.CopyTo(deflateStream);
        }
        else
        {
            stream.CopyTo(fileStream);
        }
    }

    public void MoveFile(PatchFilePath resourcePath, PatchHistory destHistory)
    {
        FileSystemPath src = GetHistoryResourceFilePath(resourcePath);
        FileSystemPath dest = destHistory.GetHistoryResourceFilePath(resourcePath);

        Directory.CreateDirectory(dest.Parent);
        File.Move(src, dest);
    }
}