using System.Collections.Generic;
using System.Diagnostics;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// Default implementation for storing a collection of logs for the <see cref="SessionLogger"/>
    /// </summary>
    public class DefaultSessionLoggerCollection : ISessionLoggerCollection
    {
        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        public DefaultSessionLoggerCollection()
        {
            Logs = new List<LogItem>();
        }

        #endregion

        #region Protected Properties

        /// <summary>
        /// The available logs
        /// </summary>
        protected List<LogItem> Logs { get; }

        /// <summary>
        /// The maximum number of logs to save
        /// </summary>
        protected int MaxLogs { get; set; }

        /// <summary>
        /// Indicates if the max logs have been set up
        /// </summary>
        protected bool HasSetUpMaxLogs { get; set; }

        #endregion

        #region Protected Methods

        /// <summary>
        /// Sets up the maximum number of logs to save
        /// </summary>
        protected void SetupMaxLogs()
        {
            // Limit session logger length to not use up too much memory
            MaxLogs = Debugger.IsAttached ? 200 : 15;

            HasSetUpMaxLogs = true;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Adds a new log item to the collection
        /// </summary>
        /// <param name="logItem">The log item to add</param>
        public void AddLog(LogItem logItem)
        {
            lock (Logs)
            {
                if (!HasSetUpMaxLogs)
                    SetupMaxLogs();

                // Check if we've exceeded the maximum number of logs
                if (MaxLogs != -1 && Logs.Count > MaxLogs)
                    // Remove first log
                    Logs.RemoveAt(0);

                Logs.Add(logItem);
                LogAdded?.Invoke(this, new LogAddedEventArgs(logItem));
            }
        }

        /// <summary>
        /// Clears the collection of logs
        /// </summary>
        public void Clear()
        {
            lock (Logs)
                Logs.Clear();
        }

        /// <summary>
        /// Gets the currently stored logs
        /// </summary>
        /// <returns>The logs</returns>
        public IReadOnlyList<LogItem> GetLogs()
        {
            lock (Logs)
                // Copy the contents of the list and return as read only
                return new List<LogItem>(Logs).AsReadOnly();
        }

        #endregion

        #region Events

        /// <summary>
        /// Occurs when a new log is added
        /// </summary>
        public event LogAddedEventHandler LogAdded;

        #endregion
    }
}