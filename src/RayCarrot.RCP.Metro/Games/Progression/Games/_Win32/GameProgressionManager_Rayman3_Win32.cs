using System.IO;
using BinarySerializer.OpenSpace;

namespace RayCarrot.RCP.Metro;

public class GameProgressionManager_Rayman3_Win32 : GameProgressionManager
{
    public GameProgressionManager_Rayman3_Win32(GameInstallation gameInstallation, string progressionId) 
        : base(gameInstallation, progressionId) { }

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    public override GameBackups_Directory[] BackupDirectories => new GameBackups_Directory[]
    {
        new(GameInstallation.InstallLocation.Directory + "GAMEDATA" + "SaveGame", SearchOption.TopDirectoryOnly, "*", "0", 0)
    };

    public override async IAsyncEnumerable<GameProgressionSlot> LoadSlotsAsync(FileSystemWrapper fileSystem)
    {
        IOSearchPattern? dir = fileSystem.GetDirectory(new IOSearchPattern(InstallDir + "GAMEDATA" + "SaveGame", SearchOption.TopDirectoryOnly, "*.sav"));

        if (dir == null)
            yield break;

        int index = 0;

        using RCPContext context = new(dir.DirPath);

        foreach (FileSystemPath filePath in dir.GetFiles())
        {
            string fileName = filePath.Name;

            Logger.Info("{0} slot {1} is being loaded...", GameInstallation.FullId, fileName);

            R3SaveFile? saveData = await context.ReadFileDataAsync<R3SaveFile>(fileName, new R3SaveEncoder(), removeFileWhenComplete: false);

            if (saveData == null)
            {
                Logger.Info("{0} slot was not found", GameInstallation.FullId);
                continue;
            }

            Logger.Info("Slot has been deserialized");

            int[] stampScores = { 20820, 44500, 25900, 58000, 55500, 26888, 26700, 43700, 48000 };

            int stamps = stampScores.Where((stampScore, i) => stampScore < saveData.Levels[i].Score).Count();

            // Create the collection with items for each level + general information
            GameProgressionDataItem[] progressItems = 
            {
                new GameProgressionDataItem(
                    isPrimaryItem: true, 
                    icon: ProgressionIconAsset.R3_Cage,
                    header: new ResourceLocString(nameof(Resources.Progression_Cages)),
                    value: saveData.TotalCages, 
                    max: 60),
                new GameProgressionDataItem(
                    isPrimaryItem: true, 
                    icon: ProgressionIconAsset.R3_Stamp, 
                    header: new ResourceLocString(nameof(Resources.Progression_R3Stamps)),
                    value: stamps, 
                    max: stampScores.Length),
                new GameProgressionDataItem(
                    isPrimaryItem: false, 
                    icon: ProgressionIconAsset.R3_Score, 
                    header: new ResourceLocString(nameof(Resources.Progression_TotalScore)),
                    value: saveData.TotalScore),
                
                new GameProgressionDataItem(false, ProgressionIconAsset.R3_Score, new ResourceLocString(nameof(Resources.Progression_R3_Level1Header)), saveData.Levels[0].Score),
                new GameProgressionDataItem(false, ProgressionIconAsset.R3_Score, new ResourceLocString(nameof(Resources.Progression_R3_Level2Header)), saveData.Levels[1].Score),
                new GameProgressionDataItem(false, ProgressionIconAsset.R3_Score, new ResourceLocString(nameof(Resources.Progression_R3_Level3Header)), saveData.Levels[2].Score),
                new GameProgressionDataItem(false, ProgressionIconAsset.R3_Score, new ResourceLocString(nameof(Resources.Progression_R3_Level4Header)), saveData.Levels[3].Score),
                new GameProgressionDataItem(false, ProgressionIconAsset.R3_Score, new ResourceLocString(nameof(Resources.Progression_R3_Level5Header)), saveData.Levels[4].Score),
                new GameProgressionDataItem(false, ProgressionIconAsset.R3_Score, new ResourceLocString(nameof(Resources.Progression_R3_Level6Header)), saveData.Levels[5].Score),
                new GameProgressionDataItem(false, ProgressionIconAsset.R3_Score, new ResourceLocString(nameof(Resources.Progression_R3_Level7Header)), saveData.Levels[6].Score),
                new GameProgressionDataItem(false, ProgressionIconAsset.R3_Score, new ResourceLocString(nameof(Resources.Progression_R3_Level8Header)), saveData.Levels[7].Score),
                new GameProgressionDataItem(false, ProgressionIconAsset.R3_Score, new ResourceLocString(nameof(Resources.Progression_R3_Level9Header)), saveData.Levels[8].Score),
            };

            yield return new SerializableGameProgressionSlot<R3SaveFile>($"{filePath.RemoveFileExtension().Name}", index, saveData.TotalCages + stamps, 60 + stampScores.Length, progressItems, context, saveData, fileName);

            Logger.Info("{0} slot has been loaded", GameInstallation.FullId);

            index++;
        }
    }
}