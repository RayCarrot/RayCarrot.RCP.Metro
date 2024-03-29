﻿using System.IO;
using BinarySerializer.UbiArt;
using RayCarrot.RCP.Metro.Games.Components;

namespace RayCarrot.RCP.Metro.ModLoader.Modules.UbiArtLocalization;

public class UbiArtLocalizationModule : ModModule
{
    private static string[] Locales { get; } = 
    { 
        "en-US",
        "fr-FR",
        "ja-JP",
        "de-DE",
        "es-ES",
        "it-IT",
        "ko-KR",
        "zh-TW",
        "pt-PT",
        "zh-CN",
        "pl-PL",
        "ru-RU",
        "nl-NL",
        "da-DK",
        "nb-NO",
        "sv-SE",
        "fi-FI",
        "pt-BR",
        "ms-MY",
        "id-ID",
        "tr-TR",
        "ar-SA",
        "ta-IN",
        "th-TH",
    };

    public override string Id => "ubiart-loc";
    public override LocalizedString Description => new ResourceLocString(nameof(Resources.ModLoader_UbiArtLocalizationModule_Description));

    public override void SetupModuleFolder(ModModuleViewModel viewModel, FileSystemPath modulePath)
    {
        File.Create(modulePath + "en-US.txt").Dispose();
    }

    public override IReadOnlyCollection<IFilePatch> GetPatchedFiles(Mod mod, FileSystemPath modulePath)
    {
        List<UbiArtLocalizationFilePatch.LocaleFile> localeFiles = new();

        foreach (FileSystemPath filePath in Directory.GetFiles(modulePath, "*.txt", SearchOption.TopDirectoryOnly))
        {
            string fileName = filePath.RemoveFileExtension().Name;
            int localeId = Locales.FindItemIndex(x => x.Equals(fileName, StringComparison.OrdinalIgnoreCase));

            if (localeId != -1)
                localeFiles.Add(new UbiArtLocalizationFilePatch.LocaleFile(localeId, filePath));
        }

        if (localeFiles.Count == 0)
            return Array.Empty<IFilePatch>();

        BinaryGameModeComponent gameModeComponent = mod.GameInstallation.GetRequiredComponent<BinaryGameModeComponent>();

        if (gameModeComponent.GameModeAttribute.GetSettingsObject() is not UbiArtSettings ubiArtSettings)
            throw new Exception($"The settings object provided by the corresponding game mode {gameModeComponent.GameMode} is not of the correct type");

        // TODO: Add support for Origins and maybe Fiesta Run
        ModFilePath locFilePath = ubiArtSettings.Game switch
        {
            BinarySerializer.UbiArt.Game.RaymanLegends when ubiArtSettings.Platform is Platform.PC => 
                new ModFilePath(@"EngineData\Localisation\localisation.loc8"),
            _ => throw new Exception($"Unsupported game {ubiArtSettings.Game}"),
        };

        return new IFilePatch[]
        {
            new UbiArtLocalizationFilePatch(mod.GameInstallation, locFilePath, localeFiles)
        };
    }
}