using System.IO;

namespace RayCarrot.RCP.Metro;

public sealed class TempFileStream : FileStream
{
    public TempFileStream(TempFile file) : base(file.TempPath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None)
    {
        TempFile = file;
    }

    public TempFile TempFile { get; }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        TempFile.Dispose();
    }

    public static TempFileStream Create() => new(new TempFile(true));
}