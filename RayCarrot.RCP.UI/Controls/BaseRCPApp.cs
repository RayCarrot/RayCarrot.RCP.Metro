using System.Threading;
using RayCarrot.WPF;

namespace RayCarrot.RCP.UI
{
    /// <summary>
    /// The base WPF application for a Rayman Control Panel app
    /// </summary>
    public abstract class BaseRCPApp : BaseRCFApp
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="useMutex">Indicates if a <see cref="Mutex"/> should be used to only allow a single instance of the application.
        /// This requires a valid GUID in the entry assembly.</param>
        /// <param name="splashScreenResourceName">The resource name for a splash screen if one is to be used</param>
        protected BaseRCPApp(bool useMutex, string splashScreenResourceName = null) : base(useMutex, splashScreenResourceName)
        {

        }
    }
}