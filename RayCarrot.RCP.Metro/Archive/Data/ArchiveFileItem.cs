using RayCarrot.IO;
using System;
using System.IO;
using System.Linq;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// File item data for the Archive Explorer
    /// </summary>
    public class ArchiveFileItem : IDisposable
    {
        static ArchiveFileItem()
        {
            // IDEA: Move this somewhere else?
            // Set the supported file types
            FileTypes = new IArchiveFileType[]
            {
                new ArchiveFileType_GF(),
                new ArchiveFileType_Image(),
                new ArchiveFileType_DDSUbiArtTex(),
                new ArchiveFileType_GXTUbiArtTex(),
                new ArchiveFileType_GTXUbiArtTex(),
                new ArchiveFileType_PVRUbiArtTex(),
                new ArchiveFileType_GNFUbiArtTex(),
                new ArchiveFileType_Xbox360UbiArtTex(),
            };

            // Set default file type
            DefaultFileType = new ArchiveFileType_Default();
        }

        public ArchiveFileItem(IArchiveDataManager manager, string fileName, string directory, object archiveEntry)
        {
            Manager = manager;
            FileName = fileName;
            Directory = directory;
            ArchiveEntry = archiveEntry;
        }

        protected IArchiveDataManager Manager { get; }

        public string FileName { get; }
        public string Directory { get; }
        public object ArchiveEntry { get; }

        public Stream PendingImport { get; protected set; }
        public bool IsPendingImport => PendingImport != null;

        public FileExtension FileExtension => new FileExtension(FileName);

        public ArchiveFileStream GetFileData(IDisposable generator)
        {
            // Get the stream
            var stream = IsPendingImport ? new ArchiveFileStream(PendingImport, false) : new ArchiveFileStream(() => Manager.GetFileData(generator, ArchiveEntry), true);
            
            // Seek to the beginning
            stream.SeekToBeginning();

            // Return the stream
            return stream;
        }

        public void SetPendingImport(Stream import)
        {
            PendingImport?.Dispose();
            PendingImport = import;
        }

        public IArchiveFileType GetFileType(ArchiveFileStream stream)
        {
            // Get types supported by the current manager
            var types = FileTypes.Where(x => x.IsSupported(Manager)).ToArray();

            // First attempt to find matching file type based off of the file extension to avoid having to read the file
            var match = types.FirstOrDefault(x => x.IsOfType(FileExtension));

            // If no match, check the data
            if (match == null)
            {
                // Get the file data stream
                Stream fileStream = stream.Stream;

                // Find a match from the stream data
                match = types.FirstOrDefault(x => x.IsOfType(FileExtension, fileStream, Manager));
            }

            // If still null, set to default
            if (match == null)
                match = DefaultFileType;

            // Return the type
            return match;
        }

        private static IArchiveFileType[] FileTypes { get; }
        private static IArchiveFileType DefaultFileType { get; }

        public void Dispose()
        {
            PendingImport?.Dispose();
        }
    }
}