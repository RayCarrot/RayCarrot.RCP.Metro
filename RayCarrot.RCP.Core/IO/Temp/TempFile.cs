using System;
using System.IO;
using RayCarrot.CarrotFramework.Abstractions;
using RayCarrot.IO;

namespace RayCarrot.RCP.Core
{
    /// <summary>
    /// A temporary file
    /// </summary>
    public sealed class TempFile : TempFileSystemEntry
    {
        /// <summary>
        /// Creates a new temporary file
        /// </summary>
        /// <param name="createFile">Indicates if the temporary file should be created</param>
        public TempFile(bool createFile)
        {
            if (createFile)
            {
                // Get the temp path and create the file
                TempPath = Path.GetTempFileName();

                // Get the file info
                var info = TempPath.GetFileInfo();

                // Set the attribute to temporary
                info.Attributes |= FileAttributes.Temporary;
            }
            else
            {
                // Get the temp path
                FileSystemPath tempFile;

                // Get a random temp path until one does not exist
                do
                {
                    tempFile = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + ".tmp");
                } while (tempFile.DirectoryExists);

                // Set the temp path
                TempPath = tempFile;
            }

            RCFCore.Logger?.LogDebugSource($"A new temp file has been created under {TempPath}");
        }

        /// <summary>
        /// The path of the temporary file
        /// </summary>
        public override FileSystemPath TempPath { get; }

        /// <summary>
        /// Removes the temporary file
        /// </summary>
        public override void Dispose()
        {
            try
            {
                // Delete the temp file
                RCFRCPA.File.DeleteFile(TempPath);
            }
            catch (Exception ex)
            {
                ex.HandleError("Deleting temp directory");
            }
        }
    }
}