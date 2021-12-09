using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NLog;
using RayCarrot.Binary;
using RayCarrot.IO;
using RayCarrot.Rayman;
using RayCarrot.Rayman.UbiArt;

namespace RayCarrot.RCP.Metro;

public class ProgressionGameViewModel_RaymanJungleRun : ProgressionGameViewModel
{
    public ProgressionGameViewModel_RaymanJungleRun() : base(Games.RaymanJungleRun) { }

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    protected override async Task LoadSlotsAsync()
    {
        FileSystemPath saveDir = Environment.SpecialFolder.LocalApplicationData.GetFolderPath() + "Packages" + Game.GetManager<GameManager_WinStore>().FullPackageName + "LocalState";

        for (int saveIndex = 0; saveIndex < 3; saveIndex++)
        {
            Logger.Info("Rayman Jungle Run slot {0} is being loaded...", saveIndex);

            // Get the file path
            FileSystemPath filePath = saveDir + $"slot{saveIndex + 1}.dat";

            // Make sure the file exists
            if (!filePath.FileExists)
            {
                Logger.Info("Slot was not loaded due to not being found");
                continue;
            }

            // Deserialize and return the data
            UbiArtSettings settings = UbiArtSettings.GetSaveSettings(UbiArtGame.RaymanJungleRun, Platform.PC);
            JungleRunPCSaveData saveData = await Task.Run(() => BinarySerializableHelpers.ReadFromFile<JungleRunPCSaveData>(filePath, settings, Services.App.GetBinarySerializerLogger(filePath.Name)));

            Logger.Info("Rayman Jungle Run slot has been deserialized");

            // Create the collection with items for each time trial level + general information
            List<ProgressionDataViewModel> progressItems = new();

            // Get data values
            int collectedLums = 0;
            int availableLums = 0;
            int collectedTeeth = 0;
            int availableTeeth = saveData.Levels.Length;

            // Enumerate each level
            for (int lvl = 0; lvl < saveData.Levels.Length; lvl++)
            {
                // Get the level data
                JungleRunPCSaveDataLevel levelData = saveData.Levels[lvl];

                // Check if the level is a normal level
                if ((lvl + 1) % 10 != 0)
                {
                    Logger.Trace("Level index {0} is a normal level", lvl);

                    // Get the collected lums
                    collectedLums += levelData.LumsRecord;
                    availableLums += 100;

                    Logger.Trace("{0} Lums have been collected", levelData.LumsRecord);

                    // Check if the level is 100% complete
                    if (levelData.LumsRecord >= 100)
                        collectedTeeth++;

                    continue;
                }

                Logger.Trace("Level index {0} is a time trial level", lvl);

                // Make sure the level has been completed
                if (levelData.RecordTime == 0)
                {
                    Logger.Trace("Level has not been completed");

                    continue;
                }

                Logger.Trace("Level has been completed with the record time {0}", levelData.RecordTime);

                collectedTeeth++;

                // Get the level number, starting at 10
                string fullLevelNumber = (lvl + 11).ToString();

                // Get the world and level numbers
                var worldNum = fullLevelNumber[0].ToString();
                var lvlNum = fullLevelNumber[1].ToString();

                // If the level is 0, correct the numbers to be level 10
                if (lvlNum == "0")
                {
                    worldNum = (Int32.Parse(worldNum) - 1).ToString();
                    lvlNum = "10";
                }

                // Add the item
                progressItems.Add(new ProgressionDataViewModel(false, GameProgression_Icon.RO_Clock, new ConstLocString($"{worldNum}-{lvlNum}: {new TimeSpan(0, 0, 0, 0, (int)levelData.RecordTime):mm\\:ss\\.fff}")));
            }

            // Add general progress info first
            progressItems.Insert(0, new ProgressionDataViewModel(true, GameProgression_Icon.RO_Lum, new ConstLocString($"{collectedLums}/{availableLums}")));
            progressItems.Insert(1, new ProgressionDataViewModel(true, GameProgression_Icon.RO_RedTooth, new ConstLocString($"{collectedTeeth}/{availableTeeth}")));

            Slots.Add(new ProgressionSlotViewModel(null, saveIndex, collectedLums + collectedTeeth, availableLums + availableTeeth, progressItems)
            {
                FilePath = filePath
            });

            Logger.Info("Rayman Jungle Run slot has been loaded");
        }
    }
}