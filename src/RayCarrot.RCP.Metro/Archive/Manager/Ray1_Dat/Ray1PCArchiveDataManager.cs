using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BinarySerializer;
using BinarySerializer.Ray1;
using ByteSizeLib;
using NLog;

namespace RayCarrot.RCP.Metro;

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

    /// <summary>
    /// Encodes the file data from the input stream, or nothing if the data does not need to be encoded
    /// </summary>
    /// <param name="inputStream">The input data stream to encode</param>
    /// <param name="outputStream">The output data stream for the encoded data</param>
    /// <param name="fileEntry">The file entry for the file to encode</param>
    public void EncodeFile(Stream inputStream, Stream outputStream, object fileEntry)
    {
        // Get the file entry
        var file = (PC_FileArchiveEntry)fileEntry;

        // Update the size
        file.FileSize = (uint)inputStream.Length;

        // Remove the encryption
        file.XORKey = 0;

        // TODO-UPDATE: Do this when writing
        // Calculate the checksum
        Checksum8Calculator c = new();
        var buffer = new byte[inputStream.Length - inputStream.Position];
        inputStream.Read(buffer, 0, buffer.Length);
        c.AddBytes(buffer, 0, buffer.Length);
        file.Checksum = c.ChecksumValue;

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
        var entry = (PC_FileArchiveEntry)fileEntry;

        if (entry.XORKey != 0)
            // Decrypt the bytes
            new XOREncoder(new XOR8Calculator(entry.XORKey), inputStream.Length - inputStream.Position).
                DecodeStream(inputStream, outputStream);
    }

    /// <summary>
    /// Gets the file data from the archive using a generator
    /// </summary>
    /// <param name="generator">The generator</param>
    /// <param name="fileEntry">The file entry</param>
    /// <returns>The encoded file data</returns>
    public Stream GetFileData(IDisposable generator, object fileEntry) => generator.CastTo<IArchiveFileGenerator<PC_FileArchiveEntry>>().GetFileStream((PC_FileArchiveEntry)fileEntry);

    /// <summary>
    /// Writes the files to the archive
    /// </summary>
    /// <param name="generator">The generator</param>
    /// <param name="archive">The loaded archive data</param>
    /// <param name="outputFileStream">The file output stream for the archive</param>
    /// <param name="files">The files to include</param>
    public void WriteArchive(IDisposable? generator, object archive, Stream outputFileStream, IList<ArchiveFileItem> files)
    {
        Logger.Info("An R1 PC archive is being repacked...");

        // Get the archive data
        var data = (PC_FileArchive)archive;

        // Create the file generator
        using ArchiveFileGenerator<PC_FileArchiveEntry> fileGenerator = new();

        // Get files and entries
        var archiveFiles = files.Select(x => new
        {
            Entry = (PC_FileArchiveEntry)x.ArchiveEntry,
            FileItem = x
        }).ToArray();

        // Set files and directories
        data.Entries = archiveFiles.Select(x => x.Entry).ToArray();

        BinaryFile binaryFile = new StreamFile(Context, "Stream", outputFileStream, leaveOpen: true);

        try
        {
            Context.AddFile(binaryFile);

            // Initialize the data
            data.Init(binaryFile.StartPointer);

            // Set the current pointer position to the header size
            data.RecalculateSize();
            uint pointer = (uint)data.Size;

            // Load each file
            foreach (var file in archiveFiles)
            {
                // Process the file name
                file.Entry.FileName = ProcessFileName(file.Entry.FileName);

                // Get the file entry
                PC_FileArchiveEntry entry = file.Entry;

                // Add to the generator
                fileGenerator.Add(entry, () =>
                {
                    // Get the file stream to write to the archive
                    Stream fileStream = file.FileItem.GetFileData(generator).Stream;

                    // Set the pointer
                    entry.FileOffset = pointer;

                    // Update the pointer by the file size
                    pointer += entry.FileSize;

                    // Invoke event
                    OnWritingFileToArchive?.Invoke(this, new ValueEventArgs<ArchiveFileItem>(file.FileItem));

                    return fileStream;
                });
            }

            // Make sure we have a generator for each file
            if (fileGenerator.Count != data.Entries.Length)
                throw new Exception("The .dat file can't be serialized without a file generator for each file");

            // Write the file contents
            foreach (PC_FileArchiveEntry file in data.Entries)
            {
                // Get the file stream
                using Stream fileStream = fileGenerator.GetFileStream(file);

                // Set the position to the pointer
                outputFileStream.Position = file.FileOffset;

                // Write the contents from the generator
                fileStream.CopyTo(outputFileStream);
            }

            outputFileStream.Position = 0;

            // Serialize the data
            FileFactory.Write(binaryFile.FilePath, data, Context);

            Logger.Info("The R1 PC archive has been repacked");
        }
        finally
        {
            Context.RemoveFile(binaryFile);
        }
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
        var data = (PC_FileArchive)archive;

        Logger.Info("The files are being retrieved for an R1 PC archive");

        string fileExt = fileName.StartsWith("VIGNET", StringComparison.OrdinalIgnoreCase) || fileName.StartsWith("WldDesc", StringComparison.OrdinalIgnoreCase) ? ".pcx" : ".dat";

        // Return the data
        return new ArchiveData(new ArchiveDirectory[]
        {
            new ArchiveDirectory(String.Empty, data.Entries.Select(f => new ArchiveFileItem(this, $"{f.FileName}{fileExt}", String.Empty, f)).ToArray())
        }, new Rayman1PCArchiveGenerator(archiveFileStream));
    }

    /// <summary>
    /// Loads the archive from a stream
    /// </summary>
    /// <param name="archiveFileStream">The file stream for the archive</param>
    /// <returns>The archive data</returns>
    public object LoadArchive(Stream archiveFileStream)
    {
        // Set the stream position to 0
        archiveFileStream.Position = 0;

        // Load the current file
        PC_FileArchive data = Context.ReadStreamData<PC_FileArchive>(archiveFileStream, leaveOpen: true);

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
        PC_FileArchive archive = new();

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
        var entry = (PC_FileArchiveEntry)fileEntry;
        var archiveData = (PC_FileArchive)archive;

        yield return new DuoGridItemViewModel(Resources.Archive_FileInfo_Size, $"{ByteSize.FromBytes(entry.FileSize)}");

        if (archiveData.Entries.Contains(entry))
            yield return new DuoGridItemViewModel(Resources.Archive_FileInfo_Pointer, $"0x{entry.FileOffset:X8}", UserLevel.Technical);
            
        yield return new DuoGridItemViewModel(Resources.Archive_FileInfo_IsEncrypted, $"{entry.XORKey != 0}", UserLevel.Advanced);
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
        return new PC_FileArchiveEntry()
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
        var entry = (PC_FileArchiveEntry)fileEntry;

        return entry.FileSize;
    }

    public void Dispose()
    {
        Context.Dispose();
    }

    #endregion

    #region Events

    /// <summary>
    /// Occurs when a file is being written to an archive
    /// </summary>
    public event EventHandler<ValueEventArgs<ArchiveFileItem>>? OnWritingFileToArchive;

    #endregion

    #region Static Helpers

    public static FileSystemPath[] GetArchiveFiles(FileSystemPath gameDir)
    {
        string[] nonArchiveDatFiles = 
        {
            "ALLFIX.DAT",
            "BIGRAY.DAT",
            "CONCLU.DAT",
            "INTRO.DAT",
        };

        return Directory.GetFiles(gameDir, "*.dat", SearchOption.AllDirectories).
            Where(x => !nonArchiveDatFiles.Any(f => Path.GetFileName(x).Equals(f, StringComparison.OrdinalIgnoreCase))).
            Select(x => new FileSystemPath(x)).
            ToArray();
    }

    #endregion

    #region Classes

    /// <summary>
    /// The archive file generator for .cnt files
    /// </summary>
    private class Rayman1PCArchiveGenerator : IArchiveFileGenerator<PC_FileArchiveEntry>
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="archiveStream">The archive file stream</param>
        public Rayman1PCArchiveGenerator(Stream archiveStream)
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
        public Stream GetFileStream(PC_FileArchiveEntry fileEntry)
        {
            // Set the position
            Stream.Position = fileEntry.FileOffset;

            // Create the buffer
            byte[] buffer = new byte[fileEntry.FileSize];

            // Read the bytes into the buffer
            Stream.Read(buffer, 0, buffer.Length);

            // Return the buffer
            return new MemoryStream(buffer);
        }

        /// <summary>
        /// Disposes the generator
        /// </summary>
        public void Dispose()
        { }
    }

    #endregion
}