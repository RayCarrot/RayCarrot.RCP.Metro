using System;
using RayCarrot.IO;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// Defines an existing RCP backup to restore from
    /// </summary>
    public class RCPBackup
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="path">The backup path</param>
        public RCPBackup(FileSystemPath path)
        {
            Path = path;
            IsCompressed = Path.FileExists;

            var pathName = Path.RemoveFileExtension().Name;

            BackupVersion = pathName[pathName.Length - 3] == '-'
                ? Int32.TryParse(pathName.Substring(pathName.Length - 2), out int result) ? result : 0
                : 0;
        }

        /// <summary>
        /// The backup path
        /// </summary>
        public FileSystemPath Path { get; }

        /// <summary>
        /// Indicates if the backup is compressed
        /// </summary>
        public bool IsCompressed { get; }

        /// <summary>
        /// The backup version
        /// </summary>
        public int BackupVersion { get; }
    }
}