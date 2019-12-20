using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using RayCarrot.Rayman;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// Archive data manager for a Rayman 2 .cnt file
    /// </summary>
    public class R2CntArchiveDataManager : IArchiveDataManager
    {
        /// <summary>
        /// Gets the available directories from the archive along with their contents
        /// </summary>
        /// <param name="archiveFileStream">The file stream for the archive</param>
        /// <returns>The directories</returns>
        public IEnumerable<ArchiveDirectory> GetDirectories(Stream archiveFileStream)
        {
            // Read the file data
            var data = new R2CntSerializer().Deserialize(archiveFileStream);

            // Add the directories to the collection
            for (var i = -1; i < data.Directories.Length; i++)
                // Return each directory with the available files, including the root directory
                yield return new ArchiveDirectory(i == -1 ? String.Empty : data.Directories[i], data.Files.Where(x => x.DirectoryIndex == i).Select(f => new R2CntArchiveFileData(f) as IArchiveFileData).ToArray());
        }
    }
}