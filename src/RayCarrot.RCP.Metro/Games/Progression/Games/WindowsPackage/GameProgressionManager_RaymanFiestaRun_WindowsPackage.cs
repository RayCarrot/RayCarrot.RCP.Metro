﻿using System.IO;
using BinarySerializer;
using BinarySerializer.UbiArt;

namespace RayCarrot.RCP.Metro;

public class GameProgressionManager_RaymanFiestaRun_WindowsPackage : GameProgressionManager
{
    public GameProgressionManager_RaymanFiestaRun_WindowsPackage(WindowsPackageGameDescriptor gameDescriptor, GameInstallation gameInstallation, string progressionId, int slotIndex) 
        : base(gameInstallation, progressionId)
    {
        GameDescriptor = gameDescriptor;
        SlotIndex = slotIndex;
    }

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    private int SlotIndex { get; }
    private WindowsPackageGameDescriptor GameDescriptor { get; }

    public override GameBackups_Directory[] BackupDirectories => GameDescriptor.GetBackupDirectories();

    private static int GetLevelIdFromIndex(int idx)
    {
        int v2 = idx + 1;
        if (v2 % 10 == 0)
            return v2 / 10 + 36;

        int v3 = idx % 100;
        if (idx % 100 > 8)
        {
            if (v3 > 18)
            {
                if (v3 <= 28)
                    return idx - 1;
                if (v3 <= 38)
                    return idx - 2;
            }
            v2 = idx;
        }
        return v2;
    }

    public override async IAsyncEnumerable<GameProgressionSlot> LoadSlotsAsync(FileSystemWrapper fileSystem)
    {
        FileSystemPath dirPath = GameDescriptor.GetLocalAppDataDirectory();
        IOSearchPattern? saveDir = fileSystem.GetDirectory(new IOSearchPattern(dirPath, SearchOption.TopDirectoryOnly, "*.dat"));

        if (saveDir is null)
            yield break;

        using RCPContext context = new(saveDir.DirPath);
        UbiArtSettings settings = new(BinarySerializer.UbiArt.Game.RaymanFiestaRun, Platform.PC);
        context.AddSettings(settings);

        Logger.Info("{0} slot is being loaded...", GameInstallation.FullId);

        // Get the file path
        string fileName = $"slot{SlotIndex}.dat";

        // Deserialize the data
        FiestaRun_SaveData? saveData = await context.ReadFileDataAsync<FiestaRun_SaveData>(fileName, endian: Endian.Little, removeFileWhenComplete: false);

        if (saveData == null)
        {
            Logger.Info("{0} slot was not found", GameInstallation.FullId);
            yield break;
        }

        Logger.Info("{0} slot has been deserialized", GameInstallation.FullId);

        int crowns = saveData.LevelInfos_Land1.Count(x => x.HasCrown);
        int maxCrowns = 72;

        List<GameProgressionDataItem> progressItems = new();

        if (saveData.Version >= 2)
        {
            crowns += saveData.LevelInfos_Land2.Count(x => x.HasCrown);
            maxCrowns += 16;
        }

        progressItems.Add(new GameProgressionDataItem(
            isPrimaryItem: true,
            icon: ProgressionIconAsset.RFR_Crown,
            header: new ResourceLocString(nameof(Resources.Progression_RFRCrowns)),
            value: crowns,
            max: maxCrowns));

        if (saveData.Version >= 2)
            progressItems.Add(new GameProgressionDataItem(
                isPrimaryItem: true,
                icon: ProgressionIconAsset.RFR_Nightmare,
                header: new ResourceLocString(nameof(Resources.Progression_RFRNightmareMode)),
                value: GetLevelIdFromIndex(saveData.MaxNightMareLevelIdx % 100),
                max: 36));

        progressItems.Add(new GameProgressionDataItem(
            isPrimaryItem: false,
            icon: ProgressionIconAsset.RL_Lum,
            header: new ResourceLocString(nameof(Resources.Progression_Lums)),
            value: (int)saveData.LumsGlobalCounter));

        // Add Livid Dead times
        for (int lvlIndex = 0; lvlIndex < saveData.LevelTimes.Length; lvlIndex++)
        {
            if (saveData.LevelTimes[lvlIndex] == 0)
                continue;

            // Add the item
            progressItems.Add(new GameProgressionDataItem(
                isPrimaryItem: false,
                icon: ProgressionIconAsset.RO_Clock,
                header: new ResourceLocString($"RFR_LevelName_{lvlIndex + 1}_10"),
                text: $"{new TimeSpan(0, 0, 0, 0, (int)saveData.LevelTimes[lvlIndex]):mm\\:ss\\.fff}"));
        }

        yield return new SerializableGameProgressionSlot<FiestaRun_SaveData>(null, 0, crowns, maxCrowns, progressItems, context, saveData, fileName);

        Logger.Info("{0} slot has been loaded", GameInstallation.FullId);
    }
}