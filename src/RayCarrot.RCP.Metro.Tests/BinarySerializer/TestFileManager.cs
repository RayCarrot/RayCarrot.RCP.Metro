using BinarySerializer;

namespace RayCarrot.RCP.Metro.Tests;

public class TestFileManager : IFileManager
{
    public bool DirectoryExists(string? path) =>
        throw new InvalidOperationException("Can't access file system in test");
    public bool FileExists(string? path) =>
        throw new InvalidOperationException("Can't access file system in test");

    public Stream GetFileReadStream(string path) =>
        throw new InvalidOperationException("Can't access file system in test");
    public Stream GetFileWriteStream(string path, bool recreateOnWrite = true) =>
        throw new InvalidOperationException("Can't access file system in test");

    public PathSeparatorChar SeparatorCharacter => PathSeparatorChar.ForwardSlash;
    public Task FillCacheForReadAsync(long length, Reader reader) => Task.CompletedTask;
}