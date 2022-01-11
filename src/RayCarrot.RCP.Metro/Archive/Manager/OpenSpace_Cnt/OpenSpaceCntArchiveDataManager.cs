using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BinarySerializer;
using BinarySerializer.OpenSpace;
using ByteSizeLib;
using NLog;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// Archive data manager for an OpenSpace .cnt file
/// </summary>
public class OpenSpaceCntArchiveDataManager : IArchiveDataManager
{
    #region Constructor

    /// <summary>
    /// Default constructor
    /// </summary>
    /// <param name="settings">The settings when serializing the data</param>
    public OpenSpaceCntArchiveDataManager(OpenSpaceSettings settings)
    {
        Settings = settings;

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

    #region Public Properties

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

    /// <summary>
    /// Encodes the file data from the input stream, or nothing if the data does not need to be encoded
    /// </summary>
    /// <param name="inputStream">The input data stream to encode</param>
    /// <param name="outputStream">The output data stream for the encoded data</param>
    /// <param name="fileEntry">The file entry for the file to encode</param>
    public void EncodeFile(Stream inputStream, Stream outputStream, object fileEntry)
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
            new XOREncoder(new XORArrayCalculator(entry.FileXORKey, maxLength: maxXORLength), inputStream.Length - inputStream.Position).
                DecodeStream(inputStream, outputStream);
        }
    }

    /// <summary>
    /// Gets the file data from the archive using a generator
    /// </summary>
    /// <param name="generator">The generator</param>
    /// <param name="fileEntry">The file entry</param>
    /// <returns>The encoded file data</returns>
    public Stream GetFileData(IDisposable generator, object fileEntry) => generator.CastTo<IArchiveFileGenerator<CNT_File>>().GetFileStream((CNT_File)fileEntry);

    /// <summary>
    /// Writes the files to the archive
    /// </summary>
    /// <param name="generator">The generator</param>
    /// <param name="archive">The loaded archive data</param>
    /// <param name="outputFileStream">The file output stream for the archive</param>
    /// <param name="files">The files to include</param>
    public void WriteArchive(IDisposable? generator, object archive, Stream outputFileStream, IList<ArchiveFileItem> files)
    {
        Logger.Info("A CNT archive is being repacked...");

        // Get the archive data
        var data = (CNT)archive;

        // Create the file generator
        using ArchiveFileGenerator<CNT_File> fileGenerator = new();

        // Get files and entries
        var archiveFiles = files.Select(x => new
        {
            Entry = (CNT_File)x.ArchiveEntry,
            FileItem = x
        }).ToArray();

        // Set files and directories
        data.Directories = files.Select(x => x.Directory).Distinct().Where(x => !x.IsNullOrWhiteSpace()).ToArray();
        data.Files = archiveFiles.Select(x => x.Entry).ToArray();

        // Set the directory indexes
        foreach (var file in archiveFiles)
            // Set the directory index
            file.Entry.DirectoryIndex = file.FileItem.Directory == String.Empty ? -1 : data.Directories.FindItemIndex(x => x == file.FileItem.Directory);

        BinaryFile binaryFile = new StreamFile(Context, "Stream", outputFileStream, leaveOpen: true);

        try
        {
            Context.AddFile(binaryFile);

            // Initialize the data
            data.Init(binaryFile.StartPointer);

            // Set the current pointer position to the header size
            data.RecalculateSize();
            uint pointer = (uint)data.Size;

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

                    // Invoke event
                    OnWritingFileToArchive?.Invoke(this, new ValueEventArgs<ArchiveFileItem>(file.FileItem));

                    return fileStream;
                });
            }

            // Make sure we have a generator for each file
            if (fileGenerator.Count != data.Files.Length)
                throw new Exception("The .cnt file can't be serialized without a file generator for each file");

            // Write the file contents
            foreach (CNT_File file in data.Files)
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
            FileFactory.Write(Context, binaryFile.FilePath, data);

            Logger.Info("The CNT archive has been repacked");
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
                    Select(f => new ArchiveFileItem(this, f.FileName, dir, f)).
                    ToArray());
            }
        }

        // Return the data
        return new ArchiveData(GetDirectories(), new CNTFileGenerator(archiveFileStream));
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
        CNT data = Context.ReadStreamData<CNT>(archiveFileStream, leaveOpen: true);

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
            text: $"{ByteSize.FromBytes(entry.FileSize)}");

        if (cnt.Files.Contains(entry))
            yield return new DuoGridItemViewModel(
                header: new ResourceLocString(nameof(Resources.Archive_FileInfo_Pointer)), 
                text: $"0x{entry.FileOffset:X8}", 
                minUserLevel: UserLevel.Technical);
            
        yield return new DuoGridItemViewModel(
            header: new ResourceLocString(nameof(Resources.Archive_FileInfo_IsEncrypted)), 
            text: new GeneratedLocString(() => $"{entry.FileXORKey.Any(x => x != 0)}"), 
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

    #region Events

    /// <summary>
    /// Occurs when a file is being written to an archive
    /// </summary>
    public event EventHandler<ValueEventArgs<ArchiveFileItem>>? OnWritingFileToArchive;

    #endregion

    #region Classes

    private class CNTFileGenerator : IArchiveFileGenerator<CNT_File>
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