namespace RayCarrot.RCP.Metro;

public class GameProgressionManager_Rayman2_Ps2 : EmulatedGameProgressionManager
{
    public GameProgressionManager_Rayman2_Ps2(GameInstallation gameInstallation, string progressionId) 
        : base(gameInstallation, progressionId) 
    { }

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    public override async IAsyncEnumerable<EmulatedGameProgressionSlot> LoadSlotsAsync(EmulatedSave emulatedSave)
    {
        Rayman2Ps2SaveData saveData = await emulatedSave.ReadAsync<Rayman2Ps2SaveData>();

        // Find the global fix data
        Rayman2Ps2SaveData.Data_Fix_3? globalFixData = saveData.PersoDsgDatas.FirstOrDefault(x => x.Data_Fix_3 != null)?.Data_Fix_3;

        if (globalFixData == null)
            yield break;

        if (emulatedSave is not EmulatedPs2Save ps2Save)
            yield break;

        // Read the save info file
        Rayman2Ps2SaveInfo saveInfo = await ps2Save.ReadAsync<Rayman2Ps2SaveInfo>("data.add");

        int lums = globalFixData.LumsPerLevel.Values.Sum();
        int cages = globalFixData.CagesPerLevel.Values.Sum();

        List<GameProgressionDataItem> progressItems = new()
        {
            new GameProgressionDataItem(
                isPrimaryItem: true,
                icon: ProgressionIconAsset.R2_Lum,
                header: new ResourceLocString(nameof(Resources.Progression_Lums)),
                value: lums,
                max: 1000),
            new GameProgressionDataItem(
                isPrimaryItem: true,
                icon: ProgressionIconAsset.R2_Cage,
                header: new ResourceLocString(nameof(Resources.Progression_Cages)),
                value: cages,
                max: 80),
        };

        yield return new SerializabeEmulatedGameProgressionSlot<Rayman2Ps2SaveData>(
            name: saveInfo.Name,
            index: -1,
            percentage: saveInfo.Percentage,
            emulatedSave: emulatedSave,
            dataItems: progressItems,
            serializable: saveData);

        Logger.Info("{0} save has been loaded", GameInstallation.FullId);
    }
}