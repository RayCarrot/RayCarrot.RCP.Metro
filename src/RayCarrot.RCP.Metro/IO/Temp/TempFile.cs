using System;
using System.IO;
using NLog;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// A temporary file
/// </summary>
public sealed class TempFile : TempFileSystemEntry
{
    /// <summary>
    /// Creates a new temporary file with a default name
    /// </summary>
    /// <param name="createFile">Indicates if the temporary file should be created</param>
    /// <param name="ext">The file extension, or null for the default extension</param>
    public TempFile(bool createFile, FileExtension? ext = null)
    {
        if (createFile)
        {
            // Get the temp path
            TempPath = GetTempFilePath(ext);

            // Create the file
            File.Create(TempPath).Dispose();

            // Get the file info
            FileInfo info = TempPath.GetFileInfo();

            // Set the temporary attribute flag
            info.Attributes |= FileAttributes.Temporary;
        }
        else
        {
            // Get the temp path, but don't create a file there
            TempPath = GetTempFilePath(ext);
        }

        Logger.Debug("A new temp file has been created under {0}", TempPath);
    }

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    private const string DefaultExt = ".tmp";

    /// <summary>
    /// The path of the temporary file
    /// </summary>
    public override FileSystemPath TempPath { get; }

    private static FileSystemPath GetTempFilePath(FileExtension? ext = null)
    {
        ext ??= new FileExtension(DefaultExt);

        // Get the temp directory
        FileSystemPath tempBaseDir = AppFilePaths.TempPath;

        FileSystemPath tempFile;

        // Generate a random temp path until one does not exist
        do
        {
            tempFile = tempBaseDir +  $"{Guid.NewGuid()}{ext.FileExtensions}";
        } while (tempFile.FileExists);

        return tempFile;
    }

    protected override void Dispose(bool disposing)
    {
        try
        {
            // Delete the temp file
            TempPath.DeleteFile();
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Deleting temp file");
        }
    }
}