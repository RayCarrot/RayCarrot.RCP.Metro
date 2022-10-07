using System;
using System.IO;
using System.Text;
using BinarySerializer;
using NLog;
using LogLevel = BinarySerializer.LogLevel;

namespace RayCarrot.RCP.Metro;

public class RCPContext : Context
{
    public RCPContext(string basePath, RCPSerializerSettings? settings = null, ISerializerLogger? logger = null, bool noLog = false) 
        : base(
            basePath: basePath, 
            settings: settings ?? new RCPSerializerSettings(), 
            serializerLogger: noLog ? null : logger ?? LogInstance, 
            fileManager: new RCPFileManager(), 
            systemLogger: new RCPLogger())
    { }

    // Use a static log instance so that multiple contexts can be open and log at the same time without conflicts
    private static readonly RCPSerializerLogger LogInstance = new();

    public class RCPSerializerLogger : ISerializerLogger
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private static bool _hasBeenCreated;

        private bool _isInvalid;

        public bool IsEnabled => !_isInvalid && Services.Data.Binary_IsSerializationLogEnabled;

        private StreamWriter? _logWriter;

        protected StreamWriter? LogWriter => _logWriter ??= GetFile();

        public string LogFile => Services.Data.Binary_BinarySerializationFileLogPath;

        public StreamWriter? GetFile()
        {
            var mode = _hasBeenCreated ? FileMode.Append : FileMode.Create;

            try
            {
                StreamWriter w = new(File.Open(LogFile, mode, FileAccess.Write, FileShare.ReadWrite), Encoding.UTF8);
                _hasBeenCreated = true;
                return w;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Opening serializer log file with mode {0}", mode);
                _isInvalid = true;
                return null;
            }
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
        // Force back slash to be used as certain older Win32 APIs don't work well with forward slash
        public override PathSeparatorChar SeparatorCharacter => PathSeparatorChar.BackSlash;
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