using RayCarrot.IO;
using System;
using System.IO;
using System.Windows;

namespace RayCarrot.RCP.Metro
{
    //TODO: Finish this

    /// <summary>
    /// Interface for a RCP utility
    /// </summary>
    public interface IRCPUtility
    {
        /// <summary>
        /// The header for the utility. This property is retrieved again when the current culture is changed.
        /// </summary>
        string DisplayHeader { get; }

        /// <summary>
        /// The utility information text (optional). This property is retrieved again when the current culture is changed.
        /// </summary>
        string InfoText { get; }

        /// <summary>
        /// The utility warning text (optional). This property is retrieved again when the current culture is changed.
        /// </summary>
        string WarningText { get; }

        /// <summary>
        /// The utility UI content
        /// </summary>
        UIElement UIContent { get; }

        /// <summary>
        /// Indicates if the utility requires administration privileges
        /// </summary>
        bool RequiresAdmin { get; }

        /// <summary>
        /// Indicates if the utility is available to the user
        /// </summary>
        bool IsAvailable { get; }

        /// <summary>
        /// The developers of the utility
        /// </summary>
        string Developers { get; }

        /// <summary>
        /// The game which the utility was made for
        /// </summary>
        Games Game { get; }

        /// <summary>
        /// Retrieves a list of applied utilities from this utility
        /// </summary>
        /// <returns>The applied utilities</returns>
        string[] GetAppliedUtilities();
    }


    public abstract class TempFileSystemEntry : IDisposable
    {
        public abstract FileSystemPath TempPath { get; }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public abstract void Dispose();
    }

    public class TempFile : TempFileSystemEntry
    {
        public TempFile()
        {
            // Get the temp path
            TempPath = Path.GetTempFileName();
        }

        public override FileSystemPath TempPath { get; }

        public override void Dispose()
        {
            // Delete the temp file
            RCFRCP.File.DeleteFile(TempPath);
        }
    }
}
