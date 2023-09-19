using System.Net.Http;
using Newtonsoft.Json.Linq;
using RayCarrot.RCP.Metro.Games.Components;
using RayCarrot.RCP.Metro.ModLoader.Dialogs.ModLoader;
using RayCarrot.RCP.Metro.ModLoader.Library;
using RayCarrot.RCP.Metro.ModLoader.Metadata;

namespace RayCarrot.RCP.Metro.ModLoader.Sources.GameBanana;

// TODO-UPDATE: Add logging
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

    public override async Task<DownloadableModsFeed> LoadDownloadableModsAsync(
        ModLoaderViewModel modLoaderViewModel,
        HttpClient httpClient, 
        GameInstallation gameInstallation,
        int page)
    {
        List<GameBananaRecord> modRecords = new();

        int largestPageCount = 0;

        // Enumerate every supported GameBanana game
        foreach (GameBananaGameComponent gameBananaGameComponent in gameInstallation.GetComponents<GameBananaGameComponent>())
        {
            int gameId = gameBananaGameComponent.GameId;

            // Read the subfeed page
            GameBananaSubfeed subfeed = await httpClient.GetDeserializedAsync<GameBananaSubfeed>(
                $"https://gamebanana.com/apiv11/Game/{gameId}/Subfeed?" +
                $"_nPage={page + 1}&" +
                $"_sSort=new&" +
                $"_csvModelInclusions=Mod");

            // Get the page count
            int pageCount = (int)Math.Ceiling(subfeed.Metadata.RecordCount / (double)subfeed.Metadata.PerPage);
            if (pageCount > largestPageCount)
                largestPageCount = pageCount;

            // Add the mods
            modRecords.AddRange(subfeed.Records.Where(x => x.HasFiles));
        }

        if (modRecords.Count == 0)
            return new DownloadableModsFeed(Array.Empty<DownloadableModViewModel>(), largestPageCount);

        // Get data for every mod
        GameBananaMod[] mods = await httpClient.GetDeserializedAsync<GameBananaMod[]>(
            $"https://gamebanana.com/apiv11/Mod/Multi?" +
            $"_csvRowIds={modRecords.Select(x => x.Id).JoinItems(",")}&" +
            $"_csvProperties=_aFiles,_sDescription,_sText,_nDownloadCount,_aModManagerIntegrations");

        List<GameBananaModViewModel> viewModels = new();

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
                viewModels.Add(new GameBananaModViewModel(
                    downloadableModsSource: this,
                    modLoaderViewModel: modLoaderViewModel,
                    gameBananaId: modRecord.Id,
                    name: modRecord.Name,
                    uploaderUserName: modRecord.Submitter?.Name ?? String.Empty,
                    uploadDate: modRecord.DateAdded,
                    description: mod.Description ?? String.Empty,
                    text: mod.Text ?? String.Empty,
                    version: modRecord.Version ?? String.Empty,
                    previewMedia: modRecord.PreviewMedia,
                    likesCount: modRecord.LikeCount,
                    downloadsCount: mod.DownloadCount,
                    viewsCount: modRecord.ViewCount,
                    files: validFiles));
        }

        return new DownloadableModsFeed(viewModels, largestPageCount);
    }

    public override async Task<ModUpdateCheckResult> CheckForUpdateAsync(HttpClient httpClient, ModInstallInfo modInstallInfo)
    {
        long gameBananaModId = modInstallInfo.GetRequiredInstallData<GameBananaInstallData>().ModId;

        GameBananaMod gameBananaMod = await httpClient.GetDeserializedAsync<GameBananaMod>(
            $"https://gamebanana.com/apiv11/Mod/{gameBananaModId}/ProfilePage");

        ModVersion localVersion = modInstallInfo.Version ?? new ModVersion(1, 0, 0);

        if (gameBananaMod.Version.IsNullOrWhiteSpace())
        {
            // TODO-LOC
            return new ModUpdateCheckResult(ModUpdateState.UnableToCheckForUpdates,
                "Unable to check for updates due to there not being a version to compare against");
        }

        if (gameBananaMod.Files == null || !gameBananaMod.Files.Any(x =>
                x.ModManagerIntegrations != null &&
                x.ModManagerIntegrations.Any(m => m.ToolId == RaymanControlPanelToolId)))
        {
            // TODO-LOC
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

            // TODO-LOC
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
            // TODO-LOC
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

