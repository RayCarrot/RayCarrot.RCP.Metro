using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// A base log provider
    /// </summary>
    /// <typeparam name="L">The logger type</typeparam>
    public class BaseLogProvider<L> : ILoggerProvider
        where L : ILogger, new()
    {
        #region Protected Members

        /// <summary>
        /// Keeps track of the loggers already created
        /// </summary>
        protected readonly ConcurrentDictionary<string, L> mLoggers = new ConcurrentDictionary<string, L>();

        #endregion

        #region ILoggerProvider Implementation

        /// <summary>
        /// Creates a new logger based on the category name
        /// </summary>
        /// <param name="categoryName">The category name of this logger</param>
        /// <returns>The logger</returns>
        public ILogger CreateLogger(string categoryName)
        {
            // Get or create the logger for this category
            return mLoggers.GetOrAdd(categoryName, name => new L());
        }

        /// <summary>
        /// Dispose
        /// </summary>
        public virtual void Dispose()
        {
            // Clear the list of loggers
            mLoggers.Clear();
        }

        #endregion
    }
}