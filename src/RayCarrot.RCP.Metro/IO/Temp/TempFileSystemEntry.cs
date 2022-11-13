using System;
using System.IO;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// Used for temporary file system entries which have a local path and are to be disposed
/// </summary>
public abstract class TempFileSystemEntry : IDisposable
{
    protected TempFileSystemEntry()
    {
        // Always make sure the temporary base path exists
        Directory.CreateDirectory(AppFilePaths.TempPath);
    }

    /// <summary>
    /// The path of the temporary entry
    /// </summary>
    public abstract FileSystemPath TempPath { get; }

    ~TempFileSystemEntry() => Dispose(false);

    protected abstract void Dispose(bool disposing);

    public void Dispose()
    {
        // Dispose of unmanaged resources
        Dispose(true);

        // Suppress finalization
        GC.SuppressFinalize(this);
    }
}