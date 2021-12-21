using System.Collections.Generic;
using System.IO;
using System.Linq;
using NLog;
using RayCarrot.IO;
using RayCarrot.Rayman;
using RayCarrot.Rayman.OpenSpace;

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
        FileSystemPath saveDir = InstallDir + "GAMEDATA" + "SaveGame";

        int index = 0;

        foreach (FileSystemPath filePath in fileSystem.GetDirectory(new IOSearchPattern(saveDir, SearchOption.TopDirectoryOnly, "*.sav"))?.GetFiles() ?? Enumerable.Empty<string>())
        {
            Logger.Info("{0} slot {1} is being loaded...", Game, filePath.Name);

            OpenSpaceSettings settings = OpenSpaceSettings.GetDefaultSettings(OpenSpaceGame.Rayman3, Platform.PC);
            Rayman3PCSaveData? saveData = await SerializeFileDataAsync<Rayman3PCSaveData>(filePath, settings, new Rayman3SaveDataEncoder());

            if (saveData == null)
            {
                Logger.Info("{0} slot was not found", Game);
                continue;
            }

            Logger.Info("Slot has been deserialized");

            // Create the collection with items for each level + general information
            ProgressionDataViewModel[] progressItems = 
            {
                // TODO-UPDATE: Localize
                new ProgressionDataViewModel(isPrimaryItem: true, icon: ProgressionIcon.R3_Cage, header: new ConstLocString("Cages"), value: saveData.TotalCages, max: 60),
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

            yield return new SerializableProgressionSlotViewModel<Rayman3PCSaveData>(this, new ConstLocString($"{filePath.RemoveFileExtension().Name}"), index, saveData.TotalCages, 60, progressItems, saveData, settings)
            {
                FilePath = filePath,
                ImportEncoder = new Rayman3SaveDataEncoder(),
            };

            Logger.Info("{0} slot has been loaded", Game);

            index++;
        }
    }
}