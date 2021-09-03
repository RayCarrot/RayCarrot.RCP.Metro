using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// A log item
    /// </summary>
    [DebuggerDisplay("{" + nameof(LogLevel) + "}: {" + nameof(Message) + "}")]
    public class LogItem
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="message">The log message</param>
        /// <param name="logLevel">The log level</param>
        public LogItem(string message, LogLevel logLevel)
        {
            Message = message;
            LogLevel = logLevel;
        }

        /// <summary>
        /// The log message
        /// </summary>
        public string Message { get; }

        /// <summary>
        /// The log level
        /// </summary>
        public LogLevel LogLevel { get; }
    }
}