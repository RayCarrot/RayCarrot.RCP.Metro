using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using NLog;
using RayCarrot.IO;
using RayCarrot.Rayman;
using RayCarrot.Rayman.OpenSpace;

namespace RayCarrot.RCP.Metro;

public class ProgressionGameViewModel_Rayman2 : ProgressionGameViewModel
{
    public ProgressionGameViewModel_Rayman2() : base(Games.Rayman2) { }

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    protected override GameBackups_Directory[] BackupDirectories => new GameBackups_Directory[]
    {
        new GameBackups_Directory(Game.GetInstallDir() + "Data" + "SaveGame", SearchOption.AllDirectories, "*", "0", 0),
        new GameBackups_Directory(Game.GetInstallDir() + "Data" + "Options", SearchOption.AllDirectories, "*", "1", 0)
    };

    protected override async IAsyncEnumerable<ProgressionSlotViewModel> LoadSlotsAsync(FileSystemWrapper fileSystem)
    {
        // Get the save data directory
        FileSystemPath saveDir = InstallDir + "Data";

        // Get the paths
        FileSystemPath configFilePath = saveDir + "Options" + "Current.cfg";
        FileSystemPath saveGamePath = saveDir + "SaveGame";

        OpenSpaceSettings settings = OpenSpaceSettings.GetDefaultSettings(OpenSpaceGame.Rayman2, Platform.PC);
        Rayman2PCConfigData? config = await SerializeFileDataAsync<Rayman2PCConfigData>(fileSystem, configFilePath, settings, new Rayman12PCSaveDataEncoder());

        if (config == null)
            yield break;

        foreach (Rayman2PCConfigSlotData saveSlot in config.Slots)
        {
            FileSystemPath slotFilePath = saveGamePath + $"Slot{saveSlot.SlotIndex}" + "General.sav";

            Logger.Info("{0} slot {1} is being loaded...", Game, saveSlot.SlotIndex);

            Rayman2PCSaveData? saveData = await SerializeFileDataAsync<Rayman2PCSaveData>(fileSystem, slotFilePath, settings, new Rayman12PCSaveDataEncoder());

            if (saveData == null)
            {
                Logger.Info("{0} slot was not found", Game);
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

            List<ProgressionDataViewModel> progressItems = new()
            {
                new ProgressionDataViewModel(true, GameProgression_Icon.R2_Lum, lums, 1000),
                new ProgressionDataViewModel(true, GameProgression_Icon.R2_Cage, cages, 80),
            };

            if (walkOfLifeTime > 120)
                progressItems.Add(new ProgressionDataViewModel(false, GameProgression_Icon.R2_Clock, new ConstLocString($"{new TimeSpan(0, 0, 0, 0, walkOfLifeTime):mm\\:ss\\.ff}"), new ResourceLocString(nameof(Resources.R2_BonusLevelName_1))));

            if (walkOfPowerTime > 120)
                progressItems.Add(new ProgressionDataViewModel(false, GameProgression_Icon.R2_Clock, new ConstLocString($"{new TimeSpan(0, 0, 0, 0, walkOfPowerTime):mm\\:ss\\.ff}"), new ResourceLocString(nameof(Resources.R2_BonusLevelName_2))));

            // Get the name and percentage
            int separatorIndex = saveSlot.SlotDisplayName.LastIndexOf((char)0x20);
            string name = saveSlot.SlotDisplayName.Substring(0, separatorIndex);
            string percentage = saveSlot.SlotDisplayName.Substring(separatorIndex + 1);
            double parsedPercentage = Double.TryParse(percentage, NumberStyles.Any, CultureInfo.InvariantCulture, out double p) ? p : 0;

            yield return new SerializableProgressionSlotViewModel<Rayman2PCSaveData>(this, new ConstLocString(name), (int)saveSlot.SlotIndex, parsedPercentage, progressItems, saveData, settings)
            {
                FilePath = slotFilePath,
                ImportEncoder = new Rayman12PCSaveDataEncoder(),
            };

            Logger.Info("{0} slot has been loaded", Game);
        }
    }
}