using System;
using System.Collections.Generic;
using System.IO;
using BinarySerializer;
using BinarySerializer.UbiArt;
using NLog;

namespace RayCarrot.RCP.Metro;

public class ProgressionGameViewModel_RaymanJungleRun : ProgressionGameViewModel
{
    public ProgressionGameViewModel_RaymanJungleRun() : base(Games.RaymanJungleRun) { }

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    protected override GameBackups_Directory[] BackupDirectories => GameManager_WinStore.GetWinStoreBackupDirs(Game.GetManager<GameManager_WinStore>().FullPackageName);

    protected override async IAsyncEnumerable<ProgressionSlotViewModel> LoadSlotsAsync(FileSystemWrapper fileSystem)
    {
        FileSystemPath dirPath = Environment.SpecialFolder.LocalApplicationData.GetFolderPath() + 
                                 "Packages" + 
                                 Game.GetManager<GameManager_WinStore>().FullPackageName + 
                                 "LocalState";
        IOSearchPattern? saveDir = fileSystem.GetDirectory(new IOSearchPattern(dirPath, SearchOption.TopDirectoryOnly, "*.dat"));

        if (saveDir is null)
            yield break;

        using RCPContext context = new(saveDir.DirPath);
        UbiArtSettings settings = new(BinarySerializer.UbiArt.Game.RaymanJungleRun, Platform.PC);
        context.AddSettings(settings);

        for (int saveIndex = 0; saveIndex < 3; saveIndex++)
        {
            Logger.Info("{0} slot {1} is being loaded...", Game, saveIndex);

            // Get the file path
            string fileName = $"slot{saveIndex + 1}.dat";

            // Deserialize the data
            JungleRun_SaveData? saveData = await context.ReadFileDataAsync<JungleRun_SaveData>(fileName, endian: Endian.Little, removeFileWhenComplete: false);

            if (saveData == null)
            {
                Logger.Info("{0} slot was not found", Game);
                continue;
            }

            Logger.Info("{0} slot has been deserialized", Game);

            // Create the collection with items for each time trial level + general information
            List<ProgressionDataViewModel> progressItems = new();

            // Default number of worlds to 5 as it is no longer possible to download the additional 2 ones on PC
            int numWorlds = 5;

            int lums = 0;
            int teeth = 0;

            // Enumerate each level
            for (int lvl = 0; lvl < saveData.LevelInfos.Length; lvl++)
            {
                // Get the level data
                JungleRun_SaveDataLevel levelData = saveData.LevelInfos[lvl];

                // Check if the level is a normal level
                if ((lvl + 1) % 10 != 0)
                {
                    // Set number of worlds to 7 if lums are collected in one of the last two worlds
                    if (lvl >= 10 * 5 && levelData.LumsRecord > 0)
                        numWorlds = 7;

                    // Get the collected lums
                    lums += levelData.LumsRecord; 

                    // Check if the level is 100% complete
                    if (levelData.LumsRecord >= 100)
                        teeth++;

                    continue;
                }

                // Make sure the level has been completed
                if (levelData.RecordTime == 0)
                {
                    // Logger.Trace("Level has not been completed");

                    continue;
                }

                teeth++;

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
                progressItems.Add(new ProgressionDataViewModel(
                    isPrimaryItem: false, 
                    icon: ProgressionIcon.RO_Clock, 
                    header: $"{worldNum}-{lvlNum}", 
                    text: $"{new TimeSpan(0, 0, 0, 0, (int)levelData.RecordTime):mm\\:ss\\.fff}"));
            }

            int maxLums = numWorlds * 9 * 100;
            int maxTeeth = numWorlds * 10;

            // Add general progress info first
            progressItems.Insert(0, new ProgressionDataViewModel(
                isPrimaryItem: true, 
                icon: ProgressionIcon.RO_Lum,
                header: new ResourceLocString(nameof(Resources.Progression_Lums)),
                value: lums, 
                max: maxLums));
            progressItems.Insert(1, new ProgressionDataViewModel(
                isPrimaryItem: true, 
                icon: ProgressionIcon.RO_RedTooth,
                header: new ResourceLocString(nameof(Resources.Progression_Teeth)),
                value: teeth, 
                max: maxTeeth));

            yield return new SerializableProgressionSlotViewModel<JungleRun_SaveData>(this, null, saveIndex, lums + teeth, maxLums + maxTeeth, progressItems, context, saveData, fileName);

            Logger.Info("{0} slot has been loaded", Game);
        }
    }
}