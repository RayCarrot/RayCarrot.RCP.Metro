using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BinarySerializer.Ray1;
using NLog;

namespace RayCarrot.RCP.Metro;

public class ProgressionGameViewModel_RaymanDesigner : ProgressionGameViewModel
{
    public ProgressionGameViewModel_RaymanDesigner(GameInstallation gameInstallation) : base(gameInstallation) { }

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    protected virtual int LevelsCount => 24;
    protected virtual Ray1EngineVersion EngineVersion => Ray1EngineVersion.PC_Kit;

    protected override GameBackups_Directory[] BackupDirectories => new GameBackups_Directory[]
    {
        new(GameInstallation.InstallLocation, SearchOption.TopDirectoryOnly, "*.cfg", "0", 0),
        new(GameInstallation.InstallLocation + "PCMAP", SearchOption.TopDirectoryOnly, "*.sct", "1", 0),
        //
        // Note:
        // This will backup the pre-installed maps and the world files as well. This is due to how the backup manager works.
        // In the future I might make a separate manager for the maps again, in which case the search pattern "MAPS???" should get the
        // correct mapper directories within each world directory
        //
        new(GameInstallation.InstallLocation + "CAKE", SearchOption.AllDirectories, "*", "Mapper0", 0),
        new(GameInstallation.InstallLocation + "CAVE", SearchOption.AllDirectories, "*", "Mapper1", 0),
        new(GameInstallation.InstallLocation + "IMAGE", SearchOption.AllDirectories, "*", "Mapper2", 0),
        new(GameInstallation.InstallLocation + "JUNGLE", SearchOption.AllDirectories, "*", "Mapper3", 0),
        new(GameInstallation.InstallLocation + "MOUNTAIN", SearchOption.AllDirectories, "*", "Mapper4", 0),
        new(GameInstallation.InstallLocation + "MUSIC", SearchOption.AllDirectories, "*", "Mapper5", 0),
    };

    protected override async IAsyncEnumerable<ProgressionSlotViewModel> LoadSlotsAsync(FileSystemWrapper fileSystem)
    {
        FileSystemPath saveDir = InstallDir + "PCMAP";

        string[] shortWorldNames = { "", "JUN", "MUS", "MON", "IMA", "CAV", "CAK" };
        string[] longWorldNames = { "", "Jungle", "Music", "Mountain", "Image", "Cave", "Cake" };

        List<ProgressionDataViewModel> progressItems = new();
        Dictionary<string, int> levelTimes = new();

        IOSearchPattern? dir = fileSystem.GetDirectory(new IOSearchPattern(saveDir, SearchOption.TopDirectoryOnly, "*.sct"));

        if (dir == null)
            yield break;

        Logger.Info("{0} saves from {1} are being loaded...", GameInstallation.Id, saveDir.Name);

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
                Logger.Info("{0} slot was not found", GameInstallation.Id);
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

            progressItems.Add(new ProgressionDataViewModel(
                isPrimaryItem: false, 
                icon: ProgressionIcon.R1_Flag, 
                header: $"{longWorldNames[save.World]} {save.Level}", 
                text: $"{time:mm\\:ss\\.fff}"));
        }

        int levelsFinished = progressItems.Count;

        if (levelsFinished > 0)
        {
            // Add levels completed
            progressItems.Insert(0, new ProgressionDataViewModel(
                isPrimaryItem: true,
                icon: ProgressionIcon.R1_Flag,
                header: new ResourceLocString(nameof(Resources.Progression_LevelsCompleted)),
                value: levelsFinished,
                max: LevelsCount));

            yield return new RaymanDesignerProgressionSlotViewModel(this, null, 0, levelsFinished, LevelsCount, progressItems, levelTimes, saveDir);
        }

        Logger.Info("{0} slot has been loaded", GameInstallation.Id);
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

            // TODO: Implement - encode level time back to encrypted format and write to files
            throw new NotImplementedException();
        }
    }
}