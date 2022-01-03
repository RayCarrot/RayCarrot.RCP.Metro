using RayCarrot.Binary;
using RayCarrot.IO;
using NLog;
using RayCarrot.Rayman;
using RayCarrot.Rayman.UbiArt;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ByteSizeLib;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// Archive data manager for a UbiArt .ipk file
/// </summary>
public class UbiArtIPKArchiveDataManager : IArchiveDataManager
{
    #region Constructor

    /// <summary>
    /// Default constructor
    /// </summary>
    /// <param name="configViewModel">The configuration view model</param>
    public UbiArtIPKArchiveDataManager(UbiArtIPKArchiveConfigViewModel configViewModel)
    {
        Config = configViewModel;
    }

    #endregion

    #region Logger

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    #endregion

    #region Public Properties

    /// <summary>
    /// Indicates if directories can be created and deleted
    /// </summary>
    public bool CanModifyDirectories => true;

    /// <summary>
    /// The path separator character to use. This is usually \ or /.
    /// </summary>
    public char PathSeparatorCharacter => '/';

    /// <summary>
    /// The file extension for the archive file
    /// </summary>
    public FileExtension ArchiveFileExtension => new FileExtension(".ipk");

    /// <summary>
    /// The serializer settings to use for the archive
    /// </summary>
    public BinarySerializerSettings SerializerSettings => Settings;

    public object ContextSettings => throw new NotImplementedException();

    /// <summary>
    /// The default archive file name to use when creating an archive
    /// </summary>
    public string DefaultArchiveFileName => "patch_PC.ipk";

    /// <summary>
    /// Gets the configuration UI to use for creator
    /// </summary>
    public object GetCreatorUIConfig => new UbiArtIPKArchiveConfigUI()
    {
        DataContext = Config
    };

    /// <summary>
    /// The settings when serializing the data
    /// </summary>
    protected UbiArtSettings Settings => Config.Settings;

    /// <summary>
    /// The configuration view model
    /// </summary>
    protected UbiArtIPKArchiveConfigViewModel Config { get; }

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
        var entry = (UbiArtIPKFileEntry)fileEntry;

        // Set the file size
        entry.Size = (uint)inputStream.Length;

        // Return the data as is if the file should not be compressed
        if (!Config.ShouldCompress(entry))
            return;

        // Compress the bytes
        UbiArtIpkData.GetEncoder(entry.IPKVersion, entry.Size).Encode(inputStream, outputStream);

        Logger.Trace("The file {0} has been compressed", entry.Path.FileName);

        // Set the compressed file size
        entry.CompressedSize = (uint)outputStream.Length;
    }

    /// <summary>
    /// Decodes the file data from the input stream, or nothing if the data is not encoded
    /// </summary>
    /// <param name="inputStream">The input data stream to decode</param>
    /// <param name="outputStream">The output data stream for the decoded data</param>
    /// <param name="fileEntry">The file entry for the file to decode</param>
    public void DecodeFile(Stream inputStream, Stream outputStream, object fileEntry)
    {
        var entry = (UbiArtIPKFileEntry)fileEntry;

        // Decompress the data if compressed
        if (entry.IsCompressed)
            UbiArtIpkData.GetEncoder(entry.IPKVersion, entry.Size).Decode(inputStream, outputStream);
    }

    /// <summary>
    /// Gets the file data from the archive using a generator
    /// </summary>
    /// <param name="generator">The generator</param>
    /// <param name="fileEntry">The file entry</param>
    /// <returns>The encoded file data</returns>
    public Stream GetFileData(IDisposable generator, object fileEntry) => generator.CastTo<IArchiveFileGenerator<UbiArtIPKFileEntry>>().GetFileStream((UbiArtIPKFileEntry)fileEntry);

    /// <summary>
    /// Writes the files to the archive
    /// </summary>
    /// <param name="generator">The generator</param>
    /// <param name="archive">The loaded archive data</param>
    /// <param name="outputFileStream">The file output stream for the archive</param>
    /// <param name="files">The files to include</param>
    public void WriteArchive(IDisposable? generator, object archive, Stream outputFileStream, IList<ArchiveFileItem> files)
    {
        Logger.Info("An IPK archive is being repacked...");

        // Get the archive data
        var data = (UbiArtIpkData)archive;

        // Create the file generator
        using ArchiveFileGenerator<UbiArtIPKFileEntry> fileGenerator = new();

        // Get files and entries
        var archiveFiles = files.Select(x => new
        {
            Entry = (UbiArtIPKFileEntry)x.ArchiveEntry,
            FileItem = x
        }).ToArray();

        // Set the files
        data.Files = archiveFiles.Select(x => x.Entry).ToArray();
        data.FilesCount = (uint)data.Files.Length;

        // Save the old base offset
        uint oldBaseOffset = data.BaseOffset;

        // Keep track of the current pointer position
        ulong currentOffset = 0;

        // Handle each file
        foreach (var file in archiveFiles)
        {
            // Get the file
            UbiArtIPKFileEntry entry = file.Entry;

            // Reset the offset array to always contain 1 item
            entry.Offsets = new ulong[]
            {
                entry.Offsets?.FirstOrDefault() ?? 0
            };

            // Set the count
            entry.OffsetCount = (uint)entry.Offsets.Length;

            // Add to the generator
            fileGenerator.Add(entry, () =>
            {
                // When reading the original file we need to use the old base offset
                uint newBaseOffset = data.BaseOffset;
                data.BaseOffset = oldBaseOffset;

                // Get the file bytes to write to the archive
                Stream fileStream = file.FileItem.GetFileData(generator).Stream;

                data.BaseOffset = newBaseOffset;

                // Set the offset
                entry.Offsets[0] = currentOffset;

                // Increase by the file size
                currentOffset += entry.ArchiveSize;

                // Invoke event
                OnWritingFileToArchive?.Invoke(this, new ValueEventArgs<ArchiveFileItem>(file.FileItem));

                return fileStream;
            });
        }

        // Set the base offset
        data.BaseOffset = data.GetHeaderSize(Settings);

        // Write the files
        data.WriteArchiveContent(outputFileStream, fileGenerator, Config.ShouldCompress(data));

        outputFileStream.Position = 0;

        // Serialize the data
        BinarySerializableHelpers.WriteToStream(data, outputFileStream, Settings, Services.App.GetBinarySerializerLogger());

        Logger.Info("The IPK archive has been repacked");
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
        var data = (UbiArtIpkData)archive;

        Logger.Info("The directories are being retrieved for an IPK archive");

        // Helper method for getting the directories
        IEnumerable<ArchiveDirectory> GetDirectories()
        {
            // Add the directories to the collection
            foreach (var file in data.Files.GroupBy(x => x.Path.DirectoryPath))
            {
                // Return each directory with the available files, including the root directory
                yield return new ArchiveDirectory(file.Key, file.Select(x => new ArchiveFileItem(this, x.Path.FileName, x.Path.DirectoryPath, x)).ToArray());
            }
        }

        // Return the data
        return new ArchiveData(GetDirectories(), data.GetArchiveContent(archiveFileStream));
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
        UbiArtIpkData data = BinarySerializableHelpers.ReadFromStream<UbiArtIpkData>(archiveFileStream, Settings, Services.App.GetBinarySerializerLogger());

        Logger.Info("Read IPK file ({0}) with {1} files", data.Version, data.FilesCount);

        return data;
    }

    /// <summary>
    /// Creates a new archive object
    /// </summary>
    /// <returns>The archive object</returns>
    public object CreateArchive()
    {
        // Create the data
        UbiArtIpkData data = new();

        // Configure the data
        Config.ConfigureIpkData(data);

        return data;
    }

    /// <summary>
    /// Gets info to display about the file
    /// </summary>
    /// <param name="archive">The archive</param>
    /// <param name="fileEntry">The file entry</param>
    /// <returns>The info items to display</returns>
    public IEnumerable<DuoGridItemViewModel> GetFileInfo(object archive, object fileEntry)
    {
        var entry = (UbiArtIPKFileEntry)fileEntry;
        var ipk = (UbiArtIpkData)archive;

        yield return new DuoGridItemViewModel(Resources.Archive_FileInfo_Size, $"{ByteSize.FromBytes(entry.Size)}");

        if (entry.IsCompressed)
            yield return new DuoGridItemViewModel(Resources.Archive_FileInfo_SizeComp, $"{ByteSize.FromBytes(entry.CompressedSize)}");

        if (ipk.Files.Contains(entry))
            yield return new DuoGridItemViewModel(Resources.Archive_FileInfo_Pointer, $"0x{entry.Offsets.First() + ipk.BaseOffset:X16}", UserLevel.Technical);
            
        yield return new DuoGridItemViewModel(Resources.Archive_FileInfo_IsComp, $"{entry.IsCompressed}");
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
        return new UbiArtIPKFileEntry
        {
            Path = new UbiArtPath(this.CombinePaths(directory, fileName)),
            IPKVersion = ((UbiArtIpkData)archive).Version
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
        var entry = (UbiArtIPKFileEntry)fileEntry;

        return encoded ? entry.ArchiveSize : entry.Size;
    }

    #endregion

    #region Events

    /// <summary>
    /// Occurs when a file is being written to an archive
    /// </summary>
    public event EventHandler<ValueEventArgs<ArchiveFileItem>>? OnWritingFileToArchive;

    #endregion
}