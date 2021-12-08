using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using NLog;
using RayCarrot.Binary;
using RayCarrot.IO;
using RayCarrot.Rayman;
using RayCarrot.Rayman.OpenSpace;

namespace RayCarrot.RCP.Metro;

public class ProgressionGameViewModel_Rayman2 : ProgressionGameViewModel
{
    public ProgressionGameViewModel_Rayman2() : base(Games.Rayman2) { }

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    protected override async Task LoadSlotsAsync()
    {
        // Get the save data directory
        FileSystemPath saveDir = InstallDir + "Data";

        // Get the paths
        FileSystemPath configFilePath = saveDir + "Options" + "Current.cfg";
        FileSystemPath saveGamePath = saveDir + "SaveGame";

        if (!configFilePath.FileExists)
            return;

        Rayman2PCConfigData config = await Task.Run(() =>
        {
            // Create streams
            using FileStream saveFileStream = File.OpenRead(configFilePath);
            using MemoryStream decodedDataStream = new MemoryStream();

            // Decode the save file
            new Rayman12PCSaveDataEncoder().Decode(saveFileStream, decodedDataStream);

            // Set position to 0
            decodedDataStream.Position = 0;

            // Get the serialized data
            return BinarySerializableHelpers.ReadFromStream<Rayman2PCConfigData>(decodedDataStream, OpenSpaceSettings.GetDefaultSettings(OpenSpaceGame.Rayman2, Platform.PC), Services.App.GetBinarySerializerLogger(configFilePath.Name));
        });

        foreach (Rayman2PCConfigSlotData saveSlot in config.Slots)
        {
            FileSystemPath slotFilePath = saveGamePath + $"Slot{saveSlot.SlotIndex}" + "General.sav";

            Logger.Info("Rayman 2 slot {0} is being loaded...", saveSlot.SlotIndex);

            // Make sure the file exists
            if (!slotFilePath.FileExists)
            {
                Logger.Info("Slot was not loaded due to not being found");
                continue;
            }

            Rayman2PCSaveData saveData = await Task.Run(() =>
            {
                // Open the file in a stream
                using var fileStream = File.Open(slotFilePath, FileMode.Open, FileAccess.Read);

                // Create a memory stream
                using var memStream = new MemoryStream();

                // Decode the data
                new Rayman12PCSaveDataEncoder().Decode(fileStream, memStream);

                // Set the position
                memStream.Position = 0;

                // Deserialize and return the data
                return BinarySerializableHelpers.ReadFromStream<Rayman2PCSaveData>(memStream, OpenSpaceSettings.GetDefaultSettings(OpenSpaceGame.Rayman2, Platform.PC), Services.App.GetBinarySerializerLogger(slotFilePath.Name));
            });

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

            Slots.Add(new ProgressionSlotViewModel(new ConstLocString(name), (int)saveSlot.SlotIndex, parsedPercentage, progressItems));

            Logger.Info("Rayman 2 slot has been loaded");
        }
    }
}