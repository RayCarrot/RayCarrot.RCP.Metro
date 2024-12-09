using System.IO;
using BinarySerializer;
using BinarySerializer.Ray1;
using BinarySerializer.Ray1.PC;

namespace RayCarrot.RCP.Metro.Archive.Ray1;

/// <summary>
/// Archive data manager for Rayman 1 PC spin-off .dat files
/// </summary>
public class Ray1PCArchiveDataManager : IArchiveDataManager
{
    #region Constructor

    /// <summary>
    /// Default constructor
    /// </summary>
    /// <param name="settings">The game settings</param>
    public Ray1PCArchiveDataManager(Ray1Settings settings)
    {
        Settings = settings;

        Context = new RCPContext(String.Empty, new RCPSerializerSettings()
        {
            DefaultEndianness = Endian.Little
        });
        Context.AddSettings(settings);

        Config = new Ray1PCArchiveConfigViewModel(settings);
    }

    #endregion

    #region Logger

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    #endregion

    #region Public Properties

    public Context Context { get; }

    /// <summary>
    /// The path separator character to use. This is usually \ or /.
    /// </summary>
    public char PathSeparatorCharacter => '\\';

    /// <summary>
    /// Indicates if directories can be created and deleted
    /// </summary>
    public bool CanModifyDirectories => false;

    /// <summary>
    /// The file extension for the archive file
    /// </summary>
    public FileExtension ArchiveFileExtension => new FileExtension(".dat");

    /// <summary>
    /// The default archive file name to use when creating an archive
    /// </summary>
    public string DefaultArchiveFileName => "Archive.dat";

    /// <summary>
    /// Gets the configuration UI to use for creator
    /// </summary>
    public object GetCreatorUIConfig => new Ray1PCArchiveConfigUI()
    {
        DataContext = Config
    };

    /// <summary>
    /// The configuration view model
    /// </summary>
    protected Ray1PCArchiveConfigViewModel Config { get; }

    /// <summary>
    /// The settings when serializing the data
    /// </summary>
    public Ray1Settings Settings { get; }

    #endregion

    #region Public Methods

    public void EncodeFile(Stream inputStream, Stream outputStream, object fileEntry, FileMetadata fileMetadata)
    {
        // Get the file entry
        var file = (FileArchiveEntry)fileEntry;

        // Update the size
        file.FileSize = (uint)inputStream.Length;

        // Remove the encryption
        file.XORKey = 0;

        // TODO: Calculate the checksum when writing instead as we're accessing the bytes then
        // Calculate the checksum
        Checksum8Processor p = new();
        int size = (int)(inputStream.Length - inputStream.Position);
        using ArrayRental<byte> buffer = new(size);
        inputStream.Read(buffer.Array, 0, size);
        p.ProcessBytes(buffer.Array, 0, size);
        file.Checksum = (byte)p.CalculatedValue;

        inputStream.Position = 0;
    }

    /// <summary>
    /// Decodes the file data from the input stream, or nothing if the data is not encoded
    /// </summary>
    /// <param name="inputStream">The input data stream to decode</param>
    /// <param name="outputStream">The output data stream for the decoded data</param>
    /// <param name="fileEntry">The file entry for the file to decode</param>
    public void DecodeFile(Stream inputStream, Stream outputStream, object fileEntry)
    {
        var entry = (FileArchiveEntry)fileEntry;

        if (entry.XORKey != 0)
            // Decrypt the bytes
            new ProcessorEncoder(new Xor8Processor(entry.XORKey), inputStream.Length - inputStream.Position).
                DecodeStream(inputStream, outputStream);
    }

    public FileMetadata GetFileMetadata(object fileEntry) => new();

    /// <summary>
    /// Gets the file data from the archive using a generator
    /// </summary>
    /// <param name="generator">The generator</param>
    /// <param name="fileEntry">The file entry</param>
    /// <returns>The encoded file data</returns>
    public Stream GetFileData(IDisposable generator, object fileEntry) => generator.CastTo<IFileGenerator<FileArchiveEntry>>().GetFileStream((FileArchiveEntry)fileEntry);

    public ArchiveRepackResult WriteArchive(
        IDisposable? generator,
        object archive,
        ArchiveFileStream outputFileStream,
        IEnumerable<FileItem> files,
        ILoadState loadState)
    {
        Logger.Info("An R1 PC archive is being repacked...");

        // Get the archive data
        var data = (FileArchive)archive;

        // Create the file generator
        using FileGenerator<FileArchiveEntry> fileGenerator = new();

        // Get files and entries
        var archiveFiles = files.Select(x => new
        {
            Entry = (FileArchiveEntry)x.ArchiveEntry,
            FileItem = x
        }).ToArray();

        // Set files and directories
        data.Entries = archiveFiles.Select(x => x.Entry).ToArray();

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

            // Load each file
            foreach (var file in archiveFiles)
            {
                // Process the file name
                file.Entry.FileName = ProcessFileName(file.Entry.FileName);

                // Get the file entry
                FileArchiveEntry entry = file.Entry;

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
            if (fileGenerator.Count != data.Entries.Length)
                throw new Exception("The .dat file can't be serialized without a file generator for each file");

            int fileIndex = 0;

            // Write the file contents
            foreach (FileArchiveEntry file in data.Entries)
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
                loadState.SetProgress(new Progress(fileIndex, data.Entries.Length));
            }

            outputFileStream.Stream.Position = 0;

            // Serialize the data
            FileFactory.Write(Context, binaryFile.FilePath, data);

            Logger.Info("The R1 PC archive has been repacked");
        }
        finally
        {
            Context.RemoveFile(binaryFile);
        }

        return new ArchiveRepackResult();
    }

    public double GetOnRepackedArchivesProgressLength() => 0;

    public Task OnRepackedArchivesAsync(
        FileSystemPath[] archiveFilePaths,
        IReadOnlyList<ArchiveRepackResult> repackResults,
        ILoadState loadState) => Task.CompletedTask;

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
        var data = (FileArchive)archive;

        Logger.Info("The files are being retrieved for an R1 PC archive");

        string fileExt = fileName.StartsWith("VIGNET", StringComparison.OrdinalIgnoreCase) ? ".pcx" : ".dat";

        // Return the data
        return new ArchiveData(new ArchiveDirectory[]
        {
            new ArchiveDirectory(String.Empty, data.Entries.Select(f => new FileItem(this, $"{f.FileName}{fileExt}", String.Empty, f)).ToArray())
        }, new Rayman1PCGenerator(archiveFileStream));
    }

    public object LoadArchive(Stream archiveFileStream, string name)
    {
        // Set the stream position to 0
        archiveFileStream.Position = 0;

        // Load the current file
        FileArchive data = Context.ReadStreamData<FileArchive>(archiveFileStream, name: name, mode: VirtualFileMode.DoNotClose);

        Logger.Info("Read R1 PC archive file with {0} files", data.Entries.Length);

        return data;
    }

    /// <summary>
    /// Creates a new archive object
    /// </summary>
    /// <returns>The archive object</returns>
    public object CreateArchive()
    {
        // Create the archive data
        FileArchive archive = new();

        Config.ConfigureArchiveData(archive);

        return archive;
    }

    /// <summary>
    /// Gets info to display about the file
    /// </summary>
    /// <param name="archive">The archive</param>
    /// <param name="fileEntry">The file entry</param>
    /// <returns>The info items to display</returns>
    public IEnumerable<DuoGridItemViewModel> GetFileInfo(object archive, object fileEntry)
    {
        var entry = (FileArchiveEntry)fileEntry;
        var archiveData = (FileArchive)archive;

        yield return new DuoGridItemViewModel(
            header: new ResourceLocString(nameof(Resources.Archive_FileInfo_Size)), 
            text: BinaryHelpers.BytesToString(entry.FileSize));

        if (archiveData.Entries.Contains(entry))
            yield return new DuoGridItemViewModel(
                header: new ResourceLocString(nameof(Resources.Archive_FileInfo_Pointer)), 
                text: $"0x{entry.FileOffset:X8}", 
                minUserLevel: UserLevel.Technical);
            
        yield return new DuoGridItemViewModel(
            header: new ResourceLocString(nameof(Resources.Archive_FileInfo_IsEncrypted)), 
            text: (entry.XORKey != 0).ToLocalizedString(),
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
        return new FileArchiveEntry()
        {
            FileName = ProcessFileName(fileName)
        };
    }

    /// <summary>
    /// Updates the file name by removing added file extensions and truncates the length
    /// </summary>
    /// <param name="fileName">The file name</param>
    /// <returns>The processed file name</returns>
    public string ProcessFileName(string fileName) => fileName.Replace(".dat", String.Empty).Replace(".pcx", String.Empty).Truncate(9);

    /// <summary>
    /// Gets the size of the file entry
    /// </summary>
    /// <param name="fileEntry">The file entry object</param>
    /// <param name="encoded">True if the size is for the encoded file, false if for the decoded file</param>
    /// <returns>The size, or null if it could not be determined</returns>
    public long? GetFileSize(object fileEntry, bool encoded)
    {
        var entry = (FileArchiveEntry)fileEntry;

        return entry.FileSize;
    }

    public void Dispose()
    {
        Context.Dispose();
    }

    #endregion

    #region Classes

    /// <summary>
    /// The archive file generator for .cnt files
    /// </summary>
    private class Rayman1PCGenerator : IFileGenerator<FileArchiveEntry>
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="archiveStream">The archive file stream</param>
        public Rayman1PCGenerator(Stream archiveStream)
        {
            // Get the stream
            Stream = archiveStream;
        }

        /// <summary>
        /// The stream
        /// </summary>
        protected Stream Stream { get; }

        /// <summary>
        /// Gets the number of files which can be retrieved from the generator
        /// </summary>
        public int Count => throw new InvalidOperationException("The count can not be retrieved for this generator");

        /// <summary>
        /// Gets the file stream for the specified key
        /// </summary>
        /// <param name="fileEntry">The file entry to get the stream for</param>
        /// <returns>The stream</returns>
        public Stream GetFileStream(FileArchiveEntry fileEntry)
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