﻿using System;
using System.Collections.Generic;
using System.Linq;
using NLog;

namespace RayCarrot.RCP.Metro;

public class ProgressionGameViewModel_RabbidsBigBang : ProgressionGameViewModel
{
    public ProgressionGameViewModel_RabbidsBigBang(WindowsPackageGameDescriptor gameDescriptor, GameInstallation gameInstallation) 
        : base(gameInstallation)
    {
        GameDescriptor = gameDescriptor;
    }

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    private WindowsPackageGameDescriptor GameDescriptor { get; }

    protected override GameBackups_Directory[] BackupDirectories => GameDescriptor.GetBackupDirectories();

    protected override async IAsyncEnumerable<ProgressionSlotViewModel> LoadSlotsAsync(FileSystemWrapper fileSystem)
    {
        FileSystemPath saveFile = fileSystem.GetFile(GameDescriptor.GetLocalAppDataDirectory() + "playerprefs.dat");

        using RCPContext context = new(saveFile.Parent);

        Logger.Info("{0} save is being loaded...", GameInstallation.Id);

        Unity_PlayerPrefs? saveData = await context.ReadFileDataAsync<Unity_PlayerPrefs>(saveFile.Name, removeFileWhenComplete: false);

        if (saveData == null)
        {
            Logger.Info("{0} save was not found", GameInstallation.Id);
            yield break;
        }

        Logger.Info("{0} save has been deserialized", GameInstallation.Id);

        const int maxScore = 12 * 45 * 3;

        int score = 0;

        for (int worldIndex = 0; worldIndex < 12; worldIndex++)
        {
            for (int levelIndex = 0; levelIndex < 45; levelIndex++)
            {
                Unity_PlayerPrefsEntry? entry = saveData.Entries.FirstOrDefault(x => x.Key == $"MissionComplete_{worldIndex + 1}_{levelIndex}");

                if (entry != null)
                    score += entry.IntValue;
            }
        }

        ProgressionDataViewModel[] progressItems =
        {
            new ProgressionDataViewModel(
                isPrimaryItem: true, 
                icon: ProgressionIcon.RabbidsBigBang_Score, 
                header: new ResourceLocString(nameof(Resources.Progression_RabbidsBigBangStars)), 
                value: score, 
                max: maxScore),
        };

        yield return new SerializableProgressionSlotViewModel<Unity_PlayerPrefs>(this, null, 0, score, maxScore, progressItems, context, saveData, saveFile.Name);

        Logger.Info("{0} save has been loaded", GameInstallation.Id);
    }
}