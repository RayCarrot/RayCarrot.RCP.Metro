using System.Diagnostics;
using Microsoft.Extensions.Logging;
using RayCarrot.Logging;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// The default trace listener implementation
    /// </summary>
    public class WPFTraceListener : TraceListener
    {
        #region Private Static Properties

        private static WPFTraceListener Instance { get; } = new WPFTraceListener();

        private static LogLevel LogLevel { get; set; }

        #endregion

        #region Public Static Methods

        /// <summary>
        /// Sets up the trace listener
        /// </summary>
        /// <param name="level">The level to log the trace listener messages</param>
        public static void Setup(LogLevel level)
        {
            LogLevel = level;

            if (PresentationTraceSources.DataBindingSource.Listeners.Contains(Instance))
                return;

            PresentationTraceSources.DataBindingSource.Listeners.Add(Instance);
        }

        #endregion

        #region Public Override Methods

        public override void Write(string message)
        {
            RL.Logger?.LogSource(message, LogLevel);
        }

        public override void WriteLine(string message)
        {
            RL.Logger?.LogSource(message, LogLevel);
        }

        #endregion
    }
}