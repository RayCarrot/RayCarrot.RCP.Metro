using BinarySerializer;

namespace RayCarrot.RCP.Metro;

// TODO: Doesn't work - the minigame completion count for story slots is wrong, and possible other things too?
public class GameProgressionManager_RaymanRavingRabbids_Ps2 : EmulatedGameProgressionManager
{
    public GameProgressionManager_RaymanRavingRabbids_Ps2(GameInstallation gameInstallation, string progressionId) 
        : base(gameInstallation, progressionId) 
    { }

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    public override async IAsyncEnumerable<EmulatedGameProgressionSlot> LoadSlotsAsync(EmulatedSave emulatedSave)
    {
        RRREncodedObject<RRR_SaveFile> saveData = await emulatedSave.ReadAsync<RRREncodedObject<RRR_SaveFile>>();

        Logger.Info("Save file has been deserialized");

        // Add save slots
        for (int saveIndex = 0; saveIndex < saveData.Obj.StorySlots.Length; saveIndex++)
        {
            RRR_SaveSlot slot = saveData.Obj.StorySlots[saveIndex];

            // Make sure the slot isn't empty
            if (slot.SlotDesc.Time == 0)
                continue;

            IReadOnlyList<GameProgressionDataItem> storyDataItems = RaymanRavingRabbidsProgression.CreateStoryProgressionItems(slot);

            int storySlotIndex = saveIndex;

            yield return new SerializabeEmulatedGameProgressionSlot<RRREncodedObject<RRR_SaveFile>>(slot.SlotDesc.Name, saveIndex, slot.SlotDesc.Progress_Percentage, emulatedSave, storyDataItems, saveData)
            {
                GetExportObject = x => x.Obj.StorySlots[storySlotIndex],
                SetImportObject = (x, o) => x.Obj.StorySlots[storySlotIndex] = (RRR_SaveSlot)o,
                ExportedType = typeof(RRR_SaveSlot),
            };
        }

        IReadOnlyList<GameProgressionDataItem> scoreDataItems = RaymanRavingRabbidsProgression.CreateScoreProgressionItems(
            slot: saveData.Obj.ScoreSlot,
            collectiblesCount: out int collectiblesCount,
            maxCollectiblesCount: out int maxCollectiblesCount);

        // Add score slot
        yield return new SerializabeEmulatedGameProgressionSlot<RRREncodedObject<RRR_SaveFile>>(new ResourceLocString(nameof(Resources.Progression_RRRScoreSlot)), 3, collectiblesCount, maxCollectiblesCount, emulatedSave, scoreDataItems, saveData)
        {
            GetExportObject = x => x.Obj.ScoreSlot,
            SetImportObject = (x, o) => x.Obj.ScoreSlot = (RRR_SaveSlot)o,
            ExportedType = typeof(RRR_SaveSlot),
            SlotGroup = 1,
        };

        Logger.Info("{0} save has been loaded", GameInstallation.FullId);
    }

#nullable disable
    // Wrapper class to allow the file to be encoded since we currently don't support an encoding for emulated saves (should be fixed in future refactor)
    private class RRREncodedObject<T> : BinarySerializable
        where T : BinarySerializable, new()
    {
        public T Obj { get; set; }

        public override void SerializeImpl(SerializerObject s)
        {
            s.DoEncoded(new RRR_SaveEncoder(), () => Obj = s.SerializeObject<T>(Obj, name: nameof(Obj)));
        }
    }
}