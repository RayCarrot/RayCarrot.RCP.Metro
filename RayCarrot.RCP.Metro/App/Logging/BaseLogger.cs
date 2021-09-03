using Microsoft.Extensions.Logging;
using System;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// A base logger for creating loggers
    /// </summary>
    public abstract class BaseLogger : ILogger
    {
        /// <summary>
        /// The <see cref="LogLevel"/> to log
        /// </summary>
        public LogLevel LoggerLogLevel { get; set; }

        /// <summary>
        /// Begins a logical operation scope
        /// </summary>
        /// <typeparam name="TState">The type of identifier for the scope</typeparam>
        /// <param name="state">The identifier for the scope</param>
        /// <returns>An IDisposable that ends the logical operation scope on dispose</returns>
        public virtual IDisposable BeginScope<TState>(TState state)
        {
            return null;
        }

        /// <summary>
        /// Checks if the given logLevel is enabled
        /// </summary>
        /// <param name="logLevel">Level to be checked</param>
        /// <returns>True if enabled</returns>
        public virtual bool IsEnabled(LogLevel logLevel)
        {
            return logLevel >= LoggerLogLevel;
        }

        /// <summary>
        /// Writes a log entry
        /// </summary>
        /// <typeparam name="TState">The type of entry</typeparam>
        /// <param name="logLevel">Entry will be written on this level</param>
        /// <param name="eventId">Id of the event</param>
        /// <param name="state">The entry to be written. Can be also an object.</param>
        /// <param name="exception">The exception related to this entry</param>
        /// <param name="formatter">Function to create a string message of the state and exception</param>
        public virtual void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (!IsEnabled(logLevel))
                return;

            string message = formatter(state, exception);

            Log(logLevel, message);
        }

        /// <summary>
        /// Writes a log entry
        /// </summary>
        /// <param name="logLevel">Entry will be written on this level</param>
        /// <param name="message">The message to write</param>
        public abstract void Log(LogLevel logLevel, string message);
    }
}