using System.IO;
using BinarySerializer.Ray1;

namespace RayCarrot.RCP.Metro;

public class GameProgressionManager_RaymanDesigner_MsDos : GameProgressionManager
{
    public GameProgressionManager_RaymanDesigner_MsDos(GameInstallation gameInstallation, string progressionId) 
        : base(gameInstallation, progressionId) { }

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    protected virtual int LevelsCount => 24;
    protected virtual Ray1EngineVersion EngineVersion => Ray1EngineVersion.PC_Kit;

    public override GameBackups_Directory[] BackupDirectories => new GameBackups_Directory[]
    {
        new(GameInstallation.InstallLocation.Directory, SearchOption.TopDirectoryOnly, "*.cfg", "0", 0),
        new(GameInstallation.InstallLocation.Directory + "PCMAP", SearchOption.TopDirectoryOnly, "*.sct", "1", 0),
        //
        // Note:
        // This will backup the pre-installed maps and the world files as well. This is due to how the backup manager works.
        // In the future I might make a separate manager for the maps again, in which case the search pattern "MAPS???" should get the
        // correct mapper directories within each world directory
        //
        new(GameInstallation.InstallLocation.Directory + "CAKE", SearchOption.AllDirectories, "*", "Mapper0", 0),
        new(GameInstallation.InstallLocation.Directory + "CAVE", SearchOption.AllDirectories, "*", "Mapper1", 0),
        new(GameInstallation.InstallLocation.Directory + "IMAGE", SearchOption.AllDirectories, "*", "Mapper2", 0),
        new(GameInstallation.InstallLocation.Directory + "JUNGLE", SearchOption.AllDirectories, "*", "Mapper3", 0),
        new(GameInstallation.InstallLocation.Directory + "MOUNTAIN", SearchOption.AllDirectories, "*", "Mapper4", 0),
        new(GameInstallation.InstallLocation.Directory + "MUSIC", SearchOption.AllDirectories, "*", "Mapper5", 0),
    };

    public override async IAsyncEnumerable<GameProgressionSlot> LoadSlotsAsync(FileSystemWrapper fileSystem)
    {
        FileSystemPath saveDir = InstallDir + "PCMAP";

        string[] shortWorldNames = { "", "JUN", "MUS", "MON", "IMA", "CAV", "CAK" };
        string[] longWorldNames = { "", "Jungle", "Music", "Mountain", "Image", "Cave", "Cake" };

        List<GameProgressionDataItem> progressItems = new();
        Dictionary<string, int> levelTimes = new();

        IOSearchPattern? dir = fileSystem.GetDirectory(new IOSearchPattern(saveDir, SearchOption.TopDirectoryOnly, "*.sct"));

        if (dir == null)
            yield break;

        Logger.Info("{0} saves from {1} are being loaded...", GameInstallation.FullId, saveDir.Name);

        using RCPContext context = new(dir.DirPath);
        context.AddSettings(new Ray1Settings(EngineVersion));

        // Find every .sct file
        foreach (var save in dir.GetFiles().Select(sct =>
        {
            string fileName = ((FileSystemPath)sct).RemoveFileExtension().Name;

            if (fileName.Length != 5)
                return null;

            string worldStr = fileName.Substring(0, 3);
            string levStr = fileName.Substring(3, 2);

            int world = shortWorldNames.FindItemIndex(x => x == worldStr);
            int lev = Int32.TryParse(levStr, out int parsedLev) ? parsedLev : -1;

            if (world < 1 || lev < 1)
                return null;

            return new
            {
                FilePath = (FileSystemPath)sct,
                World = world,
                Level = lev
            };
        }).Where(x => x != null).OrderBy(x => x!.World).ThenBy(x => x!.Level))
        {
            var settings = context.GetRequiredSettings<Ray1Settings>();
            settings.World = (World)save!.World;
            settings.Level = save.Level;

            PC_LevelTime? saveData = await context.ReadFileDataAsync<PC_LevelTime>(save.FilePath.Name, removeFileWhenComplete: false);

            if (saveData == null)
            {
                Logger.Info("{0} slot was not found", GameInstallation.FullId);
                continue;
            }

            int value = saveData.Value;

            if (value == -1)
            {
                Logger.Warn("Invalid save value for {0}", save.FilePath.Name);
                continue;
            }

            levelTimes.Add(save.FilePath.Name, value);

            // Get the time
            TimeSpan time = TimeSpan.FromMilliseconds(1000d / 60 * value);

            progressItems.Add(new GameProgressionDataItem(
                isPrimaryItem: false, 
                icon: ProgressionIconAsset.R1_Flag, 
                header: $"{longWorldNames[save.World]} {save.Level}", 
                text: $"{time:mm\\:ss\\.fff}"));
        }

        int levelsFinished = progressItems.Count;

        if (levelsFinished > 0)
        {
            // Add levels completed
            progressItems.Insert(0, new GameProgressionDataItem(
                isPrimaryItem: true,
                icon: ProgressionIconAsset.R1_Flag,
                header: new ResourceLocString(nameof(Resources.Progression_LevelsCompleted)),
                value: levelsFinished,
                max: LevelsCount));

            yield return new RaymanDesignerProgressionSlotViewModel(null, 0, levelsFinished, LevelsCount, progressItems, levelTimes, saveDir);
        }

        Logger.Info("{0} slot has been loaded", GameInstallation.FullId);
    }

    private class RaymanDesignerProgressionSlotViewModel : GameProgressionSlot
    {
        public RaymanDesignerProgressionSlotViewModel(
            LocalizedString? name, 
            int index, 
            int collectiblesCount, 
            int totalCollectiblesCount, 
            IReadOnlyList<GameProgressionDataItem> dataItems, 
            Dictionary<string, int> levelTimes, 
            FileSystemPath saveDir) 
            : base(name, index, collectiblesCount, totalCollectiblesCount, FileSystemPath.EmptyPath, dataItems)
        {
            LevelTimes = levelTimes;
            SaveDir = saveDir;
        }

        public override bool CanExport => true;
        public override bool CanImport => false;

        private Dictionary<string, int> LevelTimes { get; }
        private FileSystemPath SaveDir { get; }

        public override void ExportSlot(FileSystemPath filePath)
        {
            JsonHelpers.SerializeToFile(LevelTimes, filePath);
        }

        public override void ImportSlot(FileSystemPath filePath)
        {
            // Read the JSON file
            Dictionary<string, int> lvlTimes = JsonHelpers.DeserializeFromFile<Dictionary<string, int>>(filePath);

            // TODO: Implement - encode level time back to encrypted format and write to files
            throw new NotImplementedException();
        }
    }
}