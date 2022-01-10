using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using BinarySerializer.OpenSpace;
using NLog;

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
        IOSearchPattern? saveDir = fileSystem.GetDirectory(new IOSearchPattern(InstallDir + "Data", SearchOption.AllDirectories, "*.cfg"));

        if (saveDir == null)
            yield break;

        // Create the context
        using RCPContext context = new(saveDir.DirPath);

        // Read the config file
        R2ConfigFile? config = await context.ReadFileDataAsync<R2ConfigFile>(@"Options\Current.cfg", new R2SaveEncoder(), removeFileWhenComplete: false);

        if (config == null)
            yield break; 

        foreach (R2ConfigSlot saveSlot in config.Slots)
        {
            string slotFilePath = $@"SaveGame\Slot{saveSlot.SlotIndex}\General.sav";

            Logger.Info("{0} slot {1} is being loaded...", Game, saveSlot.SlotIndex);

            R2GeneralSaveFile? saveData= await context.ReadFileDataAsync<R2GeneralSaveFile>(slotFilePath, new R2SaveEncoder(), removeFileWhenComplete: false);

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
                new ProgressionDataViewModel(
                    isPrimaryItem: true, 
                    icon: ProgressionIcon.R2_Lum, 
                    header: new ResourceLocString(nameof(Resources.Progression_Lums)),
                    value: lums, 
                    max: 1000),
                new ProgressionDataViewModel(
                    isPrimaryItem: true, 
                    icon: ProgressionIcon.R2_Cage, 
                    header: new ResourceLocString(nameof(Resources.Progression_Cages)),
                    value: cages, 
                    max: 80),
            };

            if (walkOfLifeTime > 120)
                progressItems.Add(new ProgressionDataViewModel(
                    isPrimaryItem: false, 
                    icon: ProgressionIcon.R2_Clock, 
                    header: new ResourceLocString(nameof(Resources.R2_BonusLevelName_1)), 
                    text: new ConstLocString($"{new TimeSpan(0, 0, 0, 0, walkOfLifeTime):mm\\:ss\\.ff}")));

            if (walkOfPowerTime > 120)
                progressItems.Add(new ProgressionDataViewModel(
                    isPrimaryItem: false, 
                    icon: ProgressionIcon.R2_Clock,
                    header: new ResourceLocString(nameof(Resources.R2_BonusLevelName_2)),
                    text: new ConstLocString($"{new TimeSpan(0, 0, 0, 0, walkOfPowerTime):mm\\:ss\\.ff}")));

            // Get the name and percentage
            int separatorIndex = saveSlot.SlotDisplayName.LastIndexOf((char)0x20);
            string name = saveSlot.SlotDisplayName.Substring(0, separatorIndex);
            string percentage = saveSlot.SlotDisplayName.Substring(separatorIndex + 1);
            double parsedPercentage = Double.TryParse(percentage, NumberStyles.Any, CultureInfo.InvariantCulture, out double p) ? p : 0;

            yield return new SerializableProgressionSlotViewModel<R2GeneralSaveFile>(this, new ConstLocString(name), saveSlot.SlotIndex, parsedPercentage, progressItems, context, saveData, slotFilePath);

            Logger.Info("{0} slot has been loaded", Game);
        }
    }
}