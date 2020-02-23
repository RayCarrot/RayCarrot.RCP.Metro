using System.Collections.Generic;
using RayCarrot.IO;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// Data for creating an archive
    /// </summary>
    public class ArchiveCreationData
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="archive">The archive data</param>
        /// <param name="fileEntries">The archive file entry import data</param>
        public ArchiveCreationData(object archive, IEnumerable<FileEntryImportData> fileEntries)
        {
            Archive = archive;
            FileEntries = fileEntries;
        }

        /// <summary>
        /// The archive data
        /// </summary>
        public object Archive { get; }

        /// <summary>
        /// The archive file entry import data
        /// </summary>
        public IEnumerable<FileEntryImportData> FileEntries { get; }

        /// <summary>
        /// File entry import data
        /// </summary>
        public class FileEntryImportData
        {
            /// <summary>
            /// Default constructor
            /// </summary>
            /// <param name="fileEntry">The file entry</param>
            /// <param name="relativeImportFilePath">The relative file path to import from</param>
            public FileEntryImportData(object fileEntry, FileSystemPath relativeImportFilePath)
            {
                FileEntry = fileEntry;
                RelativeImportFilePath = relativeImportFilePath;
            }

            /// <summary>
            /// The file entry
            /// </summary>
            public object FileEntry { get; }

            /// <summary>
            /// The relative file path to import from
            /// </summary>
            public FileSystemPath RelativeImportFilePath { get; }
        }
    }
}