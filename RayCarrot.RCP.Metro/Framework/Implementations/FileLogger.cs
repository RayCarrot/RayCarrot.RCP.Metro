using ByteSizeLib;
using Microsoft.Extensions.Logging;
using RayCarrot.CarrotFramework;
using RayCarrot.CarrotFramework.Abstractions;
using RayCarrot.IO;
using System;
using System.Globalization;
using System.IO;
using System.Windows;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// A file logger to log to a file
    /// </summary>
    public class FileLogger : BaseLogger
    {
        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        public FileLogger()
        {
            // Set the log level
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
            // Make sure the application is running
            if (Application.Current == null)
                return;

            // Lock to the current application
            lock (Application.Current)
            {
                // First time the file logger gets used we check if the file should be reset
                if (!HasBeenSetUp)
                {
                    // Attempt to remove log file if over 2 Mb
                    try
                    {
                        if (CommonPaths.LogFile.FileExists && CommonPaths.LogFile.GetSize() > ByteSize.FromMegaBytes(2))
                            File.Delete(CommonPaths.LogFile);
                    }
                    catch (Exception ex)
                    {
                        ex.HandleCritical("Removing log file due to size");
                    }

                    try
                    {
                        // Create the parent directory if it doesn't exist
                        if (!CommonPaths.LogFile.Parent.DirectoryExists)
                            Directory.CreateDirectory(CommonPaths.LogFile.Parent);
                    }
                    catch (Exception ex)
                    {
                        ex.HandleCritical("Creating log file directory");
                    }

                    try
                    {
                        // Open the file stream
                        App.Current.AppLogFileStream = File.AppendText(CommonPaths.LogFile);
                    }
                    catch (Exception ex)
                    {
                        ex.HandleCritical("Opening log file");
                    }

                    // Indicate that the file logger has been set up
                    HasBeenSetUp = true;
                }

                try
                {
                    // Append the log to the log file, forcing the culture info to follow the Swedish standard
                    App.Current.AppLogFileStream?.WriteLine($"{DateTime.Now.ToString(new CultureInfo("sv-SE"))} [{logLevel}] {message}");
                }
                catch (Exception ex)
                {
                    // If the file logger crashes, do not attempt to log with it any more
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