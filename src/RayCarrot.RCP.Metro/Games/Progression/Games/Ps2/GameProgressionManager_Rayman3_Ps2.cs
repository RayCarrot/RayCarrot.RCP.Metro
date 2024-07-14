using BinarySerializer.OpenSpace;

namespace RayCarrot.RCP.Metro;

public class GameProgressionManager_Rayman3_Ps2 : EmulatedGameProgressionManager
{
    public GameProgressionManager_Rayman3_Ps2(GameInstallation gameInstallation, string progressionId) 
        : base(gameInstallation, progressionId) 
    { }

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    public override async IAsyncEnumerable<EmulatedGameProgressionSlot> LoadSlotsAsync(EmulatedSave emulatedSave)
    {
        R3SaveFile saveData = await emulatedSave.ReadAsync<R3SaveFile>();

        Logger.Info("Save file has been deserialized");

        IReadOnlyList<GameProgressionDataItem> progressItems = Rayman3Progression.CreateProgressionItems(saveData, out int collectiblesCount, out int maxCollectiblesCount);

        yield return new SerializabeEmulatedGameProgressionSlot<R3SaveFile>(
            name: (emulatedSave as EmulatedPs2Save)?.PrimaryFileName.Substring(13),
            index: -1,
            collectiblesCount: collectiblesCount,
            totalCollectiblesCount: maxCollectiblesCount,
            emulatedSave: emulatedSave,
            dataItems: progressItems,
            serializable: saveData);

        Logger.Info("{0} save has been loaded", GameInstallation.FullId);
    }
}