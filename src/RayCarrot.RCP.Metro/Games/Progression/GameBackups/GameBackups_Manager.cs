﻿using System.IO;
using System.IO.Compression;
using Nito.AsyncEx;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// The backup manager
/// </summary>
public class GameBackups_Manager
{
    #region Static Constructor

    static GameBackups_Manager()
    {
        AsyncLock = new AsyncLock();
    }

    #endregion

    #region Logger

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    #endregion

    #region Constructor

    public GameBackups_Manager(FileManager fileManager, IMessageUIManager messageUiManager, AppViewModel app, AppUserData data)
    {
        File = fileManager ?? throw new ArgumentNullException(nameof(fileManager));
        Message = messageUiManager ?? throw new ArgumentNullException(nameof(messageUiManager));
        App = app ?? throw new ArgumentNullException(nameof(app));
        Data = data ?? throw new ArgumentNullException(nameof(data));
    }

    #endregion

    #region Public Constant Fields

    /// <summary>
    /// The name of the backup directory for this application
    /// </summary>
    public const string BackupFamily = "Rayman Game Backups";

    #endregion

    #region Private Static Properties

    /// <summary>
    /// The async lock for backup and restore operations
    /// </summary>
    private static AsyncLock AsyncLock { get; }

    #endregion

    #region Private Properties

    private FileManager File { get; }
    private IMessageUIManager Message { get; }
    private AppViewModel App { get; }
    private AppUserData Data { get; }

    #endregion

    #region Public Properties

    public FileSystemPath BackupDirectory => Data.Backup_BackupLocation + BackupFamily;

    #endregion

    #region Backup

    /// <summary>
    /// Performs a backup on the game
    /// </summary>
    /// <param name="backupDirs">The backup directories</param>
    /// <param name="destinationDir">The destination directory path</param>
    /// <param name="gameDisplayName">The game display name</param>
    /// <returns>True if the backup was successful</returns>
    private async Task<bool> PerformBackupAsync(IEnumerable<BackupSearchPattern> backupDirs, FileSystemPath destinationDir, string gameDisplayName)
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
                File.MoveDirectory(destinationDir, tempDir.TempPath, true, true);
            }

            hasCreatedTempBackup = true;

            // Backup each directory
            foreach (BackupSearchPattern item in backupDirs)
            {
                var itemDestination = destinationDir + item.ID;

                // Check if the entire directory should be copied
                if (item.SearchPattern.IsEntireDir())
                    // Copy the directory   
                    File.CopyDirectory(item.SearchPattern.DirPath, itemDestination, true, true);
                else
                    // Backup the files
                    File.CopyFiles(item.SearchPattern, itemDestination, true);
            }

            // Check if any files were backed up
            if (!destinationDir.DirectoryExists)
            {
                await Message.DisplayMessageAsync(String.Format(Resources.Backup_MissingFilesError, gameDisplayName), Resources.Backup_FailedHeader, MessageType.Error);

                if (tempDir.TempPath.DirectoryExists)
                    // Restore temp backup
                    File.MoveDirectory(tempDir.TempPath, destinationDir, true, true);

                return false;
            }

            Logger.Info("Backup complete");

            return true;
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Performing backup");

            // Restore temp backup
            if (tempDir.TempPath.DirectoryExists)
                // Only replace files if the crash happened after creating the temp backup and during the backup operation
                File.MoveDirectory(tempDir.TempPath, destinationDir, hasCreatedTempBackup, false);
            else if (hasCreatedTempBackup)
                // Delete incomplete backup
                File.DeleteDirectory(destinationDir);

            Logger.Info("Backup failed - clean up succeeded");

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
    private async Task<bool> PerformCompressedBackupAsync(IEnumerable<BackupSearchPattern> backupDirs, FileSystemPath destinationFile, string gameDisplayName)
    {
        using var tempFile = new TempFile(false);

        bool hasCreatedTempBackup = false;

        try
        {
            // Check if a backup already exists
            if (destinationFile.FileExists)
                // Create a new temp backup
                File.MoveFile(destinationFile, tempFile.TempPath, true);

            hasCreatedTempBackup = true;

            // Create the parent directory
            Directory.CreateDirectory(destinationFile.Parent);

            // Keep track if any files have been backed up
            bool backedUp = false;

            // Create the compressed file
            using (var fileStream = System.IO.File.OpenWrite(destinationFile))
            {
                // Open the zip archive
                using var zip = new ZipArchive(fileStream, ZipArchiveMode.Create);

                // Enumerate the backup information
                foreach (var item in backupDirs)
                {
                    // Backup each file
                    foreach (FileSystemPath file in item.SearchPattern.GetFiles())
                    {
                        // Get the destination file
                        var destFile = item.ID + (file - item.SearchPattern.DirPath);

                        // Copy the file
                        zip.CreateEntryFromFile(file, destFile, CompressionLevel.Optimal);

                        backedUp = true;
                    }
                }
            }

            // Check if any files were backed up
            if (!backedUp)
            {
                await Message.DisplayMessageAsync(String.Format(Resources.Backup_MissingFilesError, gameDisplayName), Resources.Backup_FailedHeader, MessageType.Error);
                    
                // Check if a temp backup exists
                if (tempFile.TempPath.FileExists)
                    // Restore temp backup
                    File.MoveFile(tempFile.TempPath, destinationFile, true);
                else
                    // Delete incomplete backup
                    File.DeleteFile(destinationFile);

                return false;
            }

            Logger.Info("Backup complete");

            return true;
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Performing compressed backup");

            // Check if a temp backup exists
            if (tempFile.TempPath.FileExists)
                // Restore temp backup
                File.MoveFile(tempFile.TempPath, destinationFile, true);
            else if (hasCreatedTempBackup)
                // Delete incomplete backup
                File.DeleteFile(destinationFile);

            Logger.Info("Compressed backup failed - clean up succeeded");

            throw;
        }
    }

    private void RemoveOldBackups(FileSystemPath newBackup, IEnumerable<GameBackups_ExistingBackup> existingBackups)
    {
        try
        {
            foreach (GameBackups_ExistingBackup existingBackup in existingBackups)
            {
                // Ignore the newly created backup
                if (existingBackup.Path.CorrectPathCasing().Equals(newBackup.CorrectPathCasing()))
                    continue;

                if (existingBackup.IsCompressed)
                {
                    // Delete the file
                    File.DeleteFile(existingBackup.Path);

                    Logger.Info("Compressed leftover backup was deleted");
                }
                else
                {
                    // Delete the directory
                    File.DeleteDirectory(existingBackup.Path);

                    Logger.Info("Non-compressed leftover backup was deleted");
                }
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Deleting leftover backups");
        }
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Performs a backup on the game
    /// </summary>
    /// <param name="backupInformation">The backup information</param>
    /// <param name="displayName">The game's display name</param>
    /// <returns>True if the backup was successful</returns>
    public async Task<bool> BackupAsync(GameBackups_BackupInfo backupInformation, string displayName)
    {
        using (await AsyncLock.LockAsync())
        {
            Logger.Info("A backup has been requested for {0}", displayName);

            try
            {
                // Make sure we have write access to the backup location
                if (Directory.Exists(BackupDirectory) && !File.CheckDirectoryWriteAccess(BackupDirectory))
                {
                    Logger.Info("Backup failed - backup location lacks write access");

                    // Request to restart as admin
                    await App.RequestRestartAsAdminAsync();

                    return false;
                }

                if (backupInformation.BackupDirectories is null)
                    throw new InvalidOperationException("A backup can only be performed on an info object with valid directories");

                // Make sure all the directories to back up exist
                if (!backupInformation.BackupDirectories.Select(x => x.SearchPattern.DirPath).DirectoriesExist())
                {
                    Logger.Info("Backup failed - the input directories could not be found");

                    await Message.DisplayMessageAsync(String.Format(Resources.Backup_MissingDirectoriesError, displayName), Resources.Backup_FailedHeader, MessageType.Error);

                    return false;
                }

                // Check if the backup should be compressed
                bool compress = Data.Backup_CompressBackups;

                Logger.Debug(compress ? $"The backup will be compressed" : $"The backup will not be compressed");

                // Perform the backup and keep track if it succeeded
                bool success = await (compress ? 
                    PerformCompressedBackupAsync(backupInformation.BackupDirectories, backupInformation.CompressedBackupLocation, displayName) : 
                    PerformBackupAsync(backupInformation.BackupDirectories, backupInformation.BackupLocation, displayName));

                if (!success)
                    return false;

                FileSystemPath newBackup = compress
                    ? backupInformation.CompressedBackupLocation
                    : backupInformation.BackupLocation;

                // Remove old backups for the game
                RemoveOldBackups(newBackup, backupInformation.ExistingBackups!);

                return true;
            }
            catch (Exception ex)
            {   
                // Handle error
                Logger.Error(ex, "Backing up game");

                // Display message to user
                await Message.DisplayExceptionMessageAsync(ex, String.Format(Resources.Backup_Failed, displayName), Resources.Backup_FailedHeader);

                // Return that backup did not succeed
                return false;
            }
        }
    }

    /// <summary>
    /// Restores a backup on the game
    /// </summary>
    /// <param name="backupInformation">The backup information</param>
    /// <param name="displayName">The game's display name</param>
    /// <returns>True if the backup was successful</returns>
    public async Task<bool> RestoreAsync(GameBackups_BackupInfo backupInformation, string displayName)
    {
        using (await AsyncLock.LockAsync())
        {
            Logger.Info("A backup restore has been requested for {0}", displayName);

            try
            {
                // Get the backup directory
                GameBackups_ExistingBackup? existingBackup = backupInformation.GetPrimaryBackup;

                // Make sure a backup exists
                if (existingBackup == null)
                {
                    Logger.Info("Restore failed - the input location could not be found");

                    await Message.DisplayMessageAsync(String.Format(Resources.Restore_MissingBackup, displayName), Resources.Restore_FailedHeader, MessageType.Error);

                    return false;
                }

                if (backupInformation.RestoreDirectories is null)
                    throw new InvalidOperationException("A restore can only be performed on an info object with valid directories");

                // Make sure we have write access to the restore destinations
                if (backupInformation.RestoreDirectories.Any(x => Directory.Exists(x.SearchPattern.DirPath) && !File.CheckDirectoryWriteAccess(x.SearchPattern.DirPath)))
                {
                    Logger.Info("Restore failed - one or more restore destinations lack write access");

                    // Request to restart as admin
                    await App.RequestRestartAsAdminAsync();

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
                            using var file = System.IO.File.OpenRead(existingBackup.Path);
                            using var zip = new ZipArchive(file, ZipArchiveMode.Read);
                            zip.ExtractToDirectory(archiveTempDir.TempPath);
                        }

                        // Move existing files to temp in case the restore fails    
                        foreach (BackupSearchPattern item in backupInformation.RestoreDirectories)
                        {
                            // Make sure the directory exists
                            if (!item.SearchPattern.DirPath.DirectoryExists)
                                continue;

                            // Get the destination directory
                            var destDir = tempDir.TempPath + item.ID;

                            // Check if the entire directory should be moved
                            if (item.SearchPattern.IsEntireDir())
                                // Move the directory
                                File.MoveDirectory(item.SearchPattern.DirPath, destDir, true, true);
                            else
                                File.MoveFiles(item.SearchPattern, destDir, true);
                        }

                        hasCreatedTempBackup = true;

                        // Restore each backup directory
                        foreach (BackupSearchPattern item in backupInformation.RestoreDirectories)
                        {
                            // Get the combined directory path
                            var dirPath = (existingBackup.IsCompressed ? archiveTempDir.TempPath : existingBackup.Path) + item.ID;

                            // Restore the backup
                            if (dirPath.DirectoryExists)
                                File.CopyDirectory(dirPath, item.SearchPattern.DirPath, false, true);
                        }
                    }
                    catch
                    {
                        // Delete restored files if restore began
                        if (hasCreatedTempBackup)
                        {
                            foreach (BackupSearchPattern item in backupInformation.RestoreDirectories)
                            {
                                // Make sure the directory exists
                                if (!item.SearchPattern.DirPath.DirectoryExists)
                                    continue;

                                // Check if the entire directory should be deleted
                                if (item.SearchPattern.IsEntireDir())
                                {
                                    // Delete the directory
                                    File.DeleteDirectory(item.SearchPattern.DirPath);
                                }
                                else
                                {
                                    // Delete each file
                                    foreach (FileSystemPath file in item.SearchPattern.GetFiles())
                                        // Delete the file
                                        File.DeleteFile(file);
                                }
                            }
                        }

                        // Restore temp backup
                        foreach (BackupSearchPattern item in backupInformation.RestoreDirectories)
                        {
                            // Get the combined directory path
                            var dirPath = tempDir.TempPath + item.ID;

                            // Make sure there is a directory to restore
                            if (!dirPath.DirectoryExists)
                                continue;

                            // Restore
                            File.MoveDirectory(dirPath, item.SearchPattern.DirPath, false, false);
                        }

                        Logger.Info("Restore failed - clean up succeeded");

                        throw;
                    }
                }

                Logger.Info("Restore complete");

                return true;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Restoring game");
                await Message.DisplayExceptionMessageAsync(ex, String.Format(Resources.Restore_Failed, displayName), Resources.Restore_FailedHeader);

                return false;
            }
        }
    }

    #endregion
}