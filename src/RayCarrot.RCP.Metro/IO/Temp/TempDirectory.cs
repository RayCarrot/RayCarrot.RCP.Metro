using System;
using System.IO;
using NLog;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// A temporary directory
    /// </summary>
    public sealed class TempDirectory : TempFileSystemEntry
    {
        /// <summary>
        /// Creates a new temporary directory
        /// </summary>
        /// <param name="createDir">Indicates if the directory should be created</param>
        public TempDirectory(bool createDir)
        {
            // Get the temp path
            FileSystemPath tempDir;

            // Get a random temp path until one does not exist
            do
            {
                tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            } while (tempDir.DirectoryExists);

            // Set the temp path
            TempPath = tempDir;

            if (createDir)
                // Create the directory
                Directory.CreateDirectory(TempPath);

            Logger.Debug("A new temp directory has been created under {0}", TempPath);
        }

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// The path of the temporary directory
        /// </summary>
        public override FileSystemPath TempPath { get; }

        /// <summary>
        /// Removes the temporary directory
        /// </summary>
        public override void Dispose()
        {
            try
            {
                // Delete the temp directory
                TempPath.DeleteDirectory();
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Deleting temp directory");
            }
        }
    }
}