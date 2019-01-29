using RayCarrot.CarrotFramework;
using System.Diagnostics;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// The Rayman Control Panel trace listener implementation
    /// </summary>
    public class RCPTraceListener : TraceListener
    {
        public override void Write(string message)
        {
            RCF.Logger.LogDebugSource(message);
        }

        public override void WriteLine(string message)
        {
            RCF.Logger.LogDebugSource(message);
        }
    }
}