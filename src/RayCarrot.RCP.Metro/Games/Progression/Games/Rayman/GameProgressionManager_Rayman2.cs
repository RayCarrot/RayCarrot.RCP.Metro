using System.Globalization;
using System.IO;
using BinarySerializer.OpenSpace;

namespace RayCarrot.RCP.Metro;

public class GameProgressionManager_Rayman2 : GameProgressionManager
{
    public GameProgressionManager_Rayman2(GameInstallation gameInstallation, string backupName) 
        : base(gameInstallation, backupName) { }

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    public override GameBackups_Directory[] BackupDirectories => new GameBackups_Directory[]
    {
        new(GameInstallation.InstallLocation + "Data" + "SaveGame", SearchOption.AllDirectories, "*", "0", 0),
        new(GameInstallation.InstallLocation + "Data" + "Options", SearchOption.AllDirectories, "*", "1", 0)
    };

    public override async IAsyncEnumerable<GameProgressionSlot> LoadSlotsAsync(FileSystemWrapper fileSystem)
    {
        R2ConfigFile? config;

        // Get the config data directory
        IOSearchPattern? configDir = fileSystem.GetDirectory(new IOSearchPattern(InstallDir + "Data" + "Options", SearchOption.AllDirectories, "*.cfg"));

        if (configDir == null)
            yield break;

        // Read the config file
        using (RCPContext configContext = new(configDir.DirPath))
            config = await configContext.ReadFileDataAsync<R2ConfigFile>("Current.cfg", new R2SaveEncoder(), removeFileWhenComplete: false);

        if (config == null)
            yield break;

        // Get the save data directory
        IOSearchPattern? saveDir = fileSystem.GetDirectory(new IOSearchPattern(InstallDir + "Data" + "SaveGame", SearchOption.AllDirectories, "*.cfg"));

        if (saveDir == null)
            yield break;

        // Create the context
        using RCPContext context = new(saveDir.DirPath);

        foreach (R2ConfigSlot saveSlot in config.Slots)
        {
            string slotFilePath = $@"Slot{saveSlot.SlotIndex}\General.sav";

            Logger.Info("{0} slot {1} is being loaded...", GameInstallation.FullId, saveSlot.SlotIndex);

            R2GeneralSaveFile? saveData= await context.ReadFileDataAsync<R2GeneralSaveFile>(slotFilePath, new R2SaveEncoder(), removeFileWhenComplete: false);

            if (saveData == null)
            {
                Logger.Info("{0} slot was not found", GameInstallation.FullId);
                continue;
            }

            Logger.Info("Slot has been deserialized");

            // Get the bit array
            bool[] array = saveData.GlobalArrayAsBitFlags();

            // Get total amount of Lums and cages
            int lums =
                array.Skip(0).Take(800).Select(x => x ? 1 : 0).Sum() +
                array.Skip(1200).Take(194).Select(x => x ? 1 : 0).Sum() +
                // Woods of Light
                array.Skip(1395).Take(5).Select(x => x ? 1 : 0).Sum() +
                // 1000th Lum
                (array[1013] ? 1 : 0);

            int cages = array.Skip(839).Take(80).Select(x => x ? 1 : 0).Sum();
            int walkOfLifeTime = saveData.GlobalArray[12] * 10;
            int walkOfPowerTime = saveData.GlobalArray[11] * 10;

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

            if (walkOfLifeTime > 120)
                progressItems.Add(new GameProgressionDataItem(
                    isPrimaryItem: false, 
                    icon: ProgressionIconAsset.R2_Clock, 
                    header: new ResourceLocString(nameof(Resources.R2_BonusLevelName_1)), 
                    text: $"{new TimeSpan(0, 0, 0, 0, walkOfLifeTime):mm\\:ss\\.ff}"));

            if (walkOfPowerTime > 120)
                progressItems.Add(new GameProgressionDataItem(
                    isPrimaryItem: false, 
                    icon: ProgressionIconAsset.R2_Clock,
                    header: new ResourceLocString(nameof(Resources.R2_BonusLevelName_2)),
                    text: $"{new TimeSpan(0, 0, 0, 0, walkOfPowerTime):mm\\:ss\\.ff}"));

            // Get the name and percentage
            int separatorIndex = saveSlot.SlotDisplayName.LastIndexOf((char)0x20);
            string name = saveSlot.SlotDisplayName.Substring(0, separatorIndex);
            string percentage = saveSlot.SlotDisplayName.Substring(separatorIndex + 1);
            double parsedPercentage = Double.TryParse(percentage, NumberStyles.Any, CultureInfo.InvariantCulture, out double p) ? p : 0;

            yield return new SerializableGameProgressionSlot<R2GeneralSaveFile>(name, saveSlot.SlotIndex, parsedPercentage, progressItems, context, saveData, slotFilePath);

            Logger.Info("{0} slot has been loaded", GameInstallation.FullId);
        }
    }
}