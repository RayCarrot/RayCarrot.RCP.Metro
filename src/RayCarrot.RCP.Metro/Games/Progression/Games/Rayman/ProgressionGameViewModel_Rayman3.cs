using System.Collections.Generic;
using System.IO;
using System.Linq;
using BinarySerializer.OpenSpace;
using NLog;

namespace RayCarrot.RCP.Metro;

public class ProgressionGameViewModel_Rayman3 : ProgressionGameViewModel
{
    public ProgressionGameViewModel_Rayman3() : base(Games.Rayman3) { }

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    protected override GameBackups_Directory[] BackupDirectories => new GameBackups_Directory[]
    {
        new GameBackups_Directory(Game.GetInstallDir() + "GAMEDATA" + "SaveGame", SearchOption.TopDirectoryOnly, "*", "0", 0)
    };

    protected override async IAsyncEnumerable<ProgressionSlotViewModel> LoadSlotsAsync(FileSystemWrapper fileSystem)
    {
        IOSearchPattern? dir = fileSystem.GetDirectory(new IOSearchPattern(InstallDir + "GAMEDATA" + "SaveGame", SearchOption.TopDirectoryOnly, "*.sav"));

        if (dir == null)
            yield break;

        int index = 0;

        using RCPContext context = new(dir.DirPath);

        foreach (FileSystemPath filePath in dir.GetFiles())
        {
            string fileName = filePath.Name;

            Logger.Info("{0} slot {1} is being loaded...", Game, fileName);

            R3SaveFile? saveData = await context.ReadFileDataAsync<R3SaveFile>(fileName, new R3SaveEncoder(), removeFileWhenComplete: false);

            if (saveData == null)
            {
                Logger.Info("{0} slot was not found", Game);
                continue;
            }

            Logger.Info("Slot has been deserialized");

            int[] stampScores = { 20820, 44500, 25900, 58000, 55500, 26888, 26700, 43700, 48000 };

            int stamps = stampScores.Where((stampScore, i) => stampScore < saveData.Levels[i].Score).Count();

            // Create the collection with items for each level + general information
            ProgressionDataViewModel[] progressItems = 
            {
                // TODO-UPDATE: Localize
                new ProgressionDataViewModel(isPrimaryItem: true, icon: ProgressionIcon.R3_Cage, header: new ConstLocString("Cages"), value: saveData.TotalCages, max: 60),
                new ProgressionDataViewModel(true, ProgressionIcon.R3_Stamp, new ConstLocString("Stamps"), stamps, stampScores.Length),
                new ProgressionDataViewModel(false, ProgressionIcon.R3_Score, new ConstLocString("Total score"), saveData.TotalScore),
                new ProgressionDataViewModel(false, ProgressionIcon.R3_Score, new ResourceLocString(nameof(Resources.Progression_R3_Level1Header)), saveData.Levels[0].Score),
                new ProgressionDataViewModel(false, ProgressionIcon.R3_Score, new ResourceLocString(nameof(Resources.Progression_R3_Level2Header)), saveData.Levels[1].Score),
                new ProgressionDataViewModel(false, ProgressionIcon.R3_Score, new ResourceLocString(nameof(Resources.Progression_R3_Level3Header)), saveData.Levels[2].Score),
                new ProgressionDataViewModel(false, ProgressionIcon.R3_Score, new ResourceLocString(nameof(Resources.Progression_R3_Level4Header)), saveData.Levels[3].Score),
                new ProgressionDataViewModel(false, ProgressionIcon.R3_Score, new ResourceLocString(nameof(Resources.Progression_R3_Level5Header)), saveData.Levels[4].Score),
                new ProgressionDataViewModel(false, ProgressionIcon.R3_Score, new ResourceLocString(nameof(Resources.Progression_R3_Level6Header)), saveData.Levels[5].Score),
                new ProgressionDataViewModel(false, ProgressionIcon.R3_Score, new ResourceLocString(nameof(Resources.Progression_R3_Level7Header)), saveData.Levels[6].Score),
                new ProgressionDataViewModel(false, ProgressionIcon.R3_Score, new ResourceLocString(nameof(Resources.Progression_R3_Level8Header)), saveData.Levels[7].Score),
                new ProgressionDataViewModel(false, ProgressionIcon.R3_Score, new ResourceLocString(nameof(Resources.Progression_R3_Level9Header)), saveData.Levels[8].Score),
            };

            yield return new SerializableProgressionSlotViewModel<R3SaveFile>(this, new ConstLocString($"{filePath.RemoveFileExtension().Name}"), index, saveData.TotalCages + stamps, 60 + stampScores.Length, progressItems, context, saveData, fileName);

            Logger.Info("{0} slot has been loaded", Game);

            index++;
        }
    }
}