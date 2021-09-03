using System;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// Event arguments for when a new log is added
    /// </summary>
    public class LogAddedEventArgs : EventArgs
    {
        #region Constructors

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="logItem">The new log item</param>
        public LogAddedEventArgs(LogItem logItem)
        {
            NewLogItem = logItem;
        }

        #endregion

        #region Properties

        /// <summary>
        /// The new log item
        /// </summary>
        public LogItem NewLogItem { get; }

        #endregion
    }

    /// <summary>
    /// Event handler for when a new log is added
    /// </summary>
    /// <param name="sender">The sender</param>
    /// <param name="e">The event arguments</param>
    public delegate void LogAddedEventHandler(object sender, LogAddedEventArgs e);
}