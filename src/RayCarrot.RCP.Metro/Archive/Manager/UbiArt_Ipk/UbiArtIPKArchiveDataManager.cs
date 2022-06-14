using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BinarySerializer;
using BinarySerializer.UbiArt;
using ByteSizeLib;

namespace RayCarrot.RCP.Metro.Archive.UbiArt;

/// <summary>
/// Archive data manager for a UbiArt .ipk file
/// </summary>
public class UbiArtIPKArchiveDataManager : IArchiveDataManager
{
    #region Constructor

    /// <summary>
    /// Default constructor
    /// </summary>
    /// <param name="settings">The settings when serializing the data</param>
    /// <param name="compressionMode">The file compression mode</param>
    public UbiArtIPKArchiveDataManager(UbiArtSettings settings, UbiArtIPKArchiveConfigViewModel.FileCompressionMode compressionMode)
    {
        Context = new RCPContext(String.Empty, new RCPSerializerSettings()
        {
            DefaultEndianness = settings.GetEndian
        });
        Context.AddSettings(settings);

        Config = new UbiArtIPKArchiveConfigViewModel(settings, compressionMode);
    }

    #endregion

    #region Logger

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    #endregion

    #region Public Properties

    public Context Context { get; }

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
    /// The configuration view model
    /// </summary>
    protected UbiArtIPKArchiveConfigViewModel Config { get; }

    /// <summary>
    /// The settings when serializing the data
    /// </summary>
    protected UbiArtSettings Settings => Config.Settings;

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
        var entry = (BundleFile_FileEntry)fileEntry;

        // Set the file size
        entry.FileSize = (uint)inputStream.Length;

        // Return the data as is if the file should not be compressed
        if (!Config.ShouldCompress(entry))
            return;

        // Compress the bytes
        BundleBootHeader.GetEncoder(entry.Pre_BundleVersion, entry.FileSize).EncodeStream(inputStream, outputStream);

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
        var entry = (BundleFile_FileEntry)fileEntry;

        // Decompress the data if compressed
        if (entry.IsCompressed)
            BundleBootHeader.GetEncoder(entry.Pre_BundleVersion, entry.FileSize).DecodeStream(inputStream, outputStream);
    }

    /// <summary>
    /// Gets the file data from the archive using a generator
    /// </summary>
    /// <param name="generator">The generator</param>
    /// <param name="fileEntry">The file entry</param>
    /// <returns>The encoded file data</returns>
    public Stream GetFileData(IDisposable generator, object fileEntry) => generator.CastTo<IFileGenerator<BundleFile_FileEntry>>().GetFileStream((BundleFile_FileEntry)fileEntry);

    /// <summary>
    /// Writes the files to the archive
    /// </summary>
    /// <param name="generator">The generator</param>
    /// <param name="archive">The loaded archive data</param>
    /// <param name="outputFileStream">The file output stream for the archive</param>
    /// <param name="files">The files to include</param>
    /// <param name="progressCallback">A progress callback action</param>
    public void WriteArchive(IDisposable? generator, object archive, ArchiveFileStream outputFileStream, IList<FileItem> files, Action<Progress> progressCallback)
    {
        Logger.Info("An IPK archive is being repacked...");

        // Get the archive data
        var data = (BundleFile)archive;

        // Create the file generator
        using FileGenerator<BundleFile_FileEntry> fileGenerator = new();

        // Get files and entries
        var archiveFiles = files.Select(x => new
        {
            Entry = (BundleFile_FileEntry)x.ArchiveEntry,
            FileItem = x
        }).ToArray();

        // Set the files
        data.FilePack.Files = archiveFiles.Select(x => x.Entry).ToArray();
        data.BootHeader.FilesCount = (uint)data.FilePack.Files.Length;

        // Save the old base offset
        uint oldBaseOffset = data.BootHeader.BaseOffset;

        // Keep track of the current pointer position
        ulong currentOffset = 0;

        // Handle each file
        foreach (var file in archiveFiles)
        {
            // Get the file
            BundleFile_FileEntry entry = file.Entry;

            // Reset the offset array to always contain 1 item
            entry.Offsets = new ulong[]
            {
                entry.Offsets?.FirstOrDefault() ?? 0
            };

            // Set the count
            entry.OffsetsCount = (uint)entry.Offsets.Length;

            // Add to the generator
            fileGenerator.Add(entry, () =>
            {
                // When reading the original file we need to use the old base offset
                uint newBaseOffset = data.BootHeader.BaseOffset;
                data.BootHeader.BaseOffset = oldBaseOffset;

                // Get the file bytes to write to the archive
                Stream fileStream = file.FileItem.GetFileData(generator).Stream;

                data.BootHeader.BaseOffset = newBaseOffset;

                // Set the offset
                entry.Offsets[0] = currentOffset;

                // Increase by the file size
                currentOffset += entry.ArchiveSize;

                return fileStream;
            });
        }

        BinaryFile binaryFile = new StreamFile(Context, outputFileStream.Name, outputFileStream.Stream, leaveOpen: true);

        try
        {
            Context.AddFile(binaryFile);

            // Initialize the data
            data.Init(binaryFile.StartPointer);

            // Set the base offset
            data.RecalculateSize();
            data.BootHeader.BaseOffset = (uint)data.Size;

            // Write the files
            WriteArchiveContent(data, outputFileStream.Stream, fileGenerator, Config.ShouldCompress(data.BootHeader), progressCallback);

            outputFileStream.Stream.Position = 0;

            // Serialize the data
            FileFactory.Write(Context, binaryFile.FilePath, data);

            Logger.Info("The IPK archive has been repacked");
        }
        finally
        {
            Context.RemoveFile(binaryFile);
        }
    }

    private void WriteArchiveContent(BundleFile bundle, Stream stream, IFileGenerator<BundleFile_FileEntry> fileGenerator, bool compressBlock, Action<Progress> progressCallback)
    {
        // Make sure we have a generator for each file
        if (fileGenerator.Count != bundle.FilePack.Files.Length)
            throw new Exception("The .ipk file can't be serialized without a file generator for each file");

        TempFile? tempDecompressedBlockFile = null;
        FileStream? tempDecompressedBlockFileStream = null;

        try
        {
            // Create a temporary file to use if the block should be compressed
            if (compressBlock)
            {
                tempDecompressedBlockFile = new TempFile(true);
                tempDecompressedBlockFileStream = new FileStream(tempDecompressedBlockFile.TempPath, FileMode.Open);
            }

            // Get the stream to write the files to
            Stream currentStream = compressBlock ? tempDecompressedBlockFileStream! : stream;

            int fileIndex = 0;

            // Write the file contents
            foreach (BundleFile_FileEntry file in bundle.FilePack.Files)
            {
                // Get the file stream from the generator
                using Stream fileStream = fileGenerator.GetFileStream(file);

                // Make sure the size matches
                if (fileStream.Length != file.ArchiveSize)
                    throw new Exception("The archived file size does not match the bytes retrieved from the generator");

                // Handle every file offset
                foreach (ulong offset in file.Offsets)
                {
                    // Set the position
                    currentStream.Position = (long)(compressBlock ? offset : (offset + bundle.BootHeader.BaseOffset));

                    // Write the bytes
                    fileStream.CopyTo(currentStream);
                    fileStream.Position = 0;
                }

                fileIndex++;

                // Update progress
                progressCallback(new Progress(fileIndex, bundle.FilePack.Files.Length));
            }

            // Handle the data if it should be compressed
            if (compressBlock)
            {
                // Get the length
                long decompressedSize = tempDecompressedBlockFileStream!.Length;

                // Create a temporary file for the final compressed data
                using TempFile tempCompressedBlockFile = new(true);
                using FileStream tempCompressedBlockFileStream = new(tempCompressedBlockFile.TempPath, FileMode.Open);

                tempDecompressedBlockFileStream.Position = 0;

                // Compress the data
                BundleBootHeader.GetEncoder(bundle.BootHeader.Version, -1).EncodeStream(tempDecompressedBlockFileStream, tempCompressedBlockFileStream);

                tempCompressedBlockFileStream.Position = 0;

                // Set the .ipk stream position
                stream.Position = bundle.BootHeader.BaseOffset;

                // Write the data to main stream
                tempCompressedBlockFileStream.CopyTo(stream);

                // Update the size
                bundle.BootHeader.BlockCompressedSize = (uint)tempCompressedBlockFileStream.Length;
                bundle.BootHeader.BlockSize = (uint)decompressedSize;
            }
            else
            {
                // Reset the size
                bundle.BootHeader.BlockCompressedSize = 0;
                bundle.BootHeader.BlockSize = 0;
            }
        }
        finally
        {
            tempDecompressedBlockFile?.Dispose();
            tempDecompressedBlockFileStream?.Dispose();
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
        var data = (BundleFile)archive;

        Logger.Info("The directories are being retrieved for an IPK archive");

        // Helper method for getting the directories
        IEnumerable<ArchiveDirectory> GetDirectories()
        {
            // Add the directories to the collection
            foreach (var file in data.FilePack.Files.GroupBy(x => x.Path.DirectoryPath))
            {
                // Return each directory with the available files, including the root directory
                yield return new ArchiveDirectory(file.Key, file.Select(x => new FileItem(this, x.Path.FileName, x.Path.DirectoryPath, x)).ToArray());
            }
        }

        // Return the data
        return new ArchiveData(GetDirectories(), new IPKFileGenerator(data, archiveFileStream));
    }

    public object LoadArchive(Stream archiveFileStream, string name)
    {
        // Set the stream position to 0
        archiveFileStream.Position = 0;

        // Load the current file
        BundleFile data = Context.ReadStreamData<BundleFile>(archiveFileStream, name: name, leaveOpen: true);

        Logger.Info("Read IPK file ({0}) with {1} files", data.BootHeader.Version, data.FilePack.Files.Length);

        return data;
    }

    /// <summary>
    /// Creates a new archive object
    /// </summary>
    /// <returns>The archive object</returns>
    public object CreateArchive()
    {
        // Create the data
        BundleFile data = new()
        {
            BootHeader = new BundleBootHeader(),
            FilePack = new FilePackMaster(),
        };

        // Configure the data
        Config.ConfigureIpkData(data.BootHeader);

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
        var entry = (BundleFile_FileEntry)fileEntry;
        var ipk = (BundleFile)archive;

        yield return new DuoGridItemViewModel(
            header: new ResourceLocString(nameof(Resources.Archive_FileInfo_Size)), 
            text: $"{ByteSize.FromBytes(entry.FileSize)}");

        if (entry.IsCompressed)
            yield return new DuoGridItemViewModel(
                header: new ResourceLocString(nameof(Resources.Archive_FileInfo_SizeComp)), 
                text: $"{ByteSize.FromBytes(entry.CompressedSize)}");

        if (ipk.FilePack.Files.Contains(entry))
            yield return new DuoGridItemViewModel(
                header: new ResourceLocString(nameof(Resources.Archive_FileInfo_Pointer)), 
                text: $"0x{entry.Offsets.First() + ipk.BootHeader.BaseOffset:X16}", 
                minUserLevel: UserLevel.Technical);
            
        yield return new DuoGridItemViewModel(
            header: new ResourceLocString(nameof(Resources.Archive_FileInfo_IsComp)), 
            text: $"{entry.IsCompressed}"); // TODO-UPDATE: Localize true/false
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
        return new BundleFile_FileEntry
        {
            Path = new BinarySerializer.UbiArt.Path(this.CombinePaths(directory, fileName)),
            Pre_BundleVersion = ((BundleFile)archive).BootHeader.Version
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
        var entry = (BundleFile_FileEntry)fileEntry;

        return encoded ? entry.ArchiveSize : entry.FileSize;
    }

    public void Dispose()
    {
        Context.Dispose();
    }

    #endregion

    #region Classes

    /// <summary>
    /// The archive file generator for .ipk files
    /// </summary>
    private class IPKFileGenerator : IFileGenerator<BundleFile_FileEntry>
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="ipkData">The .ipk file data</param>
        /// <param name="archiveStream">The archive file stream</param>
        public IPKFileGenerator(BundleFile ipkData, Stream archiveStream)
        {
            // Get the .ipk data
            IPKData = ipkData;

            // If the block is compressed, decompress it to a temporary file
            if (ipkData.BootHeader.IsBlockCompressed)
            {
                // Get the temp path and create the file
                TempFile = new TempFile(true);

                // Set the stream to the temp file
                Stream = File.Open(TempFile.TempPath, FileMode.Open, FileAccess.ReadWrite);

                // Set the archive stream position
                archiveStream.Position = ipkData.BootHeader.BaseOffset;

                byte[] buffer = new byte[IPKData.BootHeader.BlockCompressedSize];
                archiveStream.Read(buffer, 0, buffer.Length);

                // Create a memory stream
                using var memStream = new MemoryStream(buffer);

                // Decompress the block
                BundleBootHeader.GetEncoder(IPKData.BootHeader.Version, IPKData.BootHeader.BlockSize).DecodeStream(memStream, Stream);

                // Set the stream to be disposed
                DisposeStream = true;
            }
            else
            {
                // Set the stream to the .ipk archive
                Stream = archiveStream;

                // Set the stream not to be disposed
                DisposeStream = false;
            }
        }

        private TempFile? TempFile { get; }

        /// <summary>
        /// The .ipk file data
        /// </summary>
        private BundleFile IPKData { get; }

        /// <summary>
        /// The stream
        /// </summary>
        private Stream Stream { get; }

        /// <summary>
        /// Indicates if the stream should be disposed
        /// </summary>
        private bool DisposeStream { get; }

        /// <summary>
        /// Gets the number of files which can be retrieved from the generator
        /// </summary>
        public int Count => IPKData.FilePack.Files.Length;

        /// <summary>
        /// Gets the file stream for the specified key
        /// </summary>
        /// <param name="fileEntry">The file entry to get the stream for</param>
        /// <returns>The stream</returns>
        public Stream GetFileStream(BundleFile_FileEntry fileEntry)
        {
            // Make sure we have offsets
            if (fileEntry.Offsets?.Any() != true)
                throw new Exception("No offsets were found");

            // NOTE: We only care about getting the bytes from the first offset as they all point to identical bytes (this is used for memory optimization on certain platforms)
            var offset = fileEntry.Offsets.First();

            // Set the position
            Stream.Position = (long)(IPKData.BootHeader.IsBlockCompressed ? offset : (offset + IPKData.BootHeader.BaseOffset));

            byte[] buffer = new byte[fileEntry.ArchiveSize];
            Stream.Read(buffer, 0, buffer.Length);

            // Read the bytes into the buffer
            return new MemoryStream(buffer);
        }

        /// <summary>
        /// Disposes the generator
        /// </summary>
        public void Dispose()
        {
            if (DisposeStream)
                Stream?.Dispose();

            TempFile?.Dispose();
        }
    }

    #endregion
}