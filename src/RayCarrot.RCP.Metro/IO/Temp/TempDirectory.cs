using System.IO;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// A temporary directory
/// </summary>
public sealed class TempDirectory : TempFileSystemEntry
{
    /// <summary>
    /// Creates a new temporary directory
    /// </summary>
    /// <param name="createDir">Indicates if the directory should be created</param>
    public TempDirectory(bool createDir)
    {
        // Get the temp path
        TempPath = GetTempDirPath();

        if (createDir)
            // Create the directory
            Directory.CreateDirectory(TempPath);

        Logger.Debug("A new temp directory has been created under {0}", TempPath);
    }

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    
    private static Random Random { get; }= new();

    /// <summary>
    /// The path of the temporary directory
    /// </summary>
    public override FileSystemPath TempPath { get; }

    private static FileSystemPath GetTempDirPath()
    {
        // Get the temp directory
        FileSystemPath tempBaseDir = AppFilePaths.TempPath;

        FileSystemPath tempDir;

        // Generate a random temp path until one does not exist
        // NOTE: Previously we used a GUID, however they're long and would sometimes cause the full path to be too
        //       long to be handled by the .NET Framework IO APIs. When we migrate to .NET this should no longer be
        //       an issue and we can revert to using a GUID.
        int num = Random.Next();
        do
        {
            tempDir = tempBaseDir + $"{num}";
            num = Random.Next();
        } while (tempDir.DirectoryExists);

        return tempDir;
    }

    protected override void Dispose(bool disposing)
    {
        try
        {
            // Delete the temp directory
            TempPath.DeleteDirectory();
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Deleting temp directory");
        }
    }
}