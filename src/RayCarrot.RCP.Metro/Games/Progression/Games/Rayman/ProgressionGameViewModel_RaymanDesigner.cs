using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NLog;
using RayCarrot.IO;
using RayCarrot.Rayman;
using RayCarrot.Rayman.Ray1;

namespace RayCarrot.RCP.Metro;

public class ProgressionGameViewModel_RaymanDesigner : ProgressionGameViewModel
{
    protected ProgressionGameViewModel_RaymanDesigner(Games game) : base(game) { }
    public ProgressionGameViewModel_RaymanDesigner() : base(Games.RaymanDesigner) { }

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    protected override GameBackups_Directory[] BackupDirectories => new GameBackups_Directory[]
    {
        new GameBackups_Directory(Game.GetInstallDir(), SearchOption.TopDirectoryOnly, "*.cfg", "0", 0),
        new GameBackups_Directory(Game.GetInstallDir() + "PCMAP", SearchOption.TopDirectoryOnly, "*.sct", "1", 0),
        //
        // Note:
        // This will backup the pre-installed maps and the world files as well. This is due to how the backup manager works.
        // In the future I might make a separate manager for the maps again, in which case the search pattern "MAPS???" should get the
        // correct mapper directories within each world directory
        //
        new GameBackups_Directory(Game.GetInstallDir() + "CAKE", SearchOption.AllDirectories, "*", "Mapper0", 0),
        new GameBackups_Directory(Game.GetInstallDir() + "CAVE", SearchOption.AllDirectories, "*", "Mapper1", 0),
        new GameBackups_Directory(Game.GetInstallDir() + "IMAGE", SearchOption.AllDirectories, "*", "Mapper2", 0),
        new GameBackups_Directory(Game.GetInstallDir() + "JUNGLE", SearchOption.AllDirectories, "*", "Mapper3", 0),
        new GameBackups_Directory(Game.GetInstallDir() + "MOUNTAIN", SearchOption.AllDirectories, "*", "Mapper4", 0),
        new GameBackups_Directory(Game.GetInstallDir() + "MUSIC", SearchOption.AllDirectories, "*", "Mapper5", 0),
    };

    protected override async IAsyncEnumerable<ProgressionSlotViewModel> LoadSlotsAsync(FileSystemWrapper fileSystem)
    {
        FileSystemPath saveDir = InstallDir + "PCMAP";

        Logger.Info("{0} saves from {1} is being loaded...", Game, saveDir.Name);

        string[] shortWorldNames = { "", "JUN", "MUS", "MON", "IMA", "CAV", "CAK" };
        string[] longWorldNames = { "", "Jungle", "Music", "Mountain", "Image", "Cave", "Cake" };

        List<ProgressionDataViewModel> progressItems = new();
        Dictionary<string, int> levelTimes = new();

        // Find every .sct file
        foreach (var save in fileSystem.GetFiles(saveDir, ".sct").Select(sct =>
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
            Ray1Settings settings = Ray1Settings.GetDefaultSettings(Ray1Game.RayKit, Platform.PC);
            (RaymanDesignerSaveData? saveData, _) = await SerializeFileDataAsync<RaymanDesignerSaveData>(fileSystem, save!.FilePath, settings);

            if (saveData == null)
            {
                Logger.Info("{0} slot was not found", Game);
                continue;
            }

            int value = saveData.GetDecodedValue(save.World, save.Level, Game == Games.RaymanDesigner ? RaymanDesignerSaveData.SaveRevision.KIT : RaymanDesignerSaveData.SaveRevision.FAN_60N);

            if (value == -1)
            {
                Logger.Warn("Invalid save value for {0}", save.FilePath.Name);
                continue;
            }

            levelTimes.Add(save.FilePath.Name, value);

            // Get the time
            TimeSpan time = TimeSpan.FromMilliseconds(1000d / 60 * value);

            progressItems.Add(new ProgressionDataViewModel(
                isPrimaryItem: false, 
                icon: ProgressionIcon.R1_Flag, 
                header: new ConstLocString($"{longWorldNames[save.World]} {save.Level}"), 
                text: new ConstLocString($"{time:mm\\:ss\\.fff}")));
        }

        int levelsCount = Game switch
        {
            Games.RaymanDesigner => 24,
            Games.RaymanByHisFans => 40,
            Games.Rayman60Levels => 60,
            _ => -1
        };

        int levelsFinished = progressItems.Count;

        // Add levels completed
        // TODO-UPDATE: Localize
        progressItems.Insert(0, new ProgressionDataViewModel(
            isPrimaryItem: true, 
            icon: ProgressionIcon.R1_Flag, 
            header: new ConstLocString("Levels completed"), 
            value: levelsFinished, 
            max: levelsCount));

        yield return new RaymanDesignerProgressionSlotViewModel(this, null, 0, levelsFinished, levelsCount, progressItems, levelTimes, saveDir);

        Logger.Info("{0} slot has been loaded", Game);
    }

    private class RaymanDesignerProgressionSlotViewModel : ProgressionSlotViewModel
    {
        public RaymanDesignerProgressionSlotViewModel(ProgressionGameViewModel game, LocalizedString? name, int index, int collectiblesCount, int totalCollectiblesCount, IEnumerable<ProgressionDataViewModel> dataItems, Dictionary<string, int> levelTimes, FileSystemPath saveDir) : base(game, name, index, collectiblesCount, totalCollectiblesCount, dataItems)
        {
            LevelTimes = levelTimes;
            SaveDir = saveDir;
            CanExport = true;
            CanImport = false;
        }

        public Dictionary<string, int> LevelTimes { get; }
        public FileSystemPath SaveDir { get; }

        protected override void ExportSlot(FileSystemPath filePath)
        {
            JsonHelpers.SerializeToFile(LevelTimes, filePath);
        }

        protected override void ImportSlot(FileSystemPath filePath)
        {
            // Read the JSON file
            Dictionary<string, int> lvlTimes = JsonHelpers.DeserializeFromFile<Dictionary<string, int>>(filePath);

            // TODO-UPDATE: Implement - encode level time back to encrypted format and write to files
            throw new NotImplementedException();
        }
    }
}