using System;
using System.IO;
using System.Linq;
using RayCarrot.IO;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// File item data for the Archive Explorer
    /// </summary>
    public class ArchiveFileItem
    {
        public ArchiveFileItem(string fileName, string directory, object archiveEntry)
        {
            FileName = fileName;
            Directory = directory;
            ArchiveEntry = archiveEntry;
        }

        private IArchiveFileType FileType { get; set; }

        public string FileName { get; }
        public string Directory { get; }
        public object ArchiveEntry { get; }

        public FileExtension FileExtension => new FileExtension(FileName);

        protected IArchiveFileType GetFileType(Func<Stream> getDataStream)
        {
            // Check if we already got the type
            if (FileType != null)
                return FileType;

            // TODO-UPDATE: We don't want to read the file twice (thumbnail + filetype) - use same decoded data for both - pass in here?

            // First attempt to find matching file type based off of the file extension to avoid having to read the file
            var match = FileTypes.First(x => x.IsOfType(FileExtension));

            // If no match, check the data
            if (match == null)
                match = FileTypes.First(x => x.IsOfType(FileExtension, getDataStream()));

            // If still null, set to default
            if (match == null)
                match = new ArchiveFileType_Default()
                {
                    // TODO-UPDATE: Implement
                    //SerializerSettings = 
                };

            // Return and set the type
            return FileType = match;
        }

        // TODO-UPDATE: Move elsewhere
        private static readonly IArchiveFileType[] FileTypes = new IArchiveFileType[]
        {

        };
    }
}