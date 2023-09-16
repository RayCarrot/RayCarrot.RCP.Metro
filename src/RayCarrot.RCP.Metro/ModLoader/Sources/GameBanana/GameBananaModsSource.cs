﻿using System.Net.Http;
using Newtonsoft.Json.Linq;
using RayCarrot.RCP.Metro.Games.Components;
using RayCarrot.RCP.Metro.ModLoader.Dialogs.ModLoader;
using RayCarrot.RCP.Metro.ModLoader.Library;
using RayCarrot.RCP.Metro.ModLoader.Metadata;

namespace RayCarrot.RCP.Metro.ModLoader.Sources.GameBanana;

public class GameBananaModsSource : DownloadableModsSource
{
    #region Constant Fields

    private const int RaymanControlPanelToolId = 10372;

    #endregion

    #region Public Properties

    public override string Id => "GameBanana";
    public override ModSourceIconAsset Icon => ModSourceIconAsset.GameBanana;

    #endregion

    #region Public Methods

    public override async Task LoadDownloadableModsAsync(
        ModLoaderViewModel modLoaderViewModel,
        HttpClient httpClient, 
        GameInstallation gameInstallation, 
        ObservableCollection<DownloadableModViewModel> modsCollection)
    {
        List<GameBananaRecord> modRecords = new();

        // TODO-UPDATE: Maybe we should load in batches if there are too many mods? Pages?

        // Enumerate every supported GameBanana game
        foreach (GameBananaGameComponent gameBananaGameComponent in gameInstallation.GetComponents<GameBananaGameComponent>())
        {
            int gameId = gameBananaGameComponent.GameId;

            GameBananaSubfeed subfeed;
            int page = 1;

            // Add every mod from every available page. This might become inefficient if there
            // are a lot of mods to load, but currently it's not a problem.
            do
            {
                // Read the subfeed page
                subfeed = await httpClient.GetDeserializedAsync<GameBananaSubfeed>(
                    $"https://gamebanana.com/apiv11/Game/{gameId}/Subfeed?" +
                    $"_nPage={page}&" +
                    $"_sSort=new&" +
                    $"_csvModelInclusions=Mod");

                // Add the mods
                modRecords.AddRange(subfeed.Records.Where(x => x.HasFiles));

                page++;
            } while (!subfeed.Metadata.IsComplete);
        }

        if (modRecords.Count == 0)
            return;

        // Get data for every mod
        GameBananaMod[] mods = await httpClient.GetDeserializedAsync<GameBananaMod[]>(
            $"https://gamebanana.com/apiv11/Mod/Multi?" +
            $"_csvRowIds={modRecords.Select(x => x.Id).JoinItems(",")}&" +
            $"_csvProperties=_aFiles,_sDescription,_sText,_nDownloadCount,_aModManagerIntegrations");

        // Process every mod
        for (int i = 0; i < modRecords.Count; i++)
        {
            GameBananaRecord modRecord = modRecords[i];
            GameBananaMod mod = mods[i];

            // Make sure the mod has files
            if (mod.Files == null)
                continue;

            // Get the files which contain valid RCP mods
            List<GameBananaFile> validFiles = mod.Files.
                Where(x => mod.ModManagerIntegrations is JObject obj &&
                           obj.ToObject<Dictionary<string, GameBananaModManager[]>>()?.TryGetValue(x.Id.ToString(), out GameBananaModManager[] m) == true &&
                           m.Any(mm => mm.ToolId == RaymanControlPanelToolId)).
                ToList();

            // Make sure at least one file has mod integration with RCP
            if (validFiles.Count > 0)
                modsCollection.Add(new GameBananaModViewModel(
                    downloadableModsSource: this,
                    modLoaderViewModel: modLoaderViewModel,
                    gameBananaId: modRecord.Id,
                    name: modRecord.Name,
                    uploaderUserName: modRecord.Submitter?.Name ?? String.Empty,
                    uploadDate: modRecord.DateAdded,
                    description: mod.Description ?? String.Empty,
                    text: mod.Text ?? String.Empty,
                    previewMedia: modRecord.PreviewMedia,
                    likesCount: modRecord.LikeCount,
                    downloadsCount: mod.DownloadCount,
                    viewsCount: modRecord.ViewCount,
                    files: validFiles));
        }
    }

    public override async Task<ModUpdateCheckResult> CheckForUpdateAsync(HttpClient httpClient, ModInstallInfo modInstallInfo)
    {
        long gameBananaModId = modInstallInfo.GetRequiredInstallData<GameBananaInstallData>().ModId;

        GameBananaMod gameBananaMod = await httpClient.GetDeserializedAsync<GameBananaMod>(
            $"https://gamebanana.com/apiv11/Mod/{gameBananaModId}/ProfilePage");

        ModVersion localVersion = modInstallInfo.Version ?? new ModVersion(1, 0, 0);

        if (gameBananaMod.Version.IsNullOrWhiteSpace())
        {
            // TODO-UPDATE: Localize
            return new ModUpdateCheckResult(ModUpdateState.UnableToCheckForUpdates,
                "Unable to check for updates due to there not being a version to compare against");
        }

        if (gameBananaMod.Files == null || !gameBananaMod.Files.Any(x =>
                x.ModManagerIntegrations != null &&
                x.ModManagerIntegrations.Any(m => m.ToolId == RaymanControlPanelToolId)))
        {
            // TODO-UPDATE: Localize
            return new ModUpdateCheckResult(ModUpdateState.UnableToCheckForUpdates,
                "Unable to check for updates due to the GameBanana mod not having any valid files");
        }

        ModVersion onlineVersion;

        try
        {
            onlineVersion = ModVersion.Parse(gameBananaMod.Version);
        }
        catch (Exception ex)
        {
            // TODO-UPDATE: Log exception

            // TODO-UPDATE: Localize
            return new ModUpdateCheckResult(ModUpdateState.UnableToCheckForUpdates,
                "Unable to check for updates due to the version not being formatted correctly");
        }

        if (onlineVersion > localVersion)
        {
            // No need to pass in a message here as it shows a button then instead
            return new ModUpdateCheckResult(ModUpdateState.UpdateAvailable, String.Empty, gameBananaMod);
        }
        else
        {
            // TODO-UPDATE: Localize
            return new ModUpdateCheckResult(ModUpdateState.UpToDate, "The mod is up to date");
        }
    }

    public override Task<ModDownload> GetModUpdateDownloadAsync(object? updateData)
    {
        if (updateData is not GameBananaMod gameBananaMod)
            throw new ArgumentException("The update data is not of the correct type", nameof(updateData));

        if (gameBananaMod.Files == null)
            throw new Exception("Files is null");

        List<GameBananaFile> validFiles = gameBananaMod.Files.
            Where(x => x.ModManagerIntegrations != null && x.ModManagerIntegrations.Any(m => m.ToolId == RaymanControlPanelToolId)).
            ToList();

        if (validFiles.Count == 0)
            throw new Exception("There are no valid files");

        GameBananaFile file;

        if (validFiles.Count > 1)
        {
            // TODO-UPDATE: Have user pick download
            file = validFiles[0];
        }
        else
        {
            file = validFiles[0];
        }

        return Task.FromResult(new ModDownload(file.File, file.DownloadUrl, file.FileSize, new GameBananaInstallData(gameBananaMod.Id, file.Id)));
    }

    #endregion
}