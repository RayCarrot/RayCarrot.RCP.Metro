using BinarySerializer.OpenSpace;

namespace RayCarrot.RCP.Metro;

public class GameProgressionManager_RaymanMArena_Ps2 : EmulatedGameProgressionManager
{
    public GameProgressionManager_RaymanMArena_Ps2(GameInstallation gameInstallation, string progressionId) 
        : base(gameInstallation, progressionId) 
    { }

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    public override async IAsyncEnumerable<EmulatedGameProgressionSlot> LoadSlotsAsync(EmulatedSave emulatedSave)
    {
        RMSaveFile saveData = await emulatedSave.ReadAsync<RMSaveFile>();

        Logger.Info("Save file has been deserialized");

        // There is a max of 8 save slots in every version
        for (int slotIndex = 0; slotIndex < 8; slotIndex++)
        {
            // Get the save name
            string name = saveData.Items.First(x => x.Key == "sg_names").Values[slotIndex].StringValue;

            // Make sure it's valid
            if (name.Contains("No Data"))
                continue;

            IReadOnlyList<GameProgressionDataItem> dataItems = RaymanMArenaProgression.CreateProgressionItems(
                saveData, false, slotIndex, out int collectiblesCount, out int maxCollectiblesCount);

            yield return new SerializabeEmulatedGameProgressionSlot<RMSaveFile>(
                name: name.TrimEnd(),
                index: slotIndex,
                collectiblesCount: collectiblesCount,
                totalCollectiblesCount: maxCollectiblesCount,
                emulatedSave: emulatedSave,
                dataItems: dataItems,
                serializable: saveData);

            Logger.Info("{0} slot has been loaded", GameInstallation.FullId);
        }
    }
}