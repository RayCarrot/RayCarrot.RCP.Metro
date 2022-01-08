using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BinarySerializer;
using BinarySerializer.UbiArt;
using NLog;

namespace RayCarrot.RCP.Metro;

public class ProgressionGameViewModel_RaymanFiestaRun : ProgressionGameViewModel
{
    public ProgressionGameViewModel_RaymanFiestaRun(UserData_FiestaRunEdition edition, string displayName) : base(Games.RaymanFiestaRun, displayName)
    {
        Edition = edition;
    }

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    public UserData_FiestaRunEdition Edition { get; }

    protected override string BackupName => $"Rayman Fiesta Run ({Edition})";
    protected override GameBackups_Directory[] BackupDirectories => GameManager_WinStore.GetWinStoreBackupDirs(Game.GetManager<GameManager_RaymanFiestaRun_WinStore>(GameType.WinStore).GetFiestaRunFullPackageName(Edition));

    private int GetLevelIdFromIndex(int idx)
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
    protected override async IAsyncEnumerable<ProgressionSlotViewModel> LoadSlotsAsync(FileSystemWrapper fileSystem)
    {
        // For now only the Windows 10 Edition and Preload versions supported
        if (Edition != UserData_FiestaRunEdition.Win10 && Edition != UserData_FiestaRunEdition.Preload)
            yield break;

        FileSystemPath dirPath = Environment.SpecialFolder.LocalApplicationData.GetFolderPath() +
                                 "Packages" +
                                 Game.GetManager<GameManager_RaymanFiestaRun_WinStore>(GameType.WinStore).GetFiestaRunFullPackageName(Edition) +
                                 "LocalState";
        IOSearchPattern? saveDir = fileSystem.GetDirectory(new IOSearchPattern(dirPath, SearchOption.TopDirectoryOnly, "*.dat"));

        if (saveDir is null)
            yield break;

        using RCPContext context = new(saveDir.DirPath);
        UbiArtSettings settings = new(BinarySerializer.UbiArt.Game.RaymanFiestaRun, Platform.PC);
        context.AddSettings(settings);

        Logger.Info("{0} slot is being loaded...", Game);

        // Get the file path
        string fileName = Edition == UserData_FiestaRunEdition.Win10 ? "slot0.dat" : "slot1.dat";

        // Deserialize the data
        FiestaRun_SaveData? saveData = await context.ReadFileDataAsync<FiestaRun_SaveData>(fileName, endian: Endian.Little);

        if (saveData == null)
        {
            Logger.Info("{0} slot was not found", Game);
            yield break;
        }

        Logger.Info("{0} slot has been deserialized", Game);

        int crowns = saveData.LevelInfos_Land1.Count(x => x.HasCrown);
        int maxCrowns = 72;

        List<ProgressionDataViewModel> progressItems = new()
        {
            // TODO-UPDATE: Localize
            new ProgressionDataViewModel(true, ProgressionIcon.RFR_Crown, new ConstLocString("Crowns"), crowns, maxCrowns),
        };

        if (saveData.Version >= 2)
        {
            crowns += saveData.LevelInfos_Land2.Count(x => x.HasCrown);
            maxCrowns += 16;

            // TODO-UPDATE: Localize
            progressItems.Add(new ProgressionDataViewModel(true, ProgressionIcon.RFR_Nightmare, new ConstLocString("Nightmare mode"), GetLevelIdFromIndex(saveData.MaxNightMareLevelIdx % 100), 36));
        }

        // TODO-UPDATE: Localize
        progressItems.Add(new ProgressionDataViewModel(false, ProgressionIcon.RL_Lum, new ConstLocString("Lums"), (int)saveData.LumsGlobalCounter));

        // Add Livid Dead times
        for (int lvlIndex = 0; lvlIndex < saveData.LevelTimes.Length; lvlIndex++)
        {
            if (saveData.LevelTimes[lvlIndex] == 0)
                continue;

            // Add the item
            progressItems.Add(new ProgressionDataViewModel(
                isPrimaryItem: false,
                icon: ProgressionIcon.RO_Clock,
                header: new ConstLocString($"{lvlIndex + 1}"), // TODO-UPDATE: Level name
                text: new ConstLocString($"{new TimeSpan(0, 0, 0, 0, (int)saveData.LevelTimes[lvlIndex]):mm\\:ss\\.fff}")));
        }

        yield return new SerializableProgressionSlotViewModel<FiestaRun_SaveData>(this, null, 0, crowns, maxCrowns, progressItems, context, saveData, fileName);

        Logger.Info("{0} slot has been loaded", Game);
    }
}