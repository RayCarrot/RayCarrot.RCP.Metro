using BinarySerializer.OpenSpace;

namespace RayCarrot.RCP.Metro;

public class GameProgressionManager_RaymanArena_GameCube : EmulatedGameProgressionManager
{
    public GameProgressionManager_RaymanArena_GameCube(GameInstallation gameInstallation, string progressionId) 
        : base(gameInstallation, progressionId) 
    { }

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    public override async IAsyncEnumerable<EmulatedGameProgressionSlot> LoadSlotsAsync(EmulatedSave emulatedSave)
    {
        // Check the name
        if (emulatedSave is EmulatedGameCubeSave gameCubeSave && gameCubeSave.FileName != "Rayman Arena")
            yield break;

        RAGameCubeSave save = await emulatedSave.ReadAsync<RAGameCubeSave>();

        Logger.Info("Save file has been deserialized");

        // Get the status for each save slot
        R3SaveValue[] slotStatus = save.SaveFile.Elements.First(x => x.ElementName == "memory_slotstatus").Values;

        // Enumerate every slot
        for (int slotIndex = 0; slotIndex < slotStatus.Length; slotIndex++)
        {
            // Skip if no slot
            if (slotStatus[slotIndex].IntegerValue == 0)
                continue;

            // Other save data which seems to be all empty:
            // extrabonus_fight: 12 values which are all -1
            // bestscore_fight: 12 values which are all -1
            // divers: 10 values which are all 0
            // extrabonus_run: 12 values which are all -1
            // bestscore_run: 12 values which are all -1
            
            // Get the save name
            string name = save.SaveFile.Elements.First(x => x.ElementName == $"memory_slotname_{slotIndex}").Values[0].StringValue;

            // Get the game params
            int[] gameParams = save.SaveFile.Elements.
                First(x => x.ElementName == $"game_parameters{slotIndex}").Values.
                Select(x => (int)x.IntegerValue).
                ToArray();

            // Percentage calculation re-implemented from game macro 68 for perso OLI_MainMenu
            double percentage = 
                // Beginner League
                gameParams[52] + 
                gameParams[53] + 
                gameParams[54] + 
                gameParams[55] + 
                // Pro League
                gameParams[56] + 
                gameParams[57] + 
                gameParams[58] + 
                gameParams[59];

            // Pro League values might be -1, so offset that
            if (gameParams[56] == -1)
                percentage++;
            if (gameParams[57] == -1)
                percentage++;
            if (gameParams[58] == -1)
                percentage++;
            if (gameParams[59] == -1)
                percentage++;

            // Master League
            percentage += gameParams[47];
            percentage += gameParams[48];
            percentage += gameParams[49];
            percentage += gameParams[50];

            // Get the amount of skins
            int skinsCount = 0;
            for (int i = 0; i < 9; i++)
                skinsCount += gameParams[60 + i];

            // Get the amount of completed beginner league cups
            int beginnerCupsCount = 0;
            for (int i = 0; i < 4; i++)
                beginnerCupsCount += gameParams[52 + i] / 3;

            // Get the amount of completed pro league cups
            int proCupsCount = 0;
            for (int i = 0; i < 4; i++)
                proCupsCount += Math.Max(0, gameParams[56 + i]) / 3;

            // Get the amount of completed master league cups
            int masterCupsCount = 0;
            for (int i = 0; i < 4; i++)
                masterCupsCount += gameParams[47 + i];

            yield return new SerializabeEmulatedGameProgressionSlot<RAGameCubeSave>(
                name: name,
                index: slotIndex,
                percentage: percentage,
                emulatedSave: emulatedSave,
                dataItems: new[]
                {
                    // TODO-LOC
                    new GameProgressionDataItem(
                        isPrimaryItem: true,
                        icon: ProgressionIconAsset.RA_Skin,
                        header: "Skins",
                        value: skinsCount,
                        max: 29),
                    new GameProgressionDataItem(
                        isPrimaryItem: true,
                        icon: ProgressionIconAsset.RA_Beginner,
                        header: "Beginner League Cups",
                        value: beginnerCupsCount,
                        max: 16),
                    new GameProgressionDataItem(
                        isPrimaryItem: true,
                        icon: ProgressionIconAsset.RA_Pro,
                        header: "Pro League Cups",
                        value: proCupsCount,
                        max: 16),
                    new GameProgressionDataItem(
                        isPrimaryItem: true,
                        icon: ProgressionIconAsset.RA_Master,
                        header: "Master Leagues Completed",
                        value: masterCupsCount,
                        max: 4),
                },
                serializable: save)
            {
                GetExportObject = x => x.SaveFile,
                SetImportObject = (x, o) => x.SaveFile = (R3SaveFile)o,
                ExportedType = typeof(R3SaveFile)
            };

            Logger.Info("{0} slot has been loaded", GameInstallation.FullId);
        }
    }
}