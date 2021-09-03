using System;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// Indicates which default loggers should be used
    /// </summary>
    [Flags]
    public enum DefaultLoggers
    {
        /// <summary>
        /// No default loggers
        /// </summary>
        None = 0,

        /// <summary>
        /// Console logger
        /// </summary>
        Console = 1,

        /// <summary>
        /// Debug logger
        /// </summary>
        Debug = 2,

        /// <summary>
        /// Session logger
        /// </summary>
        Session = 4
    }
}