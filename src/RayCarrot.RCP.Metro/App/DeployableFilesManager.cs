using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using NLog;

namespace RayCarrot.RCP.Metro;

public class DeployableFilesManager
{
    public DeployableFilesManager(FileManager file)
    {
        File = file;
    }

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    private readonly Dictionary<DeployableFile, (FileSystemPath FilePath, Func<byte[]> GetBytes)> _files = new()
    {
        [DeployableFile.AdminWorker] = (AppFilePaths.AdminWorkerPath, () => Files.AdminWorker),
        [DeployableFile.Updater] = (AppFilePaths.UpdaterFilePath, () => Files.Rayman_Control_Panel_Updater),
        [DeployableFile.Uninstaller] = (AppFilePaths.UninstallFilePath, () => Files.Uninstaller),
    };

    public FileManager File { get; }

    public FileSystemPath GetFilePath(DeployableFile file) => _files[file].FilePath;

    public FileSystemPath DeployFile(DeployableFile file)
    {
        FileSystemPath filePath = _files[file].FilePath;

        Directory.CreateDirectory(filePath.Parent);
        System.IO.File.WriteAllBytes(filePath, _files[file].GetBytes());

        return filePath;
    }

    public async Task CleanupFilesAsync()
    {
        foreach (DeployableFile file in _files.Keys)
            await CleanupFileAsync(file);
    }

    public async Task CleanupFileAsync(DeployableFile file)
    {
        FileSystemPath filePath = _files[file].FilePath;

        if (!filePath.FileExists) 
            return;
        
        int retryTime = 0;

        // Wait until we can write to the file (such as if it is closing after an update)
        while (!File.CheckFileWriteAccess(filePath))
        {
            retryTime++;

            // Try for 2 seconds first
            if (retryTime < 20)
            {
                Logger.Debug("The deployable file {0} can not be removed due to not having write access. Retrying {1}", file, retryTime);

                await Task.Delay(100);
            }
            // Now it's taking a long time... Try for 10 more seconds
            else if (retryTime < 70)
            {
                Logger.Warn("The deployable file {0} can not be removed due to not having write access. Retrying {1}", file, retryTime);

                await Task.Delay(200);
            }
            // Give up
            else
            {
                Logger.Fatal("The deployable file {0} can not be removed due to not having write access", file);
                return;
            }
        }

        try
        {
            // Remove the file
            File.DeleteFile(filePath);

            Logger.Info("The deployable file {0} has been removed", file);
        }
        catch (Exception ex)
        {
            Logger.Fatal(ex, "Removing deployable file {0}", file);
        }
    }

    public enum DeployableFile
    {
        AdminWorker,
        Updater,
        Uninstaller,
    }
}