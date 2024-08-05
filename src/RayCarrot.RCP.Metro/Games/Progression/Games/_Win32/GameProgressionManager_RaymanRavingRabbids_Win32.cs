using System.IO;
using BinarySerializer;

namespace RayCarrot.RCP.Metro;

public class GameProgressionManager_RaymanRavingRabbids_Win32 : GameProgressionManager
{
    public GameProgressionManager_RaymanRavingRabbids_Win32(GameInstallation gameInstallation, string progressionId) 
        : base(gameInstallation, progressionId) { }

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    public override GameBackups_Directory[] BackupDirectories => new GameBackups_Directory[]
    {
        new(GameInstallation.InstallLocation.Directory, SearchOption.TopDirectoryOnly, "*.sav", "0", 0),
    };

    public override async IAsyncEnumerable<GameProgressionSlot> LoadSlotsAsync(FileSystemWrapper fileSystem)
    {
        FileSystemPath saveFile = fileSystem.GetFile(InstallDir + "Rayman4.sav");

        Logger.Info("{0} save is being loaded...", GameInstallation.FullId);

        using RCPContext context = new(saveFile.Parent);

        RRR_SaveFile? saveData = await context.ReadFileDataAsync<RRR_SaveFile>(saveFile.Name, new RRR_SaveEncoder(), removeFileWhenComplete: false);

        if (saveData == null)
        {
            Logger.Info("{0} save was not found", GameInstallation.FullId);
            yield break;
        }

        Logger.Info("Save has been deserialized");

        // Add save slots
        for (int saveIndex = 0; saveIndex < saveData.StorySlots.Length; saveIndex++)
        {
            RRR_SaveSlot slot = saveData.StorySlots[saveIndex];

            // Make sure the slot isn't empty
            if (slot.SlotDesc.Time == 0)
                continue;

            IReadOnlyList<GameProgressionDataItem> storyDataItems = RaymanRavingRabbidsProgression.CreateStoryProgressionItems(slot);

            int storySlotIndex = saveIndex;

            yield return new SerializableGameProgressionSlot<RRR_SaveFile>(slot.SlotDesc.Name, saveIndex, slot.SlotDesc.Progress_Percentage, storyDataItems, context, saveData, saveFile.Name)
            {
                GetExportObject = x => x.StorySlots[storySlotIndex],
                SetImportObject = (x, o) => x.StorySlots[storySlotIndex] = (RRR_SaveSlot)o,
                ExportedType = typeof(RRR_SaveSlot),
            };
        }

        IReadOnlyList<GameProgressionDataItem> scoreDataItems = RaymanRavingRabbidsProgression.CreateScoreProgressionItems(
            slot: saveData.ScoreSlot,
            collectiblesCount: out int collectiblesCount,
            maxCollectiblesCount: out int maxCollectiblesCount);

        // Add score slot
        yield return new SerializableGameProgressionSlot<RRR_SaveFile>(new ResourceLocString(nameof(Resources.Progression_RRRScoreSlot)), 3, collectiblesCount, maxCollectiblesCount, scoreDataItems, context, saveData, saveFile.Name)
        {
            GetExportObject = x => x.ScoreSlot,
            SetImportObject = (x, o) => x.ScoreSlot = (RRR_SaveSlot)o,
            ExportedType = typeof(RRR_SaveSlot),
            SlotGroup = 1,
        };

        Logger.Info("{0} save has been loaded", GameInstallation.FullId);
    }
}