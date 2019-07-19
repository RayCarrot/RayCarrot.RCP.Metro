using System.IO;
using RayCarrot.IO;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// Contains information regarding a directory to include in backup
    /// </summary>
    public class BackupDir
    {
        /// <summary>
        /// The directory to include in backup
        /// </summary>
        public FileSystemPath DirPath { get; set; }

        /// <summary>
        /// The search option to use when finding files and sub directories
        /// </summary>
        public SearchOption SearchOption { get; set; } = SearchOption.AllDirectories;

        /// <summary>
        /// File extension to filter by
        /// </summary>
        public string ExtensionFilter { get; set; } = "*";

        /// <summary>
        /// The ID of the <see cref="BackupDir"/>
        /// </summary>
        public string ID { get; set; }

        /// <summary>
        /// Gets a value indicating if the current instance should include the entire directory and its sub content
        /// </summary>
        /// <returns>True if the current instance should include the entire directory and its sub content, otherwise false</returns>
        public bool IsEntireDir() => (ExtensionFilter == null || ExtensionFilter == "*") && SearchOption == SearchOption.AllDirectories;
    }
}