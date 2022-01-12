using System;
using System.IO;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// A file stream for the Archive Explorer
/// </summary>
public class ArchiveFileStream : IDisposable
{
    /// <summary>
    /// Constructor for a file where the stream has not yet been obtained
    /// </summary>
    /// <param name="getStream">The function for getting the stream</param>
    /// <param name="name">The stream name, used for logging and debugging</param>
    /// <param name="shouldDispose">Indicates if the stream should be disposed</param>
    public ArchiveFileStream(Func<Stream> getStream, string name, bool shouldDispose)
    {
        GetStream = getStream;
        Name = name;
        ShouldDispose = shouldDispose;
    }

    /// <summary>
    /// Constructor for a file where the stream is available
    /// </summary>
    /// <param name="stream">The stream</param>
    /// <param name="name">The stream name, used for logging and debugging</param>
    /// <param name="shouldDispose">Indicates if the stream should be disposed</param>
    public ArchiveFileStream(Stream stream, string name, bool shouldDispose)
    {
        _stream = stream;
        GetStream = () => _stream;
        Name = name;
        ShouldDispose = shouldDispose;
    }

    private Stream? _stream;

    /// <summary>
    /// The function for getting the stream
    /// </summary>
    protected Func<Stream> GetStream { get; }

    /// <summary>
    /// Indicates if the stream should be disposed
    /// </summary>
    protected bool ShouldDispose { get; }

    /// <summary>
    /// The stream name, used for logging and debugging
    /// </summary>
    public string Name { get; }
        
    /// <summary>
    /// The stream
    /// </summary>
    public Stream Stream => _stream ??= GetStream();

    /// <summary>
    /// Seeks to the beginning of the stream
    /// </summary>
    public void SeekToBeginning()
    {
        // If the stream has not been created there is no need to create it just to reset the position (we always assume that a newly created stream has the position reset!)
        if (_stream == null)
            return;

        Stream.Position = 0;
    }

    /// <summary>
    /// Disposes the stream if set to do so
    /// </summary>
    public void Dispose()
    {
        if (ShouldDispose)
            _stream?.Dispose();
    }
}