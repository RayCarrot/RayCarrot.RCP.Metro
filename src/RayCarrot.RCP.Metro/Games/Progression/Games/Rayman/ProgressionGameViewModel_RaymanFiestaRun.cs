﻿using System;
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
        // For now only the Windows 10 Edition is supported. The other versions use older save versions.
        if (Edition != UserData_FiestaRunEdition.Win10)
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
        const string fileName = $"slot0.dat";

        // Deserialize the data
        FiestaRun_SaveData? saveData = await context.ReadFileDataAsync<FiestaRun_SaveData>(fileName, endian: Endian.Little);

        if (saveData == null)
        {
            Logger.Info("{0} slot was not found", Game);
            yield break;
        }

        Logger.Info("{0} slot has been deserialized", Game);

        int crowns = saveData.LevelInfos_Land1.Concat(saveData.LevelInfos_Land2).Count(x => x.HasCrown);
        const int maxCrowns = 72 + 16;

        // TODO-UPDATE: Localize
        ProgressionDataViewModel[] progressItems = 
        {
            new ProgressionDataViewModel(true, ProgressionIcon.RFR_Crown, new ConstLocString("Crowns"), crowns, maxCrowns),
            new ProgressionDataViewModel(true, ProgressionIcon.RFR_Nightmare, new ConstLocString("Nightmare mode"), GetLevelIdFromIndex(saveData.MaxNightMareLevelIdx % 100), 36),
            new ProgressionDataViewModel(false, ProgressionIcon.RL_Lum, new ConstLocString("Lums"), (int)saveData.LumsGlobalCounter),
        };

        yield return new SerializableProgressionSlotViewModel<FiestaRun_SaveData>(this, null, 0, crowns, maxCrowns, progressItems, context, saveData, fileName);

        Logger.Info("{0} slot has been loaded", Game);
    }
}