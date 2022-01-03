using System;
using System.Collections.Generic;
using System.IO;
using RayCarrot.Binary;
using RayCarrot.IO;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// Defines an archive data manager
/// </summary>
public interface IArchiveDataManager
{
    /// <summary>
    /// The path separator character to use. This is usually \ or /.
    /// </summary>
    char PathSeparatorCharacter { get; }

    /// <summary>
    /// Indicates if directories can be created and deleted
    /// </summary>
    bool CanModifyDirectories { get; }

    /// <summary>
    /// The file extension for the archive file
    /// </summary>
    FileExtension ArchiveFileExtension { get; }

    /// <summary>
    /// The serializer settings to use for the archive
    /// </summary>
    BinarySerializerSettings SerializerSettings { get; } // Old
    object ContextSettings { get; } // New, keep here while migrating to BinarySerializer

    /// <summary>
    /// The default archive file name to use when creating an archive
    /// </summary>
    string DefaultArchiveFileName { get; }

    /// <summary>
    /// Gets the configuration UI to use for creator
    /// </summary>
    object? GetCreatorUIConfig { get; }

    /// <summary>
    /// Encodes the file data from the input stream, or nothing if the data does not need to be encoded
    /// </summary>
    /// <param name="inputStream">The input data stream to encode</param>
    /// <param name="outputStream">The output data stream for the encoded data</param>
    /// <param name="fileEntry">The file entry for the file to encode</param>
    void EncodeFile(Stream inputStream, Stream outputStream, object fileEntry);

    /// <summary>
    /// Decodes the file data from the input stream, or nothing if the data is not encoded
    /// </summary>
    /// <param name="inputStream">The input data stream to decode</param>
    /// <param name="outputStream">The output data stream for the decoded data</param>
    /// <param name="fileEntry">The file entry for the file to decode</param>
    void DecodeFile(Stream inputStream, Stream outputStream, object fileEntry);

    /// <summary>
    /// Gets the file data from the archive using a generator
    /// </summary>
    /// <param name="generator">The generator</param>
    /// <param name="fileEntry">The file entry</param>
    /// <returns>The encoded file data</returns>
    Stream GetFileData(IDisposable generator, object fileEntry);

    /// <summary>
    /// Writes the files to the archive
    /// </summary>
    /// <param name="generator">The generator</param>
    /// <param name="archive">The loaded archive data</param>
    /// <param name="outputFileStream">The file output stream for the archive</param>
    /// <param name="files">The files to include</param>
    void WriteArchive(IDisposable? generator, object archive, Stream outputFileStream, IList<ArchiveFileItem> files);

    /// <summary>
    /// Loads the archive data
    /// </summary>
    /// <param name="archive">The archive data</param>
    /// <param name="archiveFileStream">The archive file stream</param>
    /// <param name="fileName">The name of the file the archive is loaded from, if available</param>
    /// <returns>The archive data</returns>
    ArchiveData LoadArchiveData(object archive, Stream archiveFileStream, string fileName);

    /// <summary>
    /// Loads the archive from a stream
    /// </summary>
    /// <param name="archiveFileStream">The file stream for the archive</param>
    /// <returns>The archive data</returns>
    object LoadArchive(Stream archiveFileStream);

    /// <summary>
    /// Creates a new archive object
    /// </summary>
    /// <returns>The archive object</returns>
    object CreateArchive();

    /// <summary>
    /// Gets info to display about the file
    /// </summary>
    /// <param name="archive">The archive</param>
    /// <param name="fileEntry">The file entry</param>
    /// <returns>The info items to display</returns>
    IEnumerable<DuoGridItemViewModel> GetFileInfo(object archive, object fileEntry);

    /// <summary>
    /// Gets a new file entry object for the file
    /// </summary>
    /// <param name="archive">The archive</param>
    /// <param name="directory">The directory</param>
    /// <param name="fileName">The file name</param>
    /// <returns>The file entry object</returns>
    object GetNewFileEntry(object archive, string directory, string fileName);

    /// <summary>
    /// Gets the size of the file entry
    /// </summary>
    /// <param name="fileEntry">The file entry object</param>
    /// <param name="encoded">True if the size is for the encoded file, false if for the decoded file</param>
    /// <returns>The size, or null if it could not be determined</returns>
    long? GetFileSize(object fileEntry, bool encoded);

    /// <summary>
    /// Occurs when a file is being written to an archive
    /// </summary>
    event EventHandler<ValueEventArgs<ArchiveFileItem>>? OnWritingFileToArchive;
}