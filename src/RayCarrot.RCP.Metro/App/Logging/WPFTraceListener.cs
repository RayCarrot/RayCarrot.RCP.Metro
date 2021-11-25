#nullable disable
using NLog;
using System.Diagnostics;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// The default trace listener implementation
/// </summary>
public class WPFTraceListener : TraceListener
{
    #region Private Static Properties

    private static WPFTraceListener Instance { get; } = new WPFTraceListener();

    #endregion

    #region Logger

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    #endregion

    #region Public Static Methods

    /// <summary>
    /// Sets up the trace listener
    /// </summary>
    public static void Setup()
    {
        if (PresentationTraceSources.DataBindingSource.Listeners.Contains(Instance))
            return;

        PresentationTraceSources.DataBindingSource.Listeners.Add(Instance);
    }

    #endregion

    #region Public Override Methods

    public override void Write(string message)
    {
        Logger.Warn(message);
    }

    public override void WriteLine(string message)
    {
        Logger.Warn(message);
    }

    #endregion
}