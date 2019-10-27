using System;
using System.Threading.Tasks;
using RayCarrot.IO;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// A finder item result base
    /// </summary>
    public abstract class BaseFinderResult
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="installLocation">The install location</param>
        /// <param name="handledAction">An optional action to add when the item gets handled</param>
        /// <param name="handledParameter">Optional parameter for the <see cref="HandledAction"/></param>
        protected BaseFinderResult(FileSystemPath installLocation, Action<FileSystemPath, object> handledAction, object handledParameter)
        {
            InstallLocation = installLocation;
            HandledAction = handledAction;
            HandledParameter = handledParameter;
        }

        /// <summary>
        /// The install location
        /// </summary>
        public FileSystemPath InstallLocation { get; }

        /// <summary>
        /// Optional parameter for the <see cref="HandledAction"/>
        /// </summary>
        public object HandledParameter { get; }

        /// <summary>
        /// An optional action to add when the item gets handled
        /// </summary>
        public Action<FileSystemPath, object> HandledAction { get; }

        /// <summary>
        /// Handles the found item
        /// </summary>
        /// <returns>The task</returns>
        public abstract Task HandleItemAsync();
    }
}