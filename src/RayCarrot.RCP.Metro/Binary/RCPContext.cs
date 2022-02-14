using System;
using System.IO;
using System.Text;
using BinarySerializer;
using NLog;
using ILogger = BinarySerializer.ILogger;

namespace RayCarrot.RCP.Metro;

public class RCPContext : Context
{
    public RCPContext(string basePath, RCPSerializerSettings? settings = null, ISerializerLog? log = null) 
        : base(
            basePath: basePath, 
            settings: settings ?? new RCPSerializerSettings(), 
            serializerLog: log ?? new RCPSerializerLog(), 
            fileManager: new RCPFileManager(), 
            logger: new RCPLogger())
    { }

    public class RCPSerializerLog : ISerializerLog
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

    private class RCPLogger : ILogger
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public void Log(object log, params object[] args)
        {
            Logger.Info($"BinarySerializer: {log}", args);
        }

        public void LogWarning(object log, params object[] args)
        {
            Logger.Info($"BinarySerializer: {log}", args);
        }

        public void LogError(object log, params object[] args)
        {
            Logger.Info($"BinarySerializer: {log}", args);
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