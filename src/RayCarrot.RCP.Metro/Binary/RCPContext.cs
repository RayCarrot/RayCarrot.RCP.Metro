using System;
using System.IO;
using System.Text;
using BinarySerializer;
using NLog;
using ILogger = BinarySerializer.ILogger;

namespace RayCarrot.RCP.Metro;

public class RCPContext : Context
{
    public RCPContext(string basePath, Encoding? defaultStringEncoding = null) 
        : base(basePath, new RCPSerializerSettings(defaultStringEncoding), new RCPSerializerLog(), new RCPFileManager(), new RCPLogger())
    { }

    private class RCPSerializerSettings : SerializerSettings
    {
        public RCPSerializerSettings(Encoding? defaultStringEncoding = null) : base(defaultStringEncoding: defaultStringEncoding)
        {
            
        }

        public override bool CreateBackupOnWrite => false;
        public override bool SavePointersForRelocation => false;
    }

    public class RCPSerializerLog : ISerializerLog
    {
        private static bool _hasBeenCreated;
        public bool IsEnabled => !Services.Data.Binary_BinarySerializationFileLogPath.FullPath.IsNullOrWhiteSpace();

        private StreamWriter? _logWriter;

        protected StreamWriter LogWriter => _logWriter ??= GetFile();

        public string LogFile => Services.Data.Binary_BinarySerializationFileLogPath;

        public StreamWriter GetFile()
        {
            StreamWriter w = new(File.Open(LogFile, _hasBeenCreated ? FileMode.Append : FileMode.Create, FileAccess.Write, FileShare.ReadWrite), Encoding.UTF8);
            _hasBeenCreated = true;
            return w;
        }

        public void Log(object? obj)
        {
            if (IsEnabled)
                LogWriter.WriteLine(obj != null ? obj.ToString() : String.Empty);
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

        public void Log(object log)
        {
            Logger.Info("BinarySerializer: {0}", log);
        }

        public void LogWarning(object log)
        {
            Logger.Warn("BinarySerializer: {0}", log);
        }

        public void LogError(object log)
        {
            Logger.Error("BinarySerializer: {0}", log);
        }
    }
}