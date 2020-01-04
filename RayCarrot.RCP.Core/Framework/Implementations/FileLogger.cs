using System;
using System.Globalization;
using System.IO;
using System.Windows;
using ByteSizeLib;
using Microsoft.Extensions.Logging;
using RayCarrot.CarrotFramework;
using RayCarrot.CarrotFramework.Abstractions;
using RayCarrot.IO;

namespace RayCarrot.RCP.Core
{
    /// <summary>
    /// A file logger to log to a file
    /// </summary>
    public class FileLogger : BaseLogger
    {
        #region Constructor

        public FileLogger()
        {
            LoggerLogLevel = FileLoggerLogLevel;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Writes a log entry
        /// </summary>
        /// <param name="logLevel">Entry will be written on this level</param>
        /// <param name="message">The message to write</param>
        public override void Log(LogLevel logLevel, string message)
        {
            if (Application.Current == null)
                return;

            lock (Application.Current)
            {
                if (!HasBeenSetUp)
                {
                    // Attempt to remove log file if over 2 Mb
                    try
                    {
                        if (RCFRCPC.Path.LogFile.FileExists && RCFRCPC.Path.LogFile.GetSize() > ByteSize.FromMegaBytes(2))
                            File.Delete(RCFRCPC.Path.LogFile);
                    }
                    catch (Exception ex)
                    {
                        ex.HandleCritical("Removing log file due to size");
                    }

                    HasBeenSetUp = true;
                }

                try
                {
                    if (!RCFRCPC.Path.LogFile.Parent.DirectoryExists)
                        Directory.CreateDirectory(RCFRCPC.Path.LogFile.Parent);

                    // Append the log to the log file, forcing the culture info to follow the Swedish standard
                    File.AppendAllText(RCFRCPC.Path.LogFile, $@"{DateTime.Now.ToString(new CultureInfo("sv-SE"))} [{logLevel}] {message}" + Environment.NewLine);
                }
                catch (Exception ex)
                {
                    FileLoggerLogLevel = LogLevel.None;
                    LoggerLogLevel = FileLoggerLogLevel;
                    ex.HandleCritical("File logger");
                }
            }
        }

        #endregion

        #region Private Static Properties

        /// <summary>
        /// Indicates if the logger has been set up
        /// </summary>
        private static bool HasBeenSetUp { get; set; }

        #endregion

        #region Public Static Properties

        /// <summary>
        /// Indicates the log level to log
        /// </summary>
        public static LogLevel FileLoggerLogLevel { get; set; } = LogLevel.Information;

        #endregion
    }
}