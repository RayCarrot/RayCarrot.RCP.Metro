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

        TimeSpan timePlayed = new(0, 0, 0, 0, globalFixData.TimePlayed);

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

            // TODO-LOC
            new GameProgressionDataItem(
                isPrimaryItem: false,
                icon: ProgressionIconAsset.R2_Main,
                header: "Distance covered",
                text: $"{(int)globalFixData.DistanceCovered} meters"),
            new GameProgressionDataItem(
                isPrimaryItem: false,
                icon: ProgressionIconAsset.R2_Main,
                header: "Time played",
                text: String.Format("{0}:{1:mm}:{1:ss}", Math.Floor(timePlayed.TotalHours), timePlayed)),
            new GameProgressionDataItem(
                isPrimaryItem: false,
                icon: ProgressionIconAsset.R2_Main,
                header: "Player shots fired",
                text: $"{globalFixData.ShotsFired}"),
            new GameProgressionDataItem(
                isPrimaryItem: false,
                icon: ProgressionIconAsset.R2_Main,
                header: "Shooting accuracy",
                text: $"{(globalFixData.ShotsFired == 0 ? 0 : globalFixData.ShotsHitTarget * 100 / globalFixData.ShotsFired)} %"),
            new GameProgressionDataItem(
                isPrimaryItem: false,
                icon: ProgressionIconAsset.R2_Main,
                header: "Times killed",
                text: $"{globalFixData.TimesKilled}"),
            new GameProgressionDataItem(
                isPrimaryItem: false,
                icon: ProgressionIconAsset.R2_Main,
                header: "Number of try again",
                text: $"{globalFixData.NumberOfTryAgain}"),
            new GameProgressionDataItem(
                isPrimaryItem: false,
                icon: ProgressionIconAsset.R2_Main,
                header: "Number of jumps",
                text: $"{globalFixData.NumberOfJumps}"),
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