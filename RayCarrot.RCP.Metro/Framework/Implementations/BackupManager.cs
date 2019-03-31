using RayCarrot.CarrotFramework;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Nito.AsyncEx;

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

        #region Public Methods

        /// <summary>
        /// Performs a backup on the game
        /// </summary>
        /// <param name="game">The game to perform the backup on</param>
        public async Task BackupAsync(Games game)
        {
            using (await AsyncLock.LockAsync())
            {
                RCF.Logger.LogInformationSource($"A backup has been requested for {game}");

                try
                {
                    // Get the destination directory
                    FileSystemPath destinationDir = game.GetBackupDir();

                    // Confirm backup if one already exists
                    if (destinationDir.DirectoryExists && !await RCF.MessageUI.DisplayMessageAsync(String.Format(Resources.Backup_Confirm, game.GetDisplayName()), Resources.Backup_ConfirmHeader, MessageType.Warning, true))
                    {
                        RCF.Logger.LogInformationSource($"Backup canceled");

                        return;
                    }

                    // Get the backup information
                    var backupInfo = game.GetBackupInfo();

                    // Check if the directories to back up exist
                    if (!backupInfo.Select(x => x.DirPath).DirectoriesExist())
                    {
                        RCF.Logger.LogInformationSource($"Backup failed - the input directories could not be found");

                        await RCF.MessageUI.DisplayMessageAsync(String.Format(Resources.Backup_MissingDirectoriesError, game.GetDisplayName()), Resources.Backup_FailedHeader, MessageType.Error);
                        return;
                    }

                    // Get the temp path
                    var tempPath = CommonPaths.TempPath + game.GetBackupName();

                    // Delete temp backup
                    RCFRCP.File.DeleteDirectory(tempPath);

                    // Create temp path
                    Directory.CreateDirectory(CommonPaths.TempPath);

                    // Check if a backup already exists
                    if (destinationDir.DirectoryExists)
                        // Create a new temp backup
                        Directory.Move(destinationDir, tempPath);

                    try
                    {
                        // Enumerate the backup information
                        foreach (var item in backupInfo)
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

                                    // Check if the directory does not exist
                                    if (!destFile.Parent.DirectoryExists)
                                        // Create the directory
                                        Directory.CreateDirectory(destFile.Parent);

                                    // Copy the file
                                    File.Copy(file, destFile);
                                }
                            }
                        }

                        // Check if any files were backed up
                        if (!destinationDir.DirectoryExists)
                        {
                            await RCF.MessageUI.DisplayMessageAsync(String.Format(Resources.Backup_MissingFilesError, game.GetDisplayName()), Resources.Backup_FailedHeader, MessageType.Error);

                            // Check if a temp backup exists
                            if (tempPath.DirectoryExists)
                                // Restore temp backup
                                RCFRCP.File.MoveDirectory(tempPath, destinationDir, true);

                            return;
                        }

                        // Delete temp backup
                        RCFRCP.File.DeleteDirectory(tempPath);

                        RCF.Logger.LogInformationSource($"Backup complete");

                        await RCF.MessageUI.DisplaySuccessfulActionMessageAsync(String.Format(Resources.Backup_Success, game.GetDisplayName()), Resources.Backup_SuccessHeader);
                    }
                    catch
                    {
                        // Check if a temp backup exists
                        if (tempPath.DirectoryExists)
                            // Restore temp backup
                            RCFRCP.File.MoveDirectory(tempPath, destinationDir, true);

                        RCF.Logger.LogInformationSource($"Backup failed - clean up succeeded");

                        throw;
                    }
                }
                catch (Exception ex)
                {
                    ex.HandleCritical("Backing up game", game);
                    await RCF.MessageUI.DisplayMessageAsync(String.Format(Resources.Backup_Failed, game.GetDisplayName()), Resources.Backup_FailedHeader, MessageType.Error);
                }
            }
        }

        /// <summary>
        /// Restores a backup on the game
        /// </summary>
        /// <param name="game">The game to restore the backup on</param>
        public async Task RestoreAsync(Games game)
        {
            using (await AsyncLock.LockAsync())
            {
                RCF.Logger.LogInformationSource($"A backup restore has been requested for {game}");

                try
                {
                    // Get the backup directory
                    FileSystemPath backupDir = game.GetBackupDir();

                    // Make sure a backup exists
                    if (!backupDir.DirectoryExists)
                    {
                        RCF.Logger.LogInformationSource($"Restore failed - the input directory could not be found");

                        await RCF.MessageUI.DisplayMessageAsync(String.Format(Resources.Restore_MissingBackup, game.GetDisplayName()), Resources.Restore_FailedHeader, MessageType.Error);
                        return;
                    }

                    // Confirm restore
                    if (!await RCF.MessageUI.DisplayMessageAsync(String.Format(Resources.Restore_Confirm, game.GetDisplayName()), Resources.Restore_ConfirmHeader, MessageType.Warning, true))
                    {
                        RCF.Logger.LogInformationSource($"Restore canceled");
                        return;
                    }

                    // Get the backup information
                    var backupInfo = game.GetBackupInfo();

                    // Get the temp path
                    var tempPath = CommonPaths.TempPath + game.GetBackupName();

                    // Delete the temp backup
                    RCFRCP.File.DeleteDirectory(tempPath);

                    // Create the temp path
                    Directory.CreateDirectory(tempPath);

                    try
                    {
                        // Enumerate the backup information
                        foreach (var item in backupInfo)
                        {
                            // Move existing files if the directory exists to temp
                            if (item.DirPath.DirectoryExists)
                            {
                                // Check if the entire directory should be moved
                                if (item.IsEntireDir())
                                {
                                    // Get the destination directory
                                    var destDir = tempPath + item.ID + item.DirPath.Name;

                                    // Move the directory
                                    RCFRCP.File.MoveDirectory(item.DirPath, destDir, true);
                                }
                                else
                                {
                                    // Move each file
                                    foreach (FileSystemPath file in Directory.GetFiles(item.DirPath, item.ExtensionFilter, item.SearchOption))
                                    {
                                        // Get the destination file
                                        var destFile = tempPath + item.ID + file.GetRelativePath(item.DirPath);

                                        // Move the file
                                        RCFRCP.File.MoveFile(file, destFile, true);
                                    }
                                }
                            }

                            // Get the combined directory path
                            var dirPath = backupDir + item.ID;

                            // Restore the backup
                            if (dirPath.DirectoryExists)
                                RCFRCP.File.CopyDirectory(dirPath, item.DirPath, false, true);
                        }
                    }
                    catch
                    {
                        // Restore temp backup
                        foreach (var item in backupInfo)
                        {
                            // Get the combined directory path
                            var dirPath = tempPath + item.ID;

                            // Make sure there is a directory to restore
                            if (!dirPath.DirectoryExists)
                                continue;

                            // Check if the entire directory should be moved
                            if (item.IsEntireDir())
                            {
                                // Get the temp directory
                                var tempDir = dirPath + item.DirPath.Name;

                                // Get the destination directory
                                var destDir = item.DirPath;

                                // Move the directory
                                RCFRCP.File.MoveDirectory(tempDir, destDir, true);
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

                        RCF.Logger.LogInformationSource($"Restore failed - clean up succeeded");

                        throw;
                    }

                    // Delete temp backup
                    RCFRCP.File.DeleteDirectory(tempPath);

                    RCF.Logger.LogInformationSource($"Restore complete");

                    await RCF.MessageUI.DisplaySuccessfulActionMessageAsync(String.Format(Resources.Restore_Success, game.GetDisplayName()), Resources.Restore_SuccessHeader);
                }
                catch (Exception ex)
                {
                    ex.HandleCritical("Restoring game", game);
                    await RCF.MessageUI.DisplayMessageAsync(String.Format(Resources.Restore_Failed, game.GetDisplayName()), Resources.Restore_FailedHeader, MessageType.Error);
                }
            }
        }

        #endregion
    }
}