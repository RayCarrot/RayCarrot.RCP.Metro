﻿using System;
using System.Collections.Generic;
using NLog;
using RayCarrot.IO;
using RayCarrot.Rayman;
using RayCarrot.Rayman.UbiArt;

namespace RayCarrot.RCP.Metro;

public class ProgressionGameViewModel_RaymanJungleRun : ProgressionGameViewModel
{
    public ProgressionGameViewModel_RaymanJungleRun() : base(Games.RaymanJungleRun) { }

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    protected override GameBackups_Directory[] BackupDirectories => GameManager_WinStore.GetWinStoreBackupDirs(Game.GetManager<GameManager_WinStore>().FullPackageName);

    protected override async IAsyncEnumerable<ProgressionSlotViewModel> LoadSlotsAsync(FileSystemWrapper fileSystem)
    {
        FileSystemPath saveDir = Environment.SpecialFolder.LocalApplicationData.GetFolderPath() + "Packages" + Game.GetManager<GameManager_WinStore>().FullPackageName + "LocalState";

        for (int saveIndex = 0; saveIndex < 3; saveIndex++)
        {
            Logger.Info("{0} slot {1} is being loaded...", Game, saveIndex);

            // Get the file path
            FileSystemPath filePath = saveDir + $"slot{saveIndex + 1}.dat";

            // Deserialize the data
            UbiArtSettings settings = UbiArtSettings.GetSaveSettings(UbiArtGame.RaymanJungleRun, Platform.PC);
            JungleRunPCSaveData? saveData = await SerializeFileDataAsync<JungleRunPCSaveData>(fileSystem, filePath, settings);

            if (saveData == null)
            {
                Logger.Info("{0} slot was not found", Game);
                continue;
            }

            Logger.Info("{0} slot has been deserialized", Game);

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
                string worldNum = fullLevelNumber[0].ToString();
                string lvlNum = fullLevelNumber[1].ToString();

                // If the level is 0, correct the numbers to be level 10
                if (lvlNum == "0")
                {
                    worldNum = (Int32.Parse(worldNum) - 1).ToString();
                    lvlNum = "10";
                }

                // Add the item
                // TODO-UPDATE: Localize
                progressItems.Add(new ProgressionDataViewModel(
                    isPrimaryItem: false, 
                    icon: ProgressionIcon.RO_Clock, 
                    header: new ConstLocString($"{worldNum}-{lvlNum}"), 
                    text: new ConstLocString($"{new TimeSpan(0, 0, 0, 0, (int)levelData.RecordTime):mm\\:ss\\.fff}")));
            }

            // Add general progress info first
            // TODO-UPDATE: Localize
            progressItems.Insert(0, new ProgressionDataViewModel(
                isPrimaryItem: true, 
                icon: ProgressionIcon.RO_Lum, 
                header: new ConstLocString("Lums"), 
                value: collectedLums, 
                max: availableLums));
            progressItems.Insert(1, new ProgressionDataViewModel(
                isPrimaryItem: true, 
                icon: ProgressionIcon.RO_RedTooth, 
                header: new ConstLocString("Teeth"), 
                value: collectedTeeth, 
                max: availableTeeth));

            yield return new SerializableProgressionSlotViewModel<JungleRunPCSaveData>(this, null, saveIndex, collectedLums + collectedTeeth, availableLums + availableTeeth, progressItems, saveData, settings)
            {
                FilePath = filePath
            };

            Logger.Info("{0} slot has been loaded", Game);
        }
    }
}