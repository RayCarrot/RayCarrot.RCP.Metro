using System.Collections.Generic;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// Defines a collection of logs for a SessionLogger
    /// </summary>
    public interface ISessionLoggerCollection
    {
        #region Methods

        /// <summary>
        /// Adds a new log item to the collection
        /// </summary>
        /// <param name="logItem">The log item to add</param>
        void AddLog(LogItem logItem);

        /// <summary>
        /// Clears the collection of logs
        /// </summary>
        void Clear();

        /// <summary>
        /// Gets the currently stored logs
        /// </summary>
        /// <returns>The logs</returns>
        IReadOnlyList<LogItem> GetLogs();

        #endregion

        #region Events

        /// <summary>
        /// Occurs when a new log is added
        /// </summary>
        event LogAddedEventHandler LogAdded;

        #endregion
    }
}