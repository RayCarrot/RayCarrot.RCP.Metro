using System.IO;
using BinarySerializer;
using LogLevel = BinarySerializer.LogLevel;

namespace RayCarrot.RCP.Metro;

public class RCPContext : Context
{
    public RCPContext(string basePath, RCPSerializerSettings? settings = null, ISerializerLogger? logger = null, bool noLog = false) 
        : base(
            basePath: basePath, 
            settings: settings ?? new RCPSerializerSettings(), 
            serializerLogger: noLog ? null : logger ?? new RCPSerializerLogger(), 
            fileManager: new RCPFileManager(), 
            systemLogger: new RCPLogger())
    { }

    public class RCPSerializerLogger : ISerializerLogger
    {
        private static readonly SerializerLogWriterProvider _provider = new();

        private bool _isInvalid;
        private StreamWriter? _logWriter;

        protected StreamWriter? LogWriter => _logWriter ??= GetFile();
        
        public bool IsEnabled => !_isInvalid && Services.Data.Binary_IsSerializationLogEnabled;

        public StreamWriter? GetFile()
        {
            StreamWriter? writer = _provider.GetWriter();

            if (writer == null)
                _isInvalid = true;

            return writer;
        }

        public void Log(object? obj)
        {
            if (IsEnabled)
                LogWriter?.WriteLine(obj != null ? obj.ToString() : String.Empty);
        }

        public void Dispose()
        {
            _logWriter?.Dispose();
            _logWriter = null;
        }
    }

    public class RCPFileManager : DefaultFileManager
    {
        /// <summary>
        /// The file share to use when reading files
        /// </summary>
        public FileShare ReadFileShare { get; set; } = FileShare.Read;

        // Force back slash to be used as certain older Win32 APIs don't work well with forward slash
        public override PathSeparatorChar SeparatorCharacter => PathSeparatorChar.BackSlash;

        public override Stream GetFileReadStream(string path) => new FileStream(path, FileMode.Open, FileAccess.Read, ReadFileShare);
    }

    private class RCPLogger : ISystemLogger
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public void Log(LogLevel logLevel, object? log, params object?[] args)
        {
            NLog.LogLevel nlogLevel = logLevel switch
            {
                LogLevel.Trace => NLog.LogLevel.Trace,
                LogLevel.Debug => NLog.LogLevel.Debug,
                LogLevel.Info => NLog.LogLevel.Info,
                LogLevel.Warning => NLog.LogLevel.Warn,
                LogLevel.Error => NLog.LogLevel.Error,
                _ => NLog.LogLevel.Info,
            };

            Logger.Log(nlogLevel, () => String.Format($"BinarySerializer: {log}", args));
        }
    }
}

public class RCPSerializerSettings : SerializerSettings
{
    public RCPSerializerSettings()
    {
        // Disable caching by default as it's rarely used in RCP
        IgnoreCacheOnRead = true;
    }
}