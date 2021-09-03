using Microsoft.Extensions.Logging;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// A logger which stores logs for the current session
    /// </summary>
    public class SessionLogger : BaseLogger
    {
        #region Public Methods

        /// <summary>
        /// Logs a new log message
        /// </summary>
        /// <param name="logLevel">The level of the log</param>
        /// <param name="message">The log message</param>
        public override void Log(LogLevel logLevel, string message)
        {
            Services.Logs.AddLog(new LogItem(message, logLevel));
        }

        #endregion
    }
}