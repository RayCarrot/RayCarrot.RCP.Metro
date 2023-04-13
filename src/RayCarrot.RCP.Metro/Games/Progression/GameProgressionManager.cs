using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.IO.Compression;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// Manages game progression for a game installation
/// </summary>
public abstract class GameProgressionManager
{
    #region Constructor

    protected GameProgressionManager(GameInstallation gameInstallation, string backupId)
    {
        GameInstallation = gameInstallation;
        BackupId = backupId;
    }

    #endregion

    #region Protected Properties

    protected FileSystemPath InstallDir => GameInstallation.InstallLocation.Directory;

    #endregion

    #region Public Properties

    public GameInstallation GameInstallation { get; }
    public abstract GameBackups_Directory[]? BackupDirectories { get; }
    public string BackupId { get; } // TODO-14: Rename to ProgressionId

    [MemberNotNullWhen(true, nameof(BackupDirectories))]
    public bool SupportsBackups => BackupDirectories != null;
    
    /// <summary>
    /// An optional name for this progression manager. Should mainly be used when games have multiple
    /// progression managers so that they can be differentiated.
    /// </summary>
    public virtual string? Name => null;

    #endregion

    #region Public Static Methods

    public static GameProgressionSlot? CreatePrimarySlot(IEnumerable<GameProgressionSlot> gameSlots)
    {
        // Get the slot with the highest percentage from each group
        GameProgressionSlot[] slots = gameSlots.
            GroupBy(x => x.SlotGroup).
            Select(g => g.OrderBy(x => x.Percentage).LastOrDefault()).
            ToArray();

        if (!slots.Any())
            return null;

        double totalPercentage = 0;
        List<GameProgressionDataItem> dataItems = new();

        foreach (GameProgressionSlot slot in slots)
        {
            totalPercentage += slot.Percentage / slots.Length;
            dataItems.AddRange(slot.DataItems);
        }

        return new GameProgressionSlot(null, -1, totalPercentage, FileSystemPath.EmptyPath, dataItems);
    }

    #endregion

    #region Public Methods

    public virtual IAsyncEnumerable<GameProgressionSlot> LoadSlotsAsync(FileSystemWrapper fileSystem) => 
        AsyncEnumerable.Empty<GameProgressionSlot>();

    #endregion

    #region Classes

    public abstract class FileSystemWrapper
    {
        public virtual Task InitAsync() => Task.CompletedTask;

        public abstract FileSystemPath GetFile(FileSystemPath filePath);
        public abstract IOSearchPattern? GetDirectory(IOSearchPattern searchPattern);
    }

    public class PhysicalFileSystemWrapper : FileSystemWrapper
    {
        public PhysicalFileSystemWrapper(ProgramDataSource dataSource)
        {
            DataSource = dataSource;
        }

        public ProgramDataSource DataSource { get; }

        public override FileSystemPath GetFile(FileSystemPath filePath)
        {
            string fileName = filePath.Name;
            ProgressionDirectory dir = new(filePath.Parent, SearchOption.TopDirectoryOnly, fileName);
            return Path.Combine(dir.GetReadSearchPattern(DataSource).DirPath, fileName);
        }

        public override IOSearchPattern? GetDirectory(IOSearchPattern searchPattern)
        {
            // Convert the search pattern using the current data source
            searchPattern = new ProgressionDirectory(searchPattern).
                GetReadSearchPattern(DataSource);

            return searchPattern.DirPath.DirectoryExists
                ? searchPattern
                : null;
        }
    }

    public class BackupFileSystemWrapper : FileSystemWrapper, IDisposable
    {
        public BackupFileSystemWrapper(GameBackups_BackupInfo backupInfo)
        {
            Backup = backupInfo.GetPrimaryBackup;
            RestoreDirectories = backupInfo.RestoreDirectories ?? throw new Exception($"Restore directories must be set");

            if (Backup == null)
                return;

            if (Backup.IsCompressed)
            {
                TempExtractDir = new TempDirectory(true);
                BackupDir = TempExtractDir.TempPath;
            }
            else
            {
                BackupDir = Backup.Path;
            }
        }

        public GameBackups_ExistingBackup? Backup { get; }
        public BackupSearchPattern[] RestoreDirectories { get; }
        public FileSystemPath BackupDir { get; }
        public TempDirectory? TempExtractDir { get; }

        public override async Task InitAsync()
        {
            if (Backup is null || TempExtractDir is null)
                return;

            await Task.Run(() =>
            {
                using ZipArchive zip = new(File.OpenRead(Backup.Path));
                zip.ExtractToDirectory(TempExtractDir.TempPath);
            });
        }

        public override FileSystemPath GetFile(FileSystemPath filePath)
        {
            if (Backup == null)
                return FileSystemPath.EmptyPath;

            foreach (BackupSearchPattern dir in RestoreDirectories)
            {
                if (!filePath.FullPath.StartsWith(dir.SearchPattern.DirPath))
                    continue;

                FileSystemPath backupPath = BackupDir + dir.ID + (filePath - dir.SearchPattern.DirPath);

                if (backupPath.FileExists)
                    return backupPath;
            }

            return FileSystemPath.EmptyPath;
        }

        public override IOSearchPattern? GetDirectory(IOSearchPattern searchPattern)
        {
            if (Backup == null)
                return null;

            FileSystemPath backupDir = FileSystemPath.EmptyPath;

            foreach (BackupSearchPattern dir in RestoreDirectories)
            {
                if (!searchPattern.DirPath.ContainsPath(dir.SearchPattern.DirPath))
                    continue;

                FileSystemPath backupPath = BackupDir + dir.ID + (searchPattern.DirPath - dir.SearchPattern.DirPath);

                if (backupPath.DirectoryExists)
                {
                    backupDir = backupPath;
                    break;
                }
            }

            if (!backupDir.DirectoryExists)
                return null;

            return new IOSearchPattern(backupDir, searchPattern.SearchOption, searchPattern.SearchPattern);
        }

        public void Dispose()
        {
            TempExtractDir?.Dispose();
        }
    }

    #endregion
}