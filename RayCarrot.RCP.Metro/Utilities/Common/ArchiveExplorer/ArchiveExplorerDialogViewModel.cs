using RayCarrot.Extensions;
using RayCarrot.IO;
using RayCarrot.UI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// View model for an archive explorer dialog
    /// </summary>
    public class ArchiveExplorerDialogViewModel : UserInputViewModel, IDisposable
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="manager">The archive data manager</param>
        /// <param name="filePaths">The archive file paths</param>
        public ArchiveExplorerDialogViewModel(IArchiveDataManager manager, IEnumerable<FileSystemPath> filePaths)
        {
            // Get the manager
            Manager = manager;

            // Get the archives
            Archives = filePaths.Select(x => new ArchiveViewModel(x, manager)).ToArray();

            // Make sure we got an archive
            if (!Archives.Any())
                throw new ArgumentException("At least one archive path needs to be available");

            // Read each archive
            foreach (var archive in Archives)
            {
                // Get the archive directories
                var dirs = manager.GetDirectories(archive.ArchiveFileStream);

                // Add each directory
                foreach (var dir in dirs)
                {
                    // Check if it's the root directory
                    if (dir.DirectoryName == String.Empty)
                    {
                        // Add the files
                        archive.Files.AddRange(dir.Files.Select(x => new ArchiveFileViewModel(x, archive)));

                        continue;
                    }

                    // Keep track of the previous item
                    ArchiveDirectoryViewModel prevItem = archive;

                    // Enumerate each sub directory
                    foreach (string subDir in dir.DirectoryName.Split('\\'))
                    {
                        // Set the previous item and create the item if it doesn't already exist
                        prevItem = prevItem.FindItem(x => x.ID == subDir) ?? prevItem.Add(subDir);
                    }

                    // Add the files
                    prevItem.Files.AddRange(dir.Files.Select(x => new ArchiveFileViewModel(x, archive)));
                }
            }

            // Select and expand the first item
            Archives.First().IsSelected = true;
            Archives.First().IsExpanded = true;
        }

        /// <summary>
        /// The directories
        /// </summary>
        public ArchiveViewModel[] Archives { get; }

        /// <summary>
        /// The archive data manager
        /// </summary>
        public IArchiveDataManager Manager { get; }

        public void Dispose()
        {
            Archives?.ForEach(x => x.Dispose());
        }
    }
}