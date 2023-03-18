using System.IO;
using System.Text;

namespace RayCarrot.RCP.Metro;

public class SerializerLogWriterProvider
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    private readonly object _lock = new();
    private readonly HashSet<int> _lockedFiles = new();
    private readonly HashSet<int> _createdFiles = new();

    public StreamWriter? GetWriter()
    {
        lock (_lock)
        {
            int file = 0;

            while (_lockedFiles.Contains(file))
                file++;

            _lockedFiles.Add(file);

            string filePath = GetFilePath(file);
            FileMode mode = _createdFiles.Contains(file) ? FileMode.Append : FileMode.Create;

            _createdFiles.Add(file);

            try
            {
                FileStream fileStream = File.Open(filePath, mode, FileAccess.Write, FileShare.Read);
                return new SerializerLogWriter(this, file, fileStream, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Opening serializer log file with mode {0}", mode);
                return null;
            }

        }
    }

    private string GetFilePath(int file)
    {
        string filePath = Services.Data.Binary_BinarySerializationFileLogPath;
        
        int index = filePath.LastIndexOf('.');
        
        if (index == -1)
            index = filePath.Length;
        
        return filePath.Insert(index, $"_{file}");
    }

    private void ReleaseFile(int file)
    {
        lock (_lock)
        {
            _lockedFiles.Remove(file);
        }
    }

    private class SerializerLogWriter : StreamWriter
    {
        public SerializerLogWriter(SerializerLogWriterProvider provider, int file, Stream stream, Encoding encoding) : base(stream, encoding)
        {
            _provider = provider;
            _file = file;
        }

        private readonly SerializerLogWriterProvider _provider;
        private readonly int _file;

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
                _provider.ReleaseFile(_file);
        }
    }
}