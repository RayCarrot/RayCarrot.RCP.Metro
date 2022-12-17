using System.IO;

namespace RayCarrot.RCP.Metro;

public class GameBackups_BackupInfo
{
    #region Constructor

    /// <summary>
    /// Default constructor
    /// </summary>
    /// <param name="backupName">The backup name to use to get the paths</param>
    /// <param name="backupDirectories">The backup directory infos</param>
    public GameBackups_BackupInfo(string backupName, IEnumerable<GameBackups_Directory> backupDirectories)
    {
        BackupName = backupName;
        AllBackupDirectories = backupDirectories.GroupBy(x => x.BackupVersion).ToDictionary(x => x.Key, x => x.ToArray());

        // Get the latest backup version to create a backup from
        LatestAvailableBackupVersion = AllBackupDirectories.Select(x => x.Value.Select(y => y.BackupVersion)).SelectMany(x => x).Max();

        BackupLocation = Services.Data.Backup_BackupLocation + GameBackups_Manager.BackupFamily + (BackupName + $"-{LatestAvailableBackupVersion.ToString().PadLeft(2, '0')}");
        CompressedBackupLocation = BackupLocation.FullPath + AppFilePaths.BackupCompressionExtension;
    }

    #endregion

    #region Logger

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    #endregion

    #region Private Properties

    /// <summary>
    /// The backup name to use to get the paths
    /// </summary>
    private string BackupName { get; }

    /// <summary>
    /// All backup directories
    /// </summary>
    private Dictionary<int, GameBackups_Directory[]> AllBackupDirectories { get; }

    #endregion

    #region Public Properties

    /// <summary>
    /// The path for the compressed backup
    /// </summary>
    public FileSystemPath CompressedBackupLocation { get; }

    /// <summary>
    /// The path for the backup
    /// </summary>
    public FileSystemPath BackupLocation { get; }

    /// <summary>
    /// The existing backups for this game, ordered by priority.
    /// The first backup in the collection is always the one to restore.
    /// </summary>
    public GameBackups_ExistingBackup[]? ExistingBackups { get; set; }

    /// <summary>
    /// Gets the primary backup to be used when restoring
    /// </summary>
    public GameBackups_ExistingBackup? GetPrimaryBackup => ExistingBackups?.FirstOrDefault();

    /// <summary>
    /// The backup directories to use when performing a backup
    /// </summary>
    public BackupSearchPattern[]? BackupDirectories { get; protected set; }

    /// <summary>
    /// The backup directories to use when performing a restore
    /// </summary>
    public BackupSearchPattern[]? RestoreDirectories { get; protected set; }

    /// <summary>
    /// The latest available backup version
    /// </summary>
    public int LatestAvailableBackupVersion { get; }

    /// <summary>
    /// The latest available backup version to restore
    /// </summary>
    public int LatestAvailableRestoreVersion { get; set; }

    public bool HasVirtualStoreVersion => AllBackupDirectories.SelectMany(x => x.Value).Any(x => x.HasVirtualStoreVersion);

    #endregion

    #region Private Methods

    /// <summary>
    /// Get all existing backups for this info, ordered by priority
    /// </summary>
    /// <returns>The existing backups</returns>
    private GameBackups_ExistingBackup[] GetExistingBackups()
    {
        FileSystemPath path = Services.Data.Backup_BackupLocation + GameBackups_Manager.BackupFamily;

        if (!path.DirectoryExists)
            return Array.Empty<GameBackups_ExistingBackup>();

        return Directory.GetFileSystemEntries(path, $"{BackupName}*", SearchOption.TopDirectoryOnly).
            Select(x => new GameBackups_ExistingBackup(x)).
            OrderByDescending(x => x.BackupVersion).
            ThenBy(x => x.IsCompressed == Services.Data.Backup_CompressBackups ? 0 : 1).
            ToArray();
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Refreshes the backup info
    /// </summary>
    /// <param name="dataSource">The backup program data source</param>
    /// <param name="displayName">The game's display name</param>
    /// <returns>The task</returns>
    public async Task RefreshAsync(ProgramDataSource dataSource, string displayName)
    {
        try
        {
            ExistingBackups = GetExistingBackups();
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Getting existing backups");
            await Services.MessageUI.DisplayExceptionMessageAsync(ex, String.Format(Resources.GetExistingBackupsError, displayName));

            ExistingBackups = Array.Empty<GameBackups_ExistingBackup>();
        }

        // Get the latest backup version to restore from
        LatestAvailableRestoreVersion = GetPrimaryBackup?.BackupVersion ?? -1;

        BackupDirectories = AllBackupDirectories.TryGetValue(LatestAvailableBackupVersion)?.
            Select(x => x.GetBackupReadSearchPattern(dataSource)).
            ToArray() ?? Array.Empty<BackupSearchPattern>();
        
        RestoreDirectories = LatestAvailableRestoreVersion == -1 
            ? Array.Empty<BackupSearchPattern>() 
            : AllBackupDirectories.TryGetValue(LatestAvailableRestoreVersion)?.
                SelectMany(x => x.GetBackupWriteSearchPatterns(dataSource)).
                ToArray() ?? Array.Empty<BackupSearchPattern>();

        if (BackupDirectories.GroupBy(x => x.ID).Any(x => x.Count() > 1))
            throw new InvalidOperationException("Multiple backup directories can not use the same ID starting from version 13.0.0");
    }

    #endregion
}