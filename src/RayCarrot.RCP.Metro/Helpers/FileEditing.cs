using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;

namespace RayCarrot.RCP.Metro;

public class FileEditing : IDisposable
{
    public FileEditing(string fileName)
    {
        _tempDir = new TempDirectory(true);
        FilePath = _tempDir.TempPath + fileName;
    }

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    private readonly TempDirectory _tempDir;

    public FileSystemPath FilePath { get; }

    private byte[] ComputeFileHash(HashAlgorithm hashAlgorithm)
    {
        using FileStream tmpFile = File.OpenRead(FilePath);
        return hashAlgorithm.ComputeHash(tmpFile);
    }

    public async Task<bool> ExecuteAsync(string fileExtension, bool readOnly, LoadState state, Action<FileSystemPath> createFileAction)
    {
        // Get the program to open the file with
        FileSystemPath? programPath = await Services.AssociatedFileEditorsManager.RequestFileEditorAssociatonAsync(fileExtension);

        if (programPath == null)
            return false;

        // Export the file
        createFileAction(FilePath);

        using HashAlgorithm sha1 = HashAlgorithm.Create();

        // Get the original file hash
        byte[] originalHash = ComputeFileHash(sha1);

        // If read-only set the attribute
        if (readOnly)
        {
            var info = FilePath.GetFileInfo();
            info.Attributes |= FileAttributes.ReadOnly;
        }

        string args = String.Empty;

        // Add specific arguments for common editor programs so that they open a new instance. If not then the new
        // process will close immediately as it will re-use the already existing one which means the WaitForExitAsync
        // won't wait for the program to close.
        if (programPath.Value.Name == "Code.exe") // VS Code
            args += $"--new-window ";
        else if (programPath.Value.Name == "notepad++.exe") // Notepad++
            args += $"-multiInst ";

        args += $"\"{FilePath}\"";

        // Open the process
        using (Process? p = await Services.File.LaunchFileAsync(programPath.Value, arguments: args))
        {
            // Ignore if the file wasn't opened
            if (p == null)
            {
                Logger.Trace("The file was not opened");
                return false;
            }

            state.SetStatus(String.Format(Resources.WaitForEditorToClose, programPath.Value.RemoveFileExtension().Name));
            state.SetCanCancel(true);

            // Wait for the process to close...
            try
            {
                await p.WaitForExitAsync(state.CancellationToken);
            }
            catch (OperationCanceledException ex)
            {
                Logger.Trace(ex, "Canceled editing file");

                try
                {
                    // Attempt to close the process
                    p.CloseMainWindow();
                }
                catch (Exception ex2)
                {
                    Logger.Warn(ex2, "Closing file editor process after cancellation");
                }
            }

            state.SetStatus(String.Empty);
            state.SetCanCancel(false);
        }

        // If read-only we don't need to check if it has been modified
        if (readOnly)
            return false;

        // Get the new hash
        byte[] newHash = ComputeFileHash(sha1);

        // Check if the file was modified
        bool modified = !originalHash.SequenceEqual(newHash);

        // Check if the file has been modified
        if (!originalHash.SequenceEqual(newHash))
            Logger.Trace("The file was modified");
        else
            Logger.Trace("The file was not modified");

        return modified;
    }

    public void Dispose()
    {
        _tempDir.Dispose();
    }
}