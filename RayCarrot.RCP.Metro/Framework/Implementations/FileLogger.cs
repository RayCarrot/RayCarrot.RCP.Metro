﻿using System;
using System.IO;
using Microsoft.Extensions.Logging;
using RayCarrot.CarrotFramework;

namespace RayCarrot.RCP.Metro
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
            try
            {
                if (!CommonPaths.LogFile.Parent.DirectoryExists)
                    Directory.CreateDirectory(CommonPaths.LogFile.Parent);

                File.AppendAllText(CommonPaths.LogFile, $"{DateTime.Now} [{logLevel}] {message}" + Environment.NewLine);
            }
            catch (Exception ex)
            {
                FileLoggerLogLevel = LogLevel.None;
                LoggerLogLevel = FileLoggerLogLevel;
                ex.HandleCritical("File logger");
            }
        }

        #endregion

        #region Protected Static Properties

        /// <summary>
        /// Indicates the log level to log
        /// </summary>
        protected static LogLevel FileLoggerLogLevel { get; set; } = LogLevel.Information;

        #endregion
    }
}