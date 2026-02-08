using BinarySerializer.OpenSpace;

namespace RayCarrot.RCP.Metro;

public class GameProgressionManager_Rayman3_GameCube : EmulatedGameProgressionManager
{
    public GameProgressionManager_Rayman3_GameCube(GameInstallation gameInstallation, string progressionId) 
        : base(gameInstallation, progressionId) 
    { }

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    public override async IAsyncEnumerable<EmulatedGameProgressionSlot> LoadSlotsAsync(EmulatedSave emulatedSave)
    {
        // Check the name so that we skip the photo saves
        if (emulatedSave is EmulatedGameCubeSave gameCubeSave && gameCubeSave.FileName != "RAYMAN 3 Save Data")
            yield break;

        R3GameCubeSave save = await emulatedSave.ReadAsync<R3GameCubeSave>();

        Logger.Info("Save file has been deserialized");

        for (int i = 0; i < save.MaxSaveFiles; i++)
        {
            if (!save.ExistingSaveFiles[i])
                continue;

            R3SaveFile saveData = save.SaveFiles[i];
            IReadOnlyList<GameProgressionDataItem> progressItems = Rayman3Progression.CreateProgressionItems(saveData, out int collectiblesCount, out int maxCollectiblesCount);

            int slotIndex = i;
            yield return new SerializabeEmulatedGameProgressionSlot<R3GameCubeSave>(
                name: save.SaveFileNames[i],
                index: i,
                collectiblesCount: collectiblesCount,
                totalCollectiblesCount: maxCollectiblesCount,
                emulatedSave: emulatedSave,
                dataItems: progressItems,
                serializable: save)
            {
                GetExportObject = x => x.SaveFiles[slotIndex],
                SetImportObject = (x, o) => x.SaveFiles[slotIndex] = (R3SaveFile)o,
                ExportedType = typeof(R3SaveFile)
            };

            Logger.Info("{0} save {1} has been loaded", GameInstallation.FullId, i);
        }
    }
}