﻿using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using Nito.AsyncEx;
using RayCarrot.IO;
using RayCarrot.Logging;
using RayCarrot.WPF;

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

        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        public BackupManager()
        {
            // Get a new file manager
            FileManager = RCPServices.File;
        }

        #endregion

        #region Private Static Properties

        /// <summary>
        /// The async lock for backup and restore operations
        /// </summary>
        private static AsyncLock AsyncLock { get; }

        #endregion

        #region Private Properties

        /// <summary>
        /// The file manager to use
        /// </summary>
        private IFileManager FileManager { get; }

        #endregion

        #region Private Methods

        /// <summary>
        /// Performs a backup on the game
        /// </summary>
        /// <param name="backupDirs">The backup directories</param>
        /// <param name="destinationDir">The destination directory path</param>
        /// <param name="gameDisplayName">The game display name</param>
        /// <returns>True if the backup was successful</returns>
        private async Task<bool> PerformBackupAsync(IEnumerable<BackupDir> backupDirs, FileSystemPath destinationDir, string gameDisplayName)
        {
            // Use a temporary directory to store the files in case of error
            using var tempDir = new TempDirectory(false);

            bool hasCreatedTempBackup = false;
                
            try
            {
                // Check if a backup already exists
                if (destinationDir.DirectoryExists)
                {
                    // Create a new temp backup
                    FileManager.MoveDirectory(destinationDir, tempDir.TempPath, true, true);
                }

                hasCreatedTempBackup = true;

                // Backup each directory
                foreach (BackupDir item in backupDirs)
                {
                    var itemDestination = destinationDir + item.ID;

                    // Check if the entire directory should be copied
                    if (item.IsEntireDir())
                        // Copy the directory   
                        FileManager.CopyDirectory(item.DirPath, itemDestination, true, true);
                    else
                        // Backup the files
                        FileManager.CopyFiles(item, itemDestination, true);
                }

                // Check if any files were backed up
                if (!destinationDir.DirectoryExists)
                {
                    await Services.MessageUI.DisplayMessageAsync(String.Format(Resources.Backup_MissingFilesError, gameDisplayName), Resources.Backup_FailedHeader, MessageType.Error);

                    if (tempDir.TempPath.DirectoryExists)
                        // Restore temp backup
                        FileManager.MoveDirectory(tempDir.TempPath, destinationDir, true, true);

                    return false;
                }

                RL.Logger?.LogInformationSource($"Backup complete");

                return true;
            }
            catch (Exception ex)
            {
                ex.HandleError("Performing backup");

                // Restore temp backup
                if (tempDir.TempPath.DirectoryExists)
                    // Only replace files if the crash happened after creating the temp backup and during the backup operation
                    FileManager.MoveDirectory(tempDir.TempPath, destinationDir, hasCreatedTempBackup, false);
                else if (hasCreatedTempBackup)
                    // Delete incomplete backup
                    FileManager.DeleteDirectory(destinationDir);

                RL.Logger?.LogInformationSource($"Backup failed - clean up succeeded");

                throw;
            }
        }

        /// <summary>
        /// Performs a compressed backup on the game
        /// </summary>
        /// <param name="backupDirs">The backup directories</param>
        /// <param name="destinationFile">The destination file path</param>
        /// <param name="gameDisplayName">The game display name</param>
        /// <returns>True if the backup was successful</returns>
        private async Task<bool> PerformCompressedBackupAsync(IEnumerable<BackupDir> backupDirs, FileSystemPath destinationFile, string gameDisplayName)
        {
            using var tempFile = new TempFile(false);

            bool hasCreatedTempBackup = false;

            try
            {
                // Check if a backup already exists
                if (destinationFile.FileExists)
                    // Create a new temp backup
                    FileManager.MoveFile(destinationFile, tempFile.TempPath, true);

                hasCreatedTempBackup = true;

                // Create the parent directory
                Directory.CreateDirectory(destinationFile.Parent);

                // Keep track if any files have been backed up
                bool backedUp = false;

                // Create the compressed file
                using (var fileStream = File.OpenWrite(destinationFile))
                {
                    // Open the zip archive
                    using var zip = new ZipArchive(fileStream, ZipArchiveMode.Create);

                    // Enumerate the backup information
                    foreach (var item in backupDirs)
                    {
                        // Backup each file
                        foreach (FileSystemPath file in Directory.GetFiles(item.DirPath, item.SearchPattern ?? "*", item.SearchOption))
                        {
                            // Get the destination file
                            var destFile = item.ID + (file - item.DirPath);

                            // Copy the file
                            zip.CreateEntryFromFile(file, destFile, CompressionLevel.Optimal);

                            backedUp = true;
                        }
                    }
                }

                // Check if any files were backed up
                if (!backedUp)
                {
                    await Services.MessageUI.DisplayMessageAsync(String.Format(Resources.Backup_MissingFilesError, gameDisplayName), Resources.Backup_FailedHeader, MessageType.Error);
                    
                    // Check if a temp backup exists
                    if (tempFile.TempPath.FileExists)
                        // Restore temp backup
                        FileManager.MoveFile(tempFile.TempPath, destinationFile, true);
                    else
                        // Delete incomplete backup
                        FileManager.DeleteFile(destinationFile);

                    return false;
                }

                RL.Logger?.LogInformationSource($"Backup complete");

                return true;
            }
            catch (Exception ex)
            {
                ex.HandleError("Performing compressed backup");

                // Check if a temp backup exists
                if (tempFile.TempPath.FileExists)
                    // Restore temp backup
                    FileManager.MoveFile(tempFile.TempPath, destinationFile, true);
                else if (hasCreatedTempBackup)
                    // Delete incomplete backup
                    FileManager.DeleteFile(destinationFile);

                RL.Logger?.LogInformationSource($"Compressed backup failed - clean up succeeded");

                throw;
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
                RL.Logger?.LogInformationSource($"A backup has been requested for {backupInformation.GameDisplayName}");

                try
                {
                    // Make sure we have write access to the backup location
                    if (!FileManager.CheckDirectoryWriteAccess(RCPServices.Data.BackupLocation + AppViewModel.BackupFamily))
                    {
                        RL.Logger?.LogInformationSource($"Backup failed - backup location lacks write access");

                        // Request to restart as admin
                        await RCPServices.App.RequestRestartAsAdminAsync();

                        return false;
                    }

                    // Get the backup information and group items by ID
                    var backupInfoByID = backupInformation.BackupDirectories.GroupBy(x => x.ID).ToList();

                    RL.Logger?.LogDebugSource($"{backupInfoByID.Count} backup directory ID groups were found");

                    // Get the backup info
                    var backupInfo = new List<BackupDir>();

                    // Get the latest save info from each group
                    foreach (var group in backupInfoByID)
                    {
                        if (group.Count() == 1)
                        {
                            backupInfo.Add(group.First());
                            continue;
                        }

                        RL.Logger?.LogDebugSource($"ID {group.Key} has multiple items");

                        // Find which group is the latest one
                        var groupItems = new Dictionary<BackupDir, DateTime>();

                        foreach (BackupDir item in group)
                        {
                            // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
                            if (!item.DirPath.DirectoryExists)
                                groupItems.Add(item, DateTime.MinValue);
                            else
                                groupItems.Add(item, Directory.GetFiles(item.DirPath, item.SearchPattern, item.SearchOption).Select(x => new FileInfo(x).LastWriteTime).OrderByDescending(x => x).FirstOrDefault());
                        }

                        // Get the latest directory
                        var latestDir = groupItems.OrderByDescending(x => x.Value).First().Key;

                        // Add the latest directory
                        backupInfo.Add(latestDir);

                        RL.Logger?.LogDebugSource($"The most recent backup directory was found under {latestDir.DirPath}");
                    }

                    // Make sure all the directories to back up exist
                    if (!backupInfo.Select(x => x.DirPath).DirectoriesExist())
                    {
                        RL.Logger?.LogInformationSource($"Backup failed - the input directories could not be found");

                        await Services.MessageUI.DisplayMessageAsync(String.Format(Resources.Backup_MissingDirectoriesError, backupInformation.GameDisplayName), Resources.Backup_FailedHeader, MessageType.Error);

                        return false;
                    }

                    // Check if the backup should be compressed
                    bool compress = RCPServices.Data.CompressBackups;

                    RL.Logger?.LogDebugSource(compress ? $"The backup will be compressed" : $"The backup will not be compressed");

                    // Perform the backup and keep track if it succeeded
                    bool success = await (compress ? 
                        PerformCompressedBackupAsync(backupInfo, backupInformation.CompressedBackupLocation, backupInformation.GameDisplayName) : 
                        PerformBackupAsync(backupInfo, backupInformation.BackupLocation, backupInformation.GameDisplayName));

                    if (!success)
                        return false;
                    
                    // Remove old backups for the game
                    try
                    {
                        var newBackup = compress ? backupInformation.CompressedBackupLocation : backupInformation.BackupLocation;

                        foreach (RCPBackup existingBackup in backupInformation.ExistingBackups)
                        {
                            // Ignore the newly created backup
                            if (existingBackup.Path.CorrectPathCasing().Equals(newBackup.CorrectPathCasing()))
                                continue;

                            if (existingBackup.IsCompressed)
                            {
                                // Delete the file
                                FileManager.DeleteFile(existingBackup.Path);

                                RL.Logger?.LogInformationSource("Compressed leftover backup was deleted");
                            }
                            else
                            {
                                // Delete the directory
                                FileManager.DeleteDirectory(existingBackup.Path);

                                RL.Logger?.LogInformationSource("Non-compressed leftover backup was deleted");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        ex.HandleError("Deleting leftover backups");
                    }

                    return true;
                }
                catch (Exception ex)
                {   
                    // Handle error
                    ex.HandleError("Backing up game", backupInformation);

                    // Display message to user
                    await Services.MessageUI.DisplayExceptionMessageAsync(ex, String.Format(Resources.Backup_Failed, backupInformation.GameDisplayName), Resources.Backup_FailedHeader);

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
                RL.Logger?.LogInformationSource($"A backup restore has been requested for {backupInformation.GameDisplayName}");

                try
                {
                    // Get the backup directory
                    var existingBackup = backupInformation.ExistingBackups.FirstOrDefault();

                    // Make sure a backup exists
                    if (existingBackup == null)
                    {
                        RL.Logger?.LogInformationSource($"Restore failed - the input location could not be found");

                        await Services.MessageUI.DisplayMessageAsync(String.Format(Resources.Restore_MissingBackup, backupInformation.GameDisplayName), Resources.Restore_FailedHeader, MessageType.Error);

                        return false;
                    }

                    // Get the backup information
                    var backupInfo = backupInformation.RestoreDirectories;

                    // Make sure we have write access to the restore destinations
                    if (backupInfo.Any(x => !FileManager.CheckDirectoryWriteAccess(x.DirPath)))
                    {
                        RL.Logger?.LogInformationSource($"Restore failed - one or more restore destinations lack write access");

                        // Request to restart as admin
                        await RCPServices.App.RequestRestartAsAdminAsync();

                        return false;
                    }

                    using (var tempDir = new TempDirectory(true))
                    {
                        using var archiveTempDir = new TempDirectory(true);
                        bool hasCreatedTempBackup = false;

                        try
                        {
                            // If the backup is an archive, extract it to temp
                            if (existingBackup.IsCompressed)
                            {
                                using var file = File.OpenRead(existingBackup.Path);
                                using var zip = new ZipArchive(file, ZipArchiveMode.Read);
                                zip.ExtractToDirectory(archiveTempDir.TempPath);
                            }

                            // Move existing files to temp in case the restore fails    
                            foreach (BackupDir item in backupInfo)
                            {
                                // Make sure the directory exists
                                if (!item.DirPath.DirectoryExists)
                                    continue;

                                // Get the destination directory
                                var destDir = tempDir.TempPath + item.ID;

                                // Check if the entire directory should be moved
                                if (item.IsEntireDir())
                                    // Move the directory
                                    FileManager.MoveDirectory(item.DirPath, destDir, true, true);
                                else
                                    FileManager.MoveFiles(item, destDir, true);
                            }

                            hasCreatedTempBackup = true;

                            // Restore each backup directory
                            foreach (BackupDir item in backupInfo)
                            {
                                // Get the combined directory path
                                var dirPath = (existingBackup.IsCompressed ? archiveTempDir.TempPath : existingBackup.Path) + item.ID;

                                // Restore the backup
                                if (dirPath.DirectoryExists)
                                    FileManager.CopyDirectory(dirPath, item.DirPath, false, true);
                            }
                        }
                        catch
                        {
                            // Delete restored files if restore began
                            if (hasCreatedTempBackup)
                            {
                                foreach (BackupDir item in backupInfo)
                                {
                                    // Make sure the directory exists
                                    if (!item.DirPath.DirectoryExists)
                                        continue;

                                    // Check if the entire directory should be deleted
                                    if (item.IsEntireDir())
                                    {
                                        // Delete the directory
                                        FileManager.DeleteDirectory(item.DirPath);
                                    }
                                    else
                                    {
                                        // Delete each file
                                        foreach (FileSystemPath file in Directory.GetFiles(item.DirPath, item.SearchPattern, item.SearchOption))
                                            // Delete the file
                                            FileManager.DeleteFile(file);
                                    }
                                }
                            }

                            // Restore temp backup
                            foreach (BackupDir item in backupInfo)
                            {
                                // Get the combined directory path
                                var dirPath = tempDir.TempPath + item.ID;

                                // Make sure there is a directory to restore
                                if (!dirPath.DirectoryExists)
                                    continue;

                                // Restore
                                FileManager.MoveDirectory(dirPath, item.DirPath, false, false);
                            }

                            RL.Logger?.LogInformationSource($"Restore failed - clean up succeeded");

                            throw;
                        }
                    }

                    RL.Logger?.LogInformationSource($"Restore complete");

                    return true;
                }
                catch (Exception ex)
                {
                    ex.HandleError("Restoring game", backupInformation);
                    await Services.MessageUI.DisplayExceptionMessageAsync(ex, String.Format(Resources.Restore_Failed, backupInformation.GameDisplayName), Resources.Restore_FailedHeader);

                    return false;
                }
            }
        }

        #endregion
    }
}