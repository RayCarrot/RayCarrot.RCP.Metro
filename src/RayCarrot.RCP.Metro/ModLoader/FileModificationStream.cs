using System.IO;

namespace RayCarrot.RCP.Metro.ModLoader;

public class FileModificationStream : IDisposable
{
    public FileModificationStream(Func<Stream> getStreamFunc, Action<Stream?>? onStreamClosed = null)
    {
        _getStreamFunc = getStreamFunc;
        _onStreamClosed = onStreamClosed;
    }

    private readonly Func<Stream> _getStreamFunc;
    private readonly Action<Stream?>? _onStreamClosed;
    private Stream? _stream;
    private bool _isDeleted;

    public Stream Stream => _stream ??= _getStreamFunc();

    public void DeleteFile()
    {
        _isDeleted = true;
    }

    public void Dispose()
    {
        if (_isDeleted)
        {
            _stream?.Dispose();
            _onStreamClosed?.Invoke(null);
        }
        else if (_stream != null)
        {
            _onStreamClosed?.Invoke(_stream);
            _stream.Dispose();
        }
    }
}