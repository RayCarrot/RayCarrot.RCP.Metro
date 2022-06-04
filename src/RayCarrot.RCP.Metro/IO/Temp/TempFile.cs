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
    public TempFile(bool createFile)
    {
        // TODO-UPDATE: Don't use Path.GetTempFileName since it can run out of names. Instead create all temp paths under RCP sub-folder
        //              and name them based on a GUID perhaps?

        if (createFile)
        {
            // Get the temp path and create the file
            TempPath = Path.GetTempFileName();

            // Get the file info
            var info = TempPath.GetFileInfo();

            // Set the attribute to temporary
            info.Attributes |= FileAttributes.Temporary;
        }
        else
        {
            // Set the temp path
            TempPath = GetTempFilePath(new FileExtension(".tmp"));
        }

        Logger.Debug("A new temp file has been created under {0}", TempPath);
    }

    /// <summary>
    /// Creates a new temporary file with a custom file extension without creating the file
    /// </summary>
    /// <param name="ext">The file extension</param>
    public TempFile(FileExtension ext)
    {
        // Set the temp path
        TempPath = GetTempFilePath(ext ?? new FileExtension(".tmp"));

        Logger.Debug("A new temp file has been created under {0}", TempPath);
    }

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    /// <summary>
    /// The path of the temporary file
    /// </summary>
    public override FileSystemPath TempPath { get; }

    private static FileSystemPath GetTempFilePath(FileExtension ext)
    {
        // Get the temp path
        FileSystemPath tempFile;

        // Get a random temp path until one does not exist
        do
        {
            tempFile = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + ext.FileExtensions);
        } while (tempFile.FileExists);

        return tempFile;
    }

    /// <summary>
    /// Removes the temporary file
    /// </summary>
    public override void Dispose()
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