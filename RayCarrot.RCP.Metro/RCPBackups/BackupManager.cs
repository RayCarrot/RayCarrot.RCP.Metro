using RayCarrot.CarrotFramework.Abstractions;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using Nito.AsyncEx;
using RayCarrot.IO;
using RayCarrot.UI;

namespace RayCarrot.RCP.Metro
{   
    /// <summary>
    /// The backup manager
    /// </summary>
    public class BackupManager
    {
        #region Static Constructor

        static BackupManager()
        {
            AsyncLock = new AsyncLock();
        }

        #endregion

        #region Private Static Properties

        /// <summary>
        /// The async lock for backup and restore operations
        /// </summary>
        private static AsyncLock AsyncLock { get; }

        #endregion

        #region Private Methods

        /// <summary>
        /// Performs a backup on the game
        /// </summary>
        /// <param name="backupInformation">The backup information</param>
        /// <returns>True if the backup was successful</returns>
        private static async Task<bool> PerformBackupAsync(IBackupInfo backupInformation)
        {
            // Get the destination directory
            FileSystemPath destinationDir = backupInformation.BackupLocation;

            // Use a temporary directory to store the files in case of error
            using (var tempDir = new TempDirectory(false))
            {
                try
                {
                    // Check if a backup already exists
                    if (destinationDir.DirectoryExists)
                        // Create a new temp backup
                        RCFRCP.File.MoveDirectory(destinationDir, tempDir.TempPath, true);

                    // Enumerate the backup information
                    foreach (var item in backupInformation.BackupDirectories)
                    {
                        // Check if the entire directory should be copied
                        if (item.IsEntireDir())
                        {
                            // Copy the directory   
                            RCFRCP.File.CopyDirectory(item.DirPath, destinationDir + item.ID, true, true);
                        }
                        else
                        {
                            // Get the files
                            var files = Directory.GetFiles(item.DirPath, item.ExtensionFilter ?? "*", item.SearchOption);

                            // Backup each file
                            foreach (FileSystemPath file in files)
                            {
                                // Get the destination file
                                var destFile = destinationDir + item.ID + file.GetRelativePath(item.DirPath);

                                // Create the parent directory
                                Directory.CreateDirectory(destFile.Parent);

                                // Copy the file
                                File.Copy(file, destFile);
                            }
                        }
                    }

                    // Check if any files were backed up
                    if (!destinationDir.DirectoryExists)
                    {
                        await RCFUI.MessageUI.DisplayMessageAsync(String.Format(Resources.Backup_MissingFilesError, backupInformation.GameDisplayName), Resources.Backup_FailedHeader, MessageType.Error);

                        if (tempDir.TempPath.DirectoryExists)
                            // Restore temp backup
                            RCFRCP.File.MoveDirectory(tempDir.TempPath, destinationDir, true);

                        return false;
                    }

                    RCFCore.Logger?.LogInformationSource($"Backup complete");

                    return true;
                }
                catch (Exception ex)
                {
                    ex.HandleError("Performing backup");

                    if (tempDir.TempPath.DirectoryExists)
                        // Restore temp backup
                        RCFRCP.File.MoveDirectory(tempDir.TempPath, destinationDir, true);

                    RCFCore.Logger?.LogInformationSource($"Backup failed - clean up succeeded");

                    throw;
                }
            }
        }

        /// <summary>
        /// Performs a compressed backup on the game
        /// </summary>
        /// <param name="backupInformation">The backup information</param>
        /// <returns>True if the backup was successful</returns>
        private static bool PerformCompressedBackup(IBackupInfo backupInformation)
        {
            // Get the destination file
            FileSystemPath destinationFile = backupInformation.CompressedBackupLocation;

            using (var tempFile = new TempFile(false))
            {
                try
                {
                    // Check if a backup already exists
                    if (destinationFile.FileExists)
                        // Create a new temp backup
                        RCFRCP.File.MoveFile(destinationFile, tempFile.TempPath, true);

                    // Create the parent directory
                    Directory.CreateDirectory(destinationFile.Parent);

                    // Create the compressed file
                    using (var fileStream = File.OpenWrite(destinationFile))
                    {
                        using (var zip = new ZipArchive(fileStream, ZipArchiveMode.Create))
                        {
                            // Enumerate the backup information
                            foreach (var item in backupInformation.BackupDirectories)
                            {
                                // Get the files
                                var files = Directory.GetFiles(item.DirPath, item.ExtensionFilter ?? "*", item.SearchOption);

                                // Backup each file
                                foreach (FileSystemPath file in files)
                                {
                                    // Get the destination file
                                    var destFile = item.ID + file.GetRelativePath(item.DirPath);

                                    // Copy the file
                                    zip.CreateEntryFromFile(file, destFile, CompressionLevel.Optimal);
                                }
                            }
                        }
                    }

                    RCFCore.Logger?.LogInformationSource($"Backup complete");

                    return true;
                }
                catch (Exception ex)
                {
                    ex.HandleError("Performing compressed backup", backupInformation);

                    // Check if a temp backup exists
                    if (tempFile.TempPath.FileExists)
                        // Restore temp backup
                        RCFRCP.File.MoveFile(tempFile.TempPath, destinationFile, true);

                    RCFCore.Logger?.LogInformationSource($"Compressed backup failed - clean up succeeded");

                    throw;
                }
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Performs a backup on the game
        /// </summary>
        /// <param name="backupInformation">The backup information</param>
        /// <returns>True if the backup was successful</returns>
        public async Task<bool> BackupAsync(IBackupInfo backupInformation)
        {
            using (await AsyncLock.LockAsync())
            {
                RCFCore.Logger?.LogInformationSource($"A backup has been requested for {backupInformation.GameDisplayName}");

                try
                {
                    // Make sure we have write access to the backup location
                    if (!RCFRCP.File.CheckDirectoryWriteAccess(RCFRCP.Data.BackupLocation + AppViewModel.BackupFamily))
                    {
                        RCFCore.Logger?.LogInformationSource($"Backup failed - backup location lacks write access");

                        // Request to restart as admin
                        await RCFRCP.App.RequestRestartAsAdminAsync();

                        return false;
                    }

                    // Get the backup information and group items by ID
                    var backupInfoByID = backupInformation.BackupDirectories.GroupBy(x => x.ID).ToList();

                    RCFCore.Logger?.LogDebugSource($"{backupInfoByID.Count} backup directory ID groups were found");

                    // Get the backup info
                    var backupInfo = new List<BackupDir>();

                    // Get the latest version from each group
                    foreach (var group in backupInfoByID)
                    {
                        if (group.Count() == 1)
                        {
                            backupInfo.Add(group.First());
                            continue;
                        }

                        RCFCore.Logger?.LogDebugSource($"ID {group.Key} has multiple items");

                        // Find which group is the latest one
                        var groupItems = new Dictionary<BackupDir, DateTime>();

                        foreach (BackupDir item in group)
                        {
                            // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
                            if (!item.DirPath.DirectoryExists)
                                groupItems.Add(item, DateTime.MinValue);
                            else
                                groupItems.Add(item, Directory.GetFiles(item.DirPath, item.ExtensionFilter, item.SearchOption).Select(x => new FileInfo(x).LastWriteTime).OrderByDescending(x => x).FirstOrDefault());
                        }

                        // Get the latest directory
                        var latestDir = groupItems.OrderByDescending(x => x.Value).First().Key;

                        // Add the latest directory
                        backupInfo.Add(latestDir);

                        RCFCore.Logger?.LogDebugSource($"The most recent backup directory was found under {latestDir.DirPath}");
                    }

                    // Make sure all the directories to back up exist
                    if (!backupInfo.Select(x => x.DirPath).DirectoriesExist())
                    {
                        RCFCore.Logger?.LogInformationSource($"Backup failed - the input directories could not be found");

                        await RCFUI.MessageUI.DisplayMessageAsync(String.Format(Resources.Backup_MissingDirectoriesError, backupInformation.GameDisplayName), Resources.Backup_FailedHeader, MessageType.Error);

                        return false;
                    }

                    // Check if the backup should be compressed
                    bool compress = RCFRCP.Data.CompressBackups;

                    RCFCore.Logger?.LogDebugSource(compress ? $"The backup will be compressed" : $"The backup will not be compressed");

                    // Perform the backup and keep track if it succeeded
                    bool success = compress ? PerformCompressedBackup(backupInformation) : await PerformBackupAsync(backupInformation);

                    // Get the backup locations
                    var compressedLocation = backupInformation.CompressedBackupLocation;
                    var normalLocation = backupInformation.BackupLocation;

                    // Check if the non-relevant one exists, as there should not be a compressed and normal backup at the same time
                    try
                    {
                        if (compress && normalLocation.DirectoryExists)
                        {
                            // Delete the directory
                            RCFRCP.File.DeleteDirectory(normalLocation);

                            RCFCore.Logger?.LogInformationSource("Non-compressed backup was deleted due to a compressed backup having been performed");
                        }
                        else if (!compress && compressedLocation.FileExists)
                        {
                            // Delete the file
                            RCFRCP.File.DeleteFile(compressedLocation);

                            RCFCore.Logger?.LogInformationSource("Compressed backup was deleted due to a non-compressed backup having been performed");
                        }
                    }
                    catch (Exception ex)
                    {
                        ex.HandleError("Deleting leftover backups from previous compression setting");
                    }

                    return success;
                }
                catch (Exception ex)
                {   
                    // Handle error
                    ex.HandleError("Backing up game", backupInformation);

                    // Display message to user
                    await RCFUI.MessageUI.DisplayMessageAsync(String.Format(Resources.Backup_Failed, backupInformation.GameDisplayName), Resources.Backup_FailedHeader, MessageType.Error);

                    // Return that backup did not succeed
                    return false;
                }
            }
        }

        /// <summary>
        /// Restores a backup on the game
        /// </summary>
        /// <param name="backupInformation">The backup information</param>
        /// <returns>True if the backup was successful</returns>
        public async Task<bool> RestoreAsync(IBackupInfo backupInformation)
        {
            using (await AsyncLock.LockAsync())
            {
                RCFCore.Logger?.LogInformationSource($"A backup restore has been requested for {backupInformation.GameDisplayName}");

                try
                {
                    // Get the backup directory
                    var existingBackup = backupInformation.ExistingBackupLocation;

                    // Make sure a backup exists
                    if (!existingBackup?.Exists ?? true)
                    {
                        RCFCore.Logger?.LogInformationSource($"Restore failed - the input location could not be found");

                        await RCFUI.MessageUI.DisplayMessageAsync(String.Format(Resources.Restore_MissingBackup, backupInformation.GameDisplayName), Resources.Restore_FailedHeader, MessageType.Error);

                        return false;
                    }

                    // Get the backup information
                    var backupInfo = backupInformation.BackupDirectories;

                    // Make sure we have write access to the restore destinations
                    if (backupInfo.Any(x => !RCFRCP.File.CheckDirectoryWriteAccess(x.DirPath)))
                    {
                        RCFCore.Logger?.LogInformationSource($"Restore failed - one or more restore destinations lack write access");

                        // Request to restart as admin
                        await RCFRCP.App.RequestRestartAsAdminAsync();

                        return false;
                    }

                    var backupLocation = existingBackup.Value;

                    var isCompressed = backupLocation.FileExists;

                    using (var tempDir = new TempDirectory(true))
                    {
                        using (var archiveTempDir = new TempDirectory(true))
                        {
                            try
                            {
                                // If the backup is an archive, extract it
                                if (isCompressed)
                                {
                                    using (var file = File.OpenRead(backupLocation))
                                    using (var zip = new ZipArchive(file, ZipArchiveMode.Read))
                                        zip.ExtractToDirectory(archiveTempDir.TempPath);
                                }

                                // Enumerate the backup information
                                foreach (BackupDir item in backupInfo)
                                {
                                    // Move existing files if the directory exists to temp
                                    if (item.DirPath.DirectoryExists)
                                    {
                                        // Check if the entire directory should be moved
                                        if (item.IsEntireDir())
                                        {
                                            // Get the destination directory
                                            var destDir = tempDir.TempPath + backupInfo.IndexOf(item).ToString() + item.DirPath.Name;

                                            // Move the directory
                                            RCFRCP.File.MoveDirectory(item.DirPath, destDir, true);
                                        }
                                        else
                                        {
                                            // Move each file
                                            foreach (FileSystemPath file in Directory.GetFiles(item.DirPath, item.ExtensionFilter, item.SearchOption))
                                            {
                                                // Get the destination file
                                                var destFile = tempDir.TempPath + backupInfo.IndexOf(item).ToString() + file.GetRelativePath(item.DirPath);

                                                // Move the file
                                                RCFRCP.File.MoveFile(file, destFile, true);
                                            }
                                        }
                                    }

                                    // Get the combined directory path
                                    var dirPath = (isCompressed ? archiveTempDir.TempPath : backupLocation) + item.ID;

                                    // Restore the backup
                                    if (dirPath.DirectoryExists)
                                        RCFRCP.File.CopyDirectory(dirPath, item.DirPath, false, true);
                                }
                            }
                            catch
                            {
                                // Restore temp backup
                                foreach (BackupDir item in backupInfo)
                                {
                                    // Get the combined directory path
                                    var dirPath = tempDir.TempPath + backupInfo.IndexOf(item).ToString();

                                    // Make sure there is a directory to restore
                                    if (!dirPath.DirectoryExists)
                                        continue;

                                    // Check if the entire directory should be moved
                                    if (item.IsEntireDir())
                                    {
                                        // Get the temp directory
                                        var currentTempDir = dirPath + item.DirPath.Name;

                                        // Get the destination directory
                                        var destDir = item.DirPath;

                                        // Move the directory
                                        RCFRCP.File.MoveDirectory(currentTempDir, destDir, true);
                                    }
                                    else
                                    {
                                        // Restore each directory
                                        foreach (FileSystemPath dir in Directory.GetDirectories(dirPath, "*", SearchOption.AllDirectories))
                                        {
                                            // Get the destination directory
                                            var destDir = item.DirPath.Parent + dir.GetRelativePath(item.DirPath);

                                            // Create the directory
                                            Directory.CreateDirectory(destDir);
                                        }

                                        // Restore each file
                                        foreach (FileSystemPath file in Directory.GetFiles(dirPath, "*", SearchOption.AllDirectories))
                                        {
                                            // Get the destination file
                                            var destFile = item.DirPath.Parent + file.GetRelativePath(item.DirPath);

                                            // Move the file
                                            RCFRCP.File.MoveFile(file, destFile, true);
                                        }
                                    }
                                }

                                RCFCore.Logger?.LogInformationSource($"Restore failed - clean up succeeded");

                                throw;
                            }
                        }
                    }

                    RCFCore.Logger?.LogInformationSource($"Restore complete");

                    return true;
                }
                catch (Exception ex)
                {
                    ex.HandleCritical("Restoring game", backupInformation);
                    await RCFUI.MessageUI.DisplayMessageAsync(String.Format(Resources.Restore_Failed, backupInformation.GameDisplayName), Resources.Restore_FailedHeader, MessageType.Error);

                    return false;
                }
            }
        }

        #endregion
    }
}