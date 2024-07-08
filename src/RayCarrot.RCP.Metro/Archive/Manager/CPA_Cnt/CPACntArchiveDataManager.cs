using System.IO;
using BinarySerializer;
using BinarySerializer.OpenSpace;

namespace RayCarrot.RCP.Metro.Archive.CPA;

/// <summary>
/// Archive data manager for a CPA .cnt file
/// </summary>
public class CPACntArchiveDataManager : IArchiveDataManager
{
    #region Constructor

    /// <summary>
    /// Default constructor
    /// </summary>
    /// <param name="settings">The settings when serializing the data</param>
    /// <param name="gameInstallation">The game installation or null if one is not specified</param>
    /// <param name="cpaTextureSyncItems">Optional CPA texture sync items for when repacking</param>
    public CPACntArchiveDataManager(OpenSpaceSettings settings, GameInstallation? gameInstallation, CPATextureSyncDataItem[]? cpaTextureSyncItems)
    {
        Settings = settings;
        GameInstallation = gameInstallation;
        CPATextureSyncItems = cpaTextureSyncItems;

        Context = new RCPContext(String.Empty, new RCPSerializerSettings()
        {
            DefaultEndianness = settings.GetEndian
        });
        Context.AddSettings(settings);
    }

    #endregion

    #region Logger

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    #endregion

    #region Private Properties

    /// <summary>
    /// Optional CPA texture sync items for when repacking
    /// </summary>
    private CPATextureSyncDataItem[]? CPATextureSyncItems { get; }

    #endregion

    #region Public Properties

    public GameInstallation? GameInstallation { get; }

    public Context Context { get; }

    /// <summary>
    /// The path separator character to use. This is usually \ or /.
    /// </summary>
    public char PathSeparatorCharacter => '\\';

    /// <summary>
    /// Indicates if directories can be created and deleted
    /// </summary>
    public bool CanModifyDirectories => true;

    /// <summary>
    /// The file extension for the archive file
    /// </summary>
    public FileExtension ArchiveFileExtension => new FileExtension(".cnt");

    /// <summary>
    /// The default archive file name to use when creating an archive
    /// </summary>
    public string DefaultArchiveFileName => "Textures.cnt";

    /// <summary>
    /// Gets the configuration UI to use for creator
    /// </summary>
    public object? GetCreatorUIConfig => null;

    /// <summary>
    /// The settings when serializing the data
    /// </summary>
    public OpenSpaceSettings Settings { get; }

    #endregion

    #region Public Methods

    public void EncodeFile(Stream inputStream, Stream outputStream, object fileEntry, FileMetadata fileMetadata)
    {
        // Get the file entry
        var file = (CNT_File)fileEntry;

        // Update the size
        file.FileSize = (uint)inputStream.Length;

        // Remove the encryption
        file.FileXORKey = new byte[4];
    }

    /// <summary>
    /// Decodes the file data from the input stream, or nothing if the data is not encoded
    /// </summary>
    /// <param name="inputStream">The input data stream to decode</param>
    /// <param name="outputStream">The output data stream for the decoded data</param>
    /// <param name="fileEntry">The file entry for the file to decode</param>
    public void DecodeFile(Stream inputStream, Stream outputStream, object fileEntry)
    {
        var entry = (CNT_File)fileEntry;

        if (entry.FileXORKey.Any(x => x != 0))
        {
            int maxXORLength = (int)(entry.FileSize - entry.FileSize % entry.FileXORKey.Length);

            // Decrypt the bytes
            new ProcessorEncoder(new XorArrayProcessor(entry.FileXORKey, maxLength: maxXORLength), inputStream.Length - inputStream.Position).
                DecodeStream(inputStream, outputStream);
        }
    }

    public FileMetadata GetFileMetadata(object fileEntry) => new();

    /// <summary>
    /// Gets the file data from the archive using a generator
    /// </summary>
    /// <param name="generator">The generator</param>
    /// <param name="fileEntry">The file entry</param>
    /// <returns>The encoded file data</returns>
    public Stream GetFileData(IDisposable generator, object fileEntry) => generator.CastTo<IFileGenerator<CNT_File>>().GetFileStream((CNT_File)fileEntry);

    public void WriteArchive(
        IDisposable? generator,
        object archive,
        ArchiveFileStream outputFileStream,
        IEnumerable<FileItem> files,
        ILoadState loadState)
    {
        Logger.Info("A CNT archive is being repacked...");

        // Get the archive data
        var data = (CNT)archive;

        // Create the file generator
        using FileGenerator<CNT_File> fileGenerator = new();

        // Get files and entries
        var archiveFiles = files.Select(x => new
        {
            Entry = (CNT_File)x.ArchiveEntry,
            FileItem = x
        }).ToArray();

        // Set files and directories
        data.Directories = archiveFiles.Select(x => x.FileItem.Directory).Distinct().Where(x => !x.IsNullOrWhiteSpace()).ToArray();
        data.Files = archiveFiles.Select(x => x.Entry).ToArray();

        // Set the directory indexes
        foreach (var file in archiveFiles)
            // Set the directory index
            file.Entry.DirectoryIndex = file.FileItem.Directory == String.Empty ? -1 : data.Directories.FindItemIndex(x => x == file.FileItem.Directory);

        loadState.CancellationToken.ThrowIfCancellationRequested();

        BinaryFile binaryFile = new StreamFile(Context, outputFileStream.Name, outputFileStream.Stream, mode: VirtualFileMode.DoNotClose);

        try
        {
            Context.AddFile(binaryFile);

            // Initialize the data
            data.Init(binaryFile.StartPointer);

            // Set the current pointer position to the header size
            data.RecalculateSize();
            uint pointer = (uint)data.SerializedSize;

            // Disable checksum
            data.IsChecksumUsed = false;

            // NOTE: We can't disable the XOR key entirely as that would disable it for the file bytes too, which would require them all to be decrypted
            // Reset XOR keys
            data.StringsXORKey = 0;

            // Load each file
            foreach (var file in archiveFiles)
            {
                // Get the file entry
                CNT_File entry = file.Entry;

                // Reset checksum and XOR key
                entry.FileChecksum = 0;
                entry.Pre_FileNameXORKey = 0;

                // Add to the generator
                fileGenerator.Add(entry, () =>
                {
                    // Get the file stream to write to the archive
                    Stream fileStream = file.FileItem.GetFileData(generator).Stream;

                    // Set the pointer
                    entry.FileOffset = pointer;

                    // Update the pointer by the file size
                    pointer += entry.FileSize;

                    return fileStream;
                });
            }

            // Make sure we have a generator for each file
            if (fileGenerator.Count != data.Files.Length)
                throw new Exception("The .cnt file can't be serialized without a file generator for each file");

            int fileIndex = 0;

            // Write the file contents
            foreach (CNT_File file in data.Files)
            {
                loadState.CancellationToken.ThrowIfCancellationRequested();

                // Get the file stream
                using Stream fileStream = fileGenerator.GetFileStream(file);

                // Set the position to the pointer
                outputFileStream.Stream.Position = file.FileOffset;

                // Write the contents from the generator
                fileStream.CopyToEx(outputFileStream.Stream);

                fileIndex++;

                // Update progress
                loadState.SetProgress(new Progress(fileIndex, data.Files.Length));
            }

            outputFileStream.Stream.Position = 0;

            // Serialize the data
            FileFactory.Write(Context, binaryFile.FilePath, data);

            Logger.Info("The CNT archive has been repacked");
        }
        finally
        {
            Context.RemoveFile(binaryFile);
        }
    }

    public double GetOnRepackedArchivesProgressLength() => 
        GameInstallation == null || CPATextureSyncItems == null || !Services.Data.Archive_CNT_SyncOnRepack ? 0 : 0.2;

    public async Task OnRepackedArchivesAsync(FileSystemPath[] archiveFilePaths, Action<Progress>? progressCallback = null)
    {
        // Make sure the texture sync can be performed
        if (GameInstallation == null || CPATextureSyncItems == null)
            return;

        AppUserData data = Services.Data;

        // Only auto-sync if set to do so
        if (!data.Archive_CNT_SyncOnRepack)
        {
            // Ask user the first time
            if (!data.Archive_CNT_SyncOnRepackRequested)
            {
                if (await Services.MessageUI.DisplayMessageAsync(Resources.Archive_AutoSyncCPATextures, MessageType.Question, true))
                    data.Archive_CNT_SyncOnRepack = true;

                data.Archive_CNT_SyncOnRepackRequested = true;
            }

            if (!data.Archive_CNT_SyncOnRepack)
                return;
        }

        data.Archive_CNT_SyncOnRepackRequested = true;

        CPATextureSyncManager textureSyncManager = new(GameInstallation, Settings, CPATextureSyncItems);

        await textureSyncManager.SyncTextureInfoAsync(archiveFilePaths, progressCallback);
    }

    /// <summary>
    /// Loads the archive data
    /// </summary>
    /// <param name="archive">The archive data</param>
    /// <param name="archiveFileStream">The archive file stream</param>
    /// <param name="fileName">The name of the file the archive is loaded from, if available</param>
    /// <returns>The archive data</returns>
    public ArchiveData LoadArchiveData(object archive, Stream archiveFileStream, string fileName)
    {
        // Get the data
        var data = (CNT)archive;

        Logger.Info("The directories are being retrieved for a CNT archive");

        // Helper method for getting the directories
        IEnumerable<ArchiveDirectory> GetDirectories()
        {
            // Add the directories to the collection
            for (var i = -1; i < data.Directories.Length; i++)
            {
                // Get the directory path
                var dir = i == -1 ? String.Empty : data.Directories[i];

                // Get the directory index
                var dirIndex = i;

                // Return each directory with the available files, including the root directory
                yield return new ArchiveDirectory(dir, data.Files.
                    Where(x => x.DirectoryIndex == dirIndex).
                    Select(f => new FileItem(this, f.FileName, dir, f)).
                    ToArray());
            }
        }

        // Return the data
        return new ArchiveData(GetDirectories(), new CNTFileGenerator(archiveFileStream));
    }

    public object LoadArchive(Stream archiveFileStream, string name)
    {
        // Set the stream position to 0
        archiveFileStream.Position = 0;

        // Load the current file
        CNT data = Context.ReadStreamData<CNT>(archiveFileStream, name: name, mode: VirtualFileMode.DoNotClose);

        Logger.Info("Read CNT file with {0} files and {1} directories", data.Files.Length, data.Directories.Length);

        return data;
    }

    /// <summary>
    /// Creates a new archive object
    /// </summary>
    /// <returns>The archive object</returns>
    public object CreateArchive()
    {
        // Create the .cnt data
        return new CNT
        {
            // Disable the XOR encryption
            IsXORUsed = false
        };
    }

    /// <summary>
    /// Gets info to display about the file
    /// </summary>
    /// <param name="archive">The archive</param>
    /// <param name="fileEntry">The file entry</param>
    /// <returns>The info items to display</returns>
    public IEnumerable<DuoGridItemViewModel> GetFileInfo(object archive, object fileEntry)
    {
        var entry = (CNT_File)fileEntry;
        var cnt = (CNT)archive;

        yield return new DuoGridItemViewModel(
            header: new ResourceLocString(nameof(Resources.Archive_FileInfo_Size)), 
            text: BinaryHelpers.BytesToString(entry.FileSize));

        if (cnt.Files.Contains(entry))
            yield return new DuoGridItemViewModel(
                header: new ResourceLocString(nameof(Resources.Archive_FileInfo_Pointer)), 
                text: $"0x{entry.FileOffset:X8}", 
                minUserLevel: UserLevel.Technical);
            
        yield return new DuoGridItemViewModel(
            header: new ResourceLocString(nameof(Resources.Archive_FileInfo_IsEncrypted)), 
            text: entry.FileXORKey.Any(x => x != 0).ToLocalizedString(),
            minUserLevel: UserLevel.Advanced);
    }

    /// <summary>
    /// Gets a new file entry object for the file
    /// </summary>
    /// <param name="archive">The archive</param>
    /// <param name="directory">The directory</param>
    /// <param name="fileName">The file name</param>
    /// <returns>The file entry object</returns>
    public object GetNewFileEntry(object archive, string directory, string fileName)
    {
        return new CNT_File()
        {
            FileName = fileName
        };
    }

    /// <summary>
    /// Gets the size of the file entry
    /// </summary>
    /// <param name="fileEntry">The file entry object</param>
    /// <param name="encoded">True if the size is for the encoded file, false if for the decoded file</param>
    /// <returns>The size, or null if it could not be determined</returns>
    public long? GetFileSize(object fileEntry, bool encoded)
    {
        var entry = (CNT_File)fileEntry;

        return entry.FileSize;
    }

    public void Dispose()
    {
        Context.Dispose();
    }

    #endregion

    #region Classes

    private class CNTFileGenerator : IFileGenerator<CNT_File>
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="archiveStream">The archive file stream</param>
        public CNTFileGenerator(Stream archiveStream)
        {
            // Get the stream
            Stream = archiveStream;
        }

        /// <summary>
        /// The stream
        /// </summary>
        private Stream Stream { get; }

        /// <summary>
        /// Gets the number of files which can be retrieved from the generator
        /// </summary>
        public int Count => throw new InvalidOperationException("The count can not be retrieved for this generator");

        /// <summary>
        /// Gets the file stream for the specified key
        /// </summary>
        /// <param name="fileEntry">The file entry to get the stream for</param>
        /// <returns>The stream</returns>
        public Stream GetFileStream(CNT_File fileEntry)
        {
            // Set the position
            Stream.Position = fileEntry.FileOffset;

            int size = (int)fileEntry.FileSize;

            // Create the buffer
            using ArrayRental<byte> buffer = new(size);

            // Read the bytes into the buffer
            Stream.Read(buffer.Array, 0, size);

            // Return the buffer
            MemoryStream ms = new();
            ms.Write(buffer.Array, 0, size);
            ms.Position = 0;
            return ms;
        }

        /// <summary>
        /// Disposes the generator
        /// </summary>
        public void Dispose()
        { }
    }

    #endregion
}