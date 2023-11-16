using System.Net.Http;
using Newtonsoft.Json.Linq;
using RayCarrot.RCP.Metro.Games.Components;
using RayCarrot.RCP.Metro.ModLoader.Dialogs.ModLoader;
using RayCarrot.RCP.Metro.ModLoader.Library;
using RayCarrot.RCP.Metro.ModLoader.Metadata;
using RayCarrot.RCP.Metro.Pages.Games;

namespace RayCarrot.RCP.Metro.ModLoader.Sources.GameBanana;

public class GameBananaModsSource : DownloadableModsSource
{
    #region Constant Fields

    private const int RaymanControlPanelToolId = 10372;

    #endregion

    #region Logger

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    #endregion

    #region Public Properties

    public override string Id => "GameBanana";
    public override LocalizedString DisplayName => new ResourceLocString(nameof(Resources.ModLoader_GameBanana_Title));
    public override ModSourceIconAsset Icon => ModSourceIconAsset.GameBanana;

    #endregion

    #region Private Methods

    private List<GameBananaFile> GetValidFiles(GameBananaMod mod, GameBananaFile[] files)
    {
        return files.
            Where(x => mod.ModManagerIntegrations is JObject obj &&
                       obj.ToObject<Dictionary<string, GameBananaModManager[]>>()?.TryGetValue(x.Id.ToString(), out GameBananaModManager[] m) == true &&
                       m.Any(mm => mm.ToolId == RaymanControlPanelToolId)).
            ToList();
    }

    private async Task LoadFeaturedModsAsync(
        ModLoaderViewModel modLoaderViewModel,
        HttpClient httpClient,
        GameInstallation gameInstallation,
        Dictionary<int, int[]> featuredMods,
        List<GameBananaDownloadableModViewModel> modViewModels)
    {
        List<int> modIds = new();

        foreach (GameBananaGameComponent gameBananaComponent in gameInstallation.GetComponents<GameBananaGameComponent>())
        {
            if (featuredMods.TryGetValue(gameBananaComponent.GameId, out int[] ids))
            {
                modIds.AddRange(ids);
            }
        }

        if (modIds.Count == 0)
            return;

        // Get data for every mod
        GameBananaMod[] mods = await httpClient.GetDeserializedAsync<GameBananaMod[]>(
            $"https://gamebanana.com/apiv11/Mod/Multi?" +
            $"_csvRowIds={modIds.JoinItems(",")}&" +
            $"_csvProperties=_idRow,_sName,_aSubmitter,_tsDateAdded,_sVersion,_aPreviewMedia,_nLikeCount,_nViewCount,_aFiles,_sDescription,_sText,_nDownloadCount,_aModManagerIntegrations");

        // Process every mod
        foreach (GameBananaMod mod in mods)
        {
            // Make sure the mod has files
            if (mod.Files == null)
                continue;

            // Get the files which contain valid RCP mods
            List<GameBananaFile> validFiles = GetValidFiles(mod, mod.Files);

            // Make sure at least one file has mod integration with RCP
            if (validFiles.Count > 0)
                modViewModels.Add(new GameBananaDownloadableModViewModel(
                    downloadableModsSource: this,
                    modLoaderViewModel: modLoaderViewModel,
                    gameBananaId: mod.Id,
                    name: mod.Name ?? String.Empty,
                    uploaderUserName: mod.Submitter?.Name ?? String.Empty,
                    uploaderUrl: mod.Submitter?.ProfileUrl,
                    uploadDate: mod.DateAdded,
                    description: mod.Description ?? String.Empty,
                    text: mod.Text ?? String.Empty,
                    version: mod.Version ?? String.Empty,
                    previewMedia: mod.PreviewMedia,
                    likesCount: mod.LikeCount,
                    downloadsCount: mod.DownloadCount,
                    viewsCount: mod.ViewCount,
                    files: validFiles,
                    isFeatured: true));
        }
    }

    #endregion

    #region Public Methods

    public override async Task<DownloadableModsFeed> LoadDownloadableModsAsync(
        ModLoaderViewModel modLoaderViewModel,
        IReadOnlyCollection<DownloadableModViewModel> loadedDownloadableMods,
        HttpClient httpClient,
        GameInstallation gameInstallation,
        int page)
    {
        List<GameBananaDownloadableModViewModel> modViewModels = new();

        // Only load features mods on first page
        if (page == 0)
        {
            Logger.Info("Loading featured GameBanana mods");

            Dictionary<int, int[]>? featuredMods = null;
            try
            {
                featuredMods = await JsonHelpers.DeserializeFromURLAsync<Dictionary<int, int[]>>(AppURLs.ModLoader_FeaturedGameBananaMods_URL);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Loading featured GameBanana mods");
            }

            // Load the featured mods first
            if (featuredMods != null)
                await LoadFeaturedModsAsync(modLoaderViewModel, httpClient, gameInstallation, featuredMods, modViewModels);
        }

        Logger.Info("Loading downloadable GameBanana mods");

        List<GameBananaRecord> modRecords = new();

        int largestPageCount = 0;

        // Enumerate every supported GameBanana game
        foreach (GameBananaGameComponent gameBananaGameComponent in gameInstallation.GetComponents<GameBananaGameComponent>())
        {
            int gameId = gameBananaGameComponent.GameId;

            Logger.Info("Loading mods using GameBanana game id {0}", gameId);

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
            modRecords.AddRange(subfeed.Records.Where(x => 
                x.HasFiles && 
                !loadedDownloadableMods.Any(m => m is GameBananaDownloadableModViewModel vm && vm.GameBananaId == x.Id) &&
                !modViewModels.Any(vm => vm.GameBananaId == x.Id)));
        }

        Logger.Info("{0} mods found", modRecords.Count);

        if (modRecords.Count == 0)
            return new DownloadableModsFeed(modViewModels, largestPageCount);

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
            List<GameBananaFile> validFiles = GetValidFiles(mod, mod.Files);

            // Make sure at least one file has mod integration with RCP
            if (validFiles.Count > 0)
                modViewModels.Add(new GameBananaDownloadableModViewModel(
                    downloadableModsSource: this,
                    modLoaderViewModel: modLoaderViewModel,
                    gameBananaId: modRecord.Id,
                    name: modRecord.Name,
                    uploaderUserName: modRecord.Submitter?.Name ?? String.Empty,
                    uploaderUrl: modRecord.Submitter?.ProfileUrl,
                    uploadDate: modRecord.DateAdded,
                    description: mod.Description ?? String.Empty,
                    text: mod.Text ?? String.Empty,
                    version: modRecord.Version ?? String.Empty,
                    previewMedia: modRecord.PreviewMedia,
                    likesCount: modRecord.LikeCount,
                    downloadsCount: mod.DownloadCount,
                    viewsCount: modRecord.ViewCount,
                    files: validFiles,
                    isFeatured: false));
        }

        Logger.Info("Finished loading mods");

        return new DownloadableModsFeed(modViewModels, largestPageCount);
    }

    public override ModPanelFooterViewModel GetPanelFooterViewModel(ModInstallInfo modInstallInfo)
    {
        return new GameBananaModPanelFooterViewModel(modInstallInfo.GetRequiredInstallData<GameBananaInstallData>().ModId);
    }

    public override async Task<ModUpdateCheckResult> CheckForUpdateAsync(HttpClient httpClient, ModInstallInfo modInstallInfo)
    {
        long gameBananaModId = modInstallInfo.GetRequiredInstallData<GameBananaInstallData>().ModId;

        Logger.Info("Checking mod updates for mod with GameBanana id {0}", gameBananaModId);

        GameBananaMod gameBananaMod = await httpClient.GetDeserializedAsync<GameBananaMod>(
            $"https://gamebanana.com/apiv11/Mod/{gameBananaModId}/ProfilePage");

        ModVersion localVersion = modInstallInfo.Version ?? new ModVersion(1, 0, 0);

        Logger.Trace("Local version determined as {0}", localVersion);

        if (gameBananaMod.Version.IsNullOrWhiteSpace())
        {
            Logger.Trace("Failed to check for updates due to there not being a valid version defined on GameBanana");

            return new ModUpdateCheckResult(ModUpdateState.UnableToCheckForUpdates, new ResourceLocString(nameof(Resources.ModLoader_GameBanana_UpdateState_MissingVersion)));
        }

        if (gameBananaMod.Files == null || !gameBananaMod.Files.Any(x =>
                x.ModManagerIntegrations != null &&
                x.ModManagerIntegrations.Any(m => m.ToolId == RaymanControlPanelToolId)))
        {
            Logger.Trace("Failed to check for updates due to the GameBanana mod not having any valid files");

            return new ModUpdateCheckResult(ModUpdateState.UnableToCheckForUpdates, new ResourceLocString(nameof(Resources.ModLoader_GameBanana_UpdateState_MissingFiles)));
        }

        ModVersion onlineVersion;

        try
        {
            onlineVersion = ModVersion.Parse(gameBananaMod.Version);
        }
        catch (Exception ex)
        {
            Logger.Info(ex, "Failed to check for updates due to the GameBanana mod not having its version formatted correctly");

            return new ModUpdateCheckResult(ModUpdateState.UnableToCheckForUpdates, new ResourceLocString(nameof(Resources.ModLoader_GameBanana_UpdateState_InvalidVersion)));
        }

        if (onlineVersion > localVersion)
        {
            Logger.Info("A new update was found with version {0}", onlineVersion);

            // No need to pass in a message here as it shows a button then instead
            return new ModUpdateCheckResult(ModUpdateState.UpdateAvailable, String.Empty, gameBananaMod);
        }
        else
        {
            Logger.Info("No new update found");

            return new ModUpdateCheckResult(ModUpdateState.UpToDate, new ResourceLocString(nameof(Resources.ModLoader_UpdateState_UpToDate)));
        }
    }

    public override async Task<ModDownload?> GetModUpdateDownloadAsync(object? updateData)
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
            ItemSelectionDialogResult result = await Services.UI.SelectItemAsync(new ItemSelectionDialogViewModel(validFiles.Select(x => x.File).ToArray(),
                Resources.ModLoader_GameBanana_SelectUpdateFileHeader)
            {
                Title = Resources.ModLoader_GameBanana_SelectUpdateFileTitle
            });

            if (result.CanceledByUser)
                return null;

            file = validFiles[result.SelectedIndex];
        }
        else
        {
            file = validFiles[0];
        }

        return new ModDownload(file.File, file.DownloadUrl, file.FileSize, new GameBananaInstallData(gameBananaMod.Id, file.Id));
    }

    public override async IAsyncEnumerable<NewModViewModel> GetNewModsAsync(GamesManager gamesManager)
    {
        // Get the GameBanana game id for every game, even ones that are not installed
        Dictionary<int, List<GameDescriptor>> games = new();
        foreach (GameDescriptor gameDescriptor in gamesManager.GetGameDescriptors())
        {
            GameComponentBuilder gameComponentBuilder = gameDescriptor.RegisterComponents();
            IEnumerable<GameComponentBuilder.Component> builtComponents = gameComponentBuilder.Build();

            foreach (GameComponentBuilder.Component builtComponent in builtComponents)
            {
                if (builtComponent.BaseType == typeof(GameBananaGameComponent))
                {
                    GameBananaGameComponent gameBananaComponent = (GameBananaGameComponent)builtComponent.GetInstance();
                    int gameId = gameBananaComponent.GameId;

                    if (!games.TryGetValue(gameId, out List<GameDescriptor> g))
                    {
                        g = new List<GameDescriptor>();
                        games[gameId] = g;
                    }

                    g.Add(gameDescriptor);
                }
            }
        }

        using HttpClient httpClient = new();

        foreach (var g in games)
        {
            GameBananaSubfeed newSubfeed = await httpClient.GetDeserializedAsync<GameBananaSubfeed>(
                $"https://gamebanana.com/apiv11/Game/{g.Key}/Subfeed?" +
                $"_nPage=1&" +
                $"_sSort=new&" +
                $"_csvModelInclusions=Mod");
            GameBananaSubfeed updatedSubfeed = await httpClient.GetDeserializedAsync<GameBananaSubfeed>(
                $"https://gamebanana.com/apiv11/Game/{g.Key}/Subfeed?" +
                $"_nPage=1&" +
                $"_sSort=updated&" +
                $"_csvModelInclusions=Mod");

            var modRecords = newSubfeed.Records.
                // Take the 10 most recent new mods
                Take(10).Select(x => new { Record = x, IsUpdate = false, }).
                // Add the 5 most recent updated mods
                Concat(updatedSubfeed.Records.Take(5).Select(x => new { Record = x, IsUpdate = true, })).
                // Remove duplicates
                GroupBy(x => x.Record.Id).Select(x => x.OrderBy(y => y.Record.DateModified).Last()).
                // Only keep ones which have files
                Where(x => x.Record.HasFiles).
                // Only keep from a maximum of a year back
                Where(x => (x.Record.DateModified - DateTime.Now) < TimeSpan.FromDays(365)).
                ToList();

            if (modRecords.Count == 0)
                yield break;

            // Get additional data for every mod
            GameBananaMod[] mods = await httpClient.GetDeserializedAsync<GameBananaMod[]>(
                $"https://gamebanana.com/apiv11/Mod/Multi?" +
                $"_csvRowIds={modRecords.Select(x => x.Record.Id).JoinItems(",")}&" +
                $"_csvProperties=_aFiles,_aModManagerIntegrations");

            for (int i = 0; i < mods.Length; i++)
            {
                GameBananaRecord modRecord = modRecords[i].Record;
                bool isUpdate = modRecords[i].IsUpdate;
                GameBananaMod mod = mods[i];

                // Make sure the mod has files
                if (mod.Files == null)
                    continue;

                // Make sure it's compatible with Rayman Control Panel
                if (mod.Files.Any(x => mod.ModManagerIntegrations is JObject obj &&
                                       obj.ToObject<Dictionary<string, GameBananaModManager[]>>()?.
                                           TryGetValue(x.Id.ToString(), out GameBananaModManager[] m) == true &&
                                       m.Any(mm => mm.ToolId == RaymanControlPanelToolId)))
                {
                     yield return new NewModViewModel(
                        name: modRecord.Name,
                        modificationDate: modRecord.DateModified,
                        modUrl: $"https://gamebanana.com/mods/{modRecord.Id}",
                        isUpdate: isUpdate,
                        gameDescriptors: g.Value);
                }
            }
        }
    }

    #endregion
}