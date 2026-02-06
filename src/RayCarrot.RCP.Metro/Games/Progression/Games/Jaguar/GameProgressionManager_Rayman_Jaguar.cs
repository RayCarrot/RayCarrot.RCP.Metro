using BinarySerializer.Ray1.Jaguar;

namespace RayCarrot.RCP.Metro;

public class GameProgressionManager_Rayman_Jaguar : EmulatedGameProgressionManager
{
    public GameProgressionManager_Rayman_Jaguar(GameInstallation gameInstallation, string progressionId) 
        : base(gameInstallation, progressionId) 
    { }

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    public override async IAsyncEnumerable<EmulatedGameProgressionSlot> LoadSlotsAsync(EmulatedSave emulatedSave)
    {
        JAG_SaveData saveData = await emulatedSave.ReadAsync<JAG_SaveData>();

        for (int saveIndex = 0; saveIndex < saveData.SaveSlots.Length; saveIndex++)
        {
            JAG_SaveSlot saveSlot = saveData.SaveSlots[saveIndex];

            if (saveSlot.SaveName.IsNullOrEmpty())
                continue;

            int cages = 0;

            void addCages(JAG_SaveLevel saveLevel)
            {
                if (saveLevel.Cage1)
                    cages++;
                if (saveLevel.Cage2)
                    cages++;
                if (saveLevel.Cage3)
                    cages++;
                if (saveLevel.Cage4)
                    cages++;
                if (saveLevel.Cage5)
                    cages++;
                if (saveLevel.Cage6)
                    cages++;
            }

            // Calculate cages from each level
            addCages(saveSlot.PinkPlantWoods);
            addCages(saveSlot.AnguishLagoon);
            addCages(saveSlot.ForgottenSwamps);
            addCages(saveSlot.MoskitosNest);
            addCages(saveSlot.BongoHills);
            addCages(saveSlot.AllegroPresto);
            addCages(saveSlot.GongHeights);
            addCages(saveSlot.MrSaxsHullaballoo);
            addCages(saveSlot.TwilightGulch);
            addCages(saveSlot.TheHardRocks);
            addCages(saveSlot.MrStonesPeaks);
            addCages(saveSlot.EraserPlains);
            addCages(saveSlot.PencilPentathlon);
            addCages(saveSlot.SpaceMamasCrater);
            addCages(saveSlot.CrystalPalace);
            addCages(saveSlot.EatatJoes);
            addCages(saveSlot.MrSkopsStalactites);

            // Convert from BCD
            int livesCount = 0;
            livesCount += 10 * (saveSlot.LivesCount >> 4);
            livesCount += saveSlot.LivesCount & 0xF;

            int slotIndex = saveIndex;

            yield return new SerializabeEmulatedGameProgressionSlot<JAG_SaveData>(
                name: saveSlot.SaveName.ToUpper(),
                index: saveIndex,
                collectiblesCount: cages,
                totalCollectiblesCount: 102,
                emulatedSave: emulatedSave,
                dataItems: new[]
                {
                    new GameProgressionDataItem(
                        isPrimaryItem: true,
                        icon: ProgressionIconAsset.R1_Cage,
                        header: new ResourceLocString(nameof(Resources.Progression_Cages)),
                        value: cages,
                        max: 102),
                    new GameProgressionDataItem(
                        isPrimaryItem: false,
                        icon: ProgressionIconAsset.R1_Continue,
                        header: new ResourceLocString(nameof(Resources.Progression_Continues)),
                        value: saveSlot.ContinuesCount),
                    new GameProgressionDataItem(
                        isPrimaryItem: false,
                        icon: ProgressionIconAsset.R1_Life,
                        header: new ResourceLocString(nameof(Resources.Progression_Lives)),
                        value: livesCount),
                },
                serializable: saveData)
            {
                GetExportObject = x => x.SaveSlots[slotIndex],
                SetImportObject = (x, o) => x.SaveSlots[slotIndex] = (JAG_SaveSlot)o,
                ExportedType = typeof(JAG_SaveSlot)
            };
        }

        Logger.Info("{0} save has been loaded", GameInstallation.FullId);
    }
}