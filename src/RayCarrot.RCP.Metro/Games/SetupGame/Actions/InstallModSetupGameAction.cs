﻿using System.Net.Http;
using RayCarrot.RCP.Metro.ModLoader.Library;
using RayCarrot.RCP.Metro.ModLoader.Sources.GameBanana;

namespace RayCarrot.RCP.Metro.Games.SetupGame;

public abstract class InstallModSetupGameAction : SetupGameAction
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    protected abstract long GameBananaModId { get; }
    protected abstract string ModId { get; }

    public override GenericIconKind FixActionIcon => GenericIconKind.SetupGame_Mod;
    public override LocalizedString? FixActionDisplayName => "Download mod"; // TODO-LOC

    public override bool CheckIsComplete(GameInstallation gameInstallation)
    {
        try
        {
            ModLibrary library = new(gameInstallation);
            ModManifest modManifest = library.ReadModManifest();
            return modManifest.Mods.TryGetValue(ModId, out ModManifestEntry entry) && entry.IsEnabled;
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Checking if mod is installed");
            return false;
        }
    }

    public override async Task FixAsync(GameInstallation gameInstallation)
    {
        GameBananaModsSource gb = new();
        GameBananaFile? file;

        try
        {
            using HttpClient httpClient = new();

            // Get the mod
            GameBananaMod mod = await httpClient.GetDeserializedAsync<GameBananaMod>(
                $"https://gamebanana.com/apiv11/Mod/{GameBananaModId}?" +
                $"_csvProperties=_aFiles,_aModManagerIntegrations");

            if (mod.Files == null)
            {
                // TODO-LOC
                await Services.MessageUI.DisplayMessageAsync("No valid files were found for the mod", MessageType.Error);
                return;
            }

            // Get the most recent file
            file = gb.GetValidFiles(mod, mod.Files).OrderBy(x => x.DateAdded).LastOrDefault();

            if (file == null)
            {
                // TODO-LOC
                await Services.MessageUI.DisplayMessageAsync("No valid files were found for the mod", MessageType.Error);
                return;
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Getting mod download for setup game action");

            // TODO-LOC
            await Services.MessageUI.DisplayExceptionMessageAsync(ex, "An error occurred when getting the download for the mod");

            return;
        }

        await Services.UI.ShowModLoaderAsync(gameInstallation, file.DownloadUrl, file.File, gb.Id, new GameBananaInstallData(GameBananaModId, file.Id));
    }
}