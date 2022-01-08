using System;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// Used for temporary file system entries which have a local path and are to be disposed
    /// </summary>
    public abstract class TempFileSystemEntry : IDisposable
    {
        /// <summary>
        /// The path of the temporary entry
        /// </summary>
        public abstract FileSystemPath TempPath { get; }

        /// <summary>
        /// Removes the temporary entry
        /// </summary>
        public abstract void Dispose();
    }
}