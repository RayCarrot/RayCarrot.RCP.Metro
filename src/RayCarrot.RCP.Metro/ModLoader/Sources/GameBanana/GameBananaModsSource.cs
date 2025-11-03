using System.Net;
using System.Net.Http;
using System.Text;
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
    private const int RecordsPerPage = 16;

    private static readonly string[] ExcludedSortOptions =
    [
        "Generic_LatestModified",
        "Generic_MostCommented",
        "Generic_LatestComment",
    ];

    public const string SourceId = "GameBanana";

    #endregion

    #region Logger

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    #endregion

    #region Public Properties

    public override string Id => SourceId;
    public override LocalizedString DisplayName => new ResourceLocString(nameof(Resources.ModLoader_GameBanana_Title));
    public override ModSourceIconAsset Icon => ModSourceIconAsset.GameBanana;

    #endregion

    #region Private Methods

    private async Task LoadFeaturedModsAsync(
        ModLoaderViewModel modLoaderViewModel,
        WebImageCache webImageCache,
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
            $"_csvProperties=_idRow,_sName,_aSubmitter,_tsDateAdded,_tsDateUpdated,_sVersion,_aRootCategory,_aPreviewMedia,_nLikeCount,_nViewCount");

        // Process every mod
        foreach (GameBananaMod mod in mods)
        {
            GameBananaDownloadableModViewModel modViewModel = new(
                downloadableModsSource: this,
                modLoaderViewModel: modLoaderViewModel,
                webImageCache: webImageCache,
                httpClient: httpClient,
                gameBananaId: mod.Id,
                isFeatured: true);

            modViewModel.LoadFeedDetails(mod);
            modViewModels.Add(modViewModel);
        }
    }

    private static bool IsModValid(GameBananaMod mod)
    {
        return mod.HasFiles && (!mod.HasContentRatings || Services.Data.ModLoader_IncludeDownloadableNsfwMods);
    }

    #endregion

    #region Public Methods

    public List<GameBananaFile> GetValidFiles(GameBananaFile[] files)
    {
        return files.
            Where(x => x.ModManagerIntegrations?.Any(mm => mm.ToolId == RaymanControlPanelToolId) == true).
            ToList();
    }

    public async Task<GameBananaMod> LoadModAsync(HttpClient httpClient, int modId)
    {
        string url = $"https://gamebanana.com/apiv11/Mod/{modId}/ProfilePage";
        return await httpClient.GetDeserializedAsync<GameBananaMod>(url);
    }

    public override object? ParseInstallData(JObject? installData)
    {
        return installData?.ToObject<GameBananaInstallData>();
    }

    public override object? ParseDependencyDataAsInstallData(JObject? sourceData)
    {
        GameBananaModDependencyData? data = sourceData?.ToObject<GameBananaModDependencyData>();

        if (data == null)
            return null;

        return new GameBananaInstallData(data.ModId, data.FileId ?? -1);
    }

    public override int GetModsFeedPageLength() => RecordsPerPage;

    public override async Task<DownloadableModsFeedPage> LoadModsFeedPage(
        ModLoaderViewModel modLoaderViewModel,
        IReadOnlyCollection<DownloadableModViewModel> loadedDownloadableMods,
        WebImageCache webImageCache,
        HttpClient httpClient,
        GameInstallation gameInstallation,
        DownloadableModsFeedFilter? filter,
        int page)
    {
        List<GameBananaDownloadableModViewModel> modViewModels = new();

        // Only load featured mods on first page when there is no filter
        if (Services.Data.ModLoader_ShowEssentialMods && filter == null && page == 0)
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
                await LoadFeaturedModsAsync(modLoaderViewModel, webImageCache, httpClient, gameInstallation, featuredMods, modViewModels);
        }

        Logger.Info("Loading downloadable GameBanana mods for page {0}", page);

        List<GameBananaMod> modRecords = new();

        int largestPageCount = 0;

        // Enumerate every supported GameBanana game
        foreach (GameBananaGameComponent gameBananaGameComponent in gameInstallation.GetComponents<GameBananaGameComponent>())
        {
            int gameId = gameBananaGameComponent.GameId;

            Logger.Info("Loading mods using GameBanana game id {0}", gameId);

            StringBuilder url = new();

            if (filter is DownloadableModsFeedSearchTextFilter searchTextFilter)
            {
                // Use the search api
                url.Append("https://gamebanana.com/apiv11/Util/Search/Results?");

                // Search only mods
                url.Append("_sModelName=Mod");

                // Sort by best match
                url.Append("&_sOrder=best_match");

                // Set the game id
                url.Append($"&_idGameRow={gameId}");

                // Set the search text
                url.Append($"&_sSearchString={WebUtility.HtmlEncode(searchTextFilter.SearchText)}");

                // Search all allowed fields
                url.Append("&_csvFields=name,description,article,attribs,studio,owner,credits");

                // NOTE: Sadly the search API doesn't allow setting the number of records per page, so we're limited to the default of 15
                // Set page (index starts at 1)
                url.Append($"&_nPage={page + 1}");
            }
            else
            {
                // Use the mod index api
                url.Append("https://gamebanana.com/apiv11/Mod/Index?");

                // Set records per page
                url.Append($"_nPerpage={RecordsPerPage}");

                // Set page (index starts at 1)
                url.Append($"&_nPage={page + 1}");

                // Filter by game
                url.Append($"&_aFilters[Generic_Game]={gameId}");

                // Optionally filter by category
                if (filter is DownloadableModsFeedCategoryAndSortFilter { Category: { } category })
                    url.Append($"&_aFilters[Generic_Category]={category}");

                // Optionally sort
                if (filter is DownloadableModsFeedCategoryAndSortFilter { Sort: { } sort })
                    url.Append($"&_sSort={sort}");
            }

            // Read the mod page feed
            GameBananaFeed feed = await httpClient.GetDeserializedAsync<GameBananaFeed>(url.ToString());

            // Get the page count
            int pageCount = (int)Math.Ceiling(feed.Metadata.RecordCount / (double)feed.Metadata.PerPage);
            if (pageCount > largestPageCount)
                largestPageCount = pageCount;

            // Add the mods
            modRecords.AddRange(feed.Records.Where(x => 
                IsModValid(x) && !loadedDownloadableMods.Concat(modViewModels).Any(m => m is GameBananaDownloadableModViewModel vm && vm.GameBananaId == x.Id)));
        }

        Logger.Info("{0} mods found", modRecords.Count);

        // Process every mod
        foreach (GameBananaMod modRecord in modRecords)
        {
            GameBananaDownloadableModViewModel modViewModel = new(
                downloadableModsSource: this,
                modLoaderViewModel: modLoaderViewModel,
                webImageCache: webImageCache,
                httpClient: httpClient,
                gameBananaId: modRecord.Id,
                isFeatured: false);

            modViewModel.LoadFeedDetails(modRecord);

            modViewModels.Add(modViewModel);
        }

        Logger.Info("Finished loading mods");

        return new DownloadableModsFeedPage(modViewModels, largestPageCount);
    }

    public override Task<DownloadableModViewModel?> LoadModViewModelAsync(
        ModLoaderViewModel modLoaderViewModel, 
        WebImageCache webImageCache,
        HttpClient httpClient, 
        object? installData, 
        GameInstallation gameInstallation)
    {
        if (installData is not GameBananaInstallData gbInstallData)
            return Task.FromResult<DownloadableModViewModel?>(null);

        int modId = (int)gbInstallData.ModId;

        GameBananaDownloadableModViewModel modViewModel = new(
            downloadableModsSource: this,
            modLoaderViewModel: modLoaderViewModel,
            webImageCache: webImageCache,
            httpClient: httpClient,
            gameBananaId: modId,
            isFeatured: false);

        return Task.FromResult<DownloadableModViewModel?>(modViewModel);
    }

    public override async Task<IEnumerable<DownloadableModsCategoryViewModel>> LoadDownloadableModsCategoriesAsync(
        WebImageCache webImageCache,
        HttpClient httpClient,
        GameInstallation gameInstallation)
    {
        List<DownloadableModsCategoryViewModel> categories = new();

        Logger.Info("Loading downloadable GameBanana mod categories");

        // Enumerate every supported GameBanana game
        foreach (GameBananaGameComponent gameBananaGameComponent in gameInstallation.GetComponents<GameBananaGameComponent>())
        {
            int gameId = gameBananaGameComponent.GameId;

            Logger.Info("Loading categories using GameBanana game id {0}", gameId);

            string url = $"https://gamebanana.com/apiv11/Mod/Categories?" +
                         $"_idGameRow={gameId}&" +
                         $"_sSort=count&" + // Can be "count" or "a_to_z"
                         $"_bShowEmpty=true";
            GameBananaCategory[] gameCategories = await httpClient.GetDeserializedAsync<GameBananaCategory[]>(url);

            foreach (GameBananaCategory cat in gameCategories)
            {
                WebImageViewModel? image = cat.IconUrl != null ? new(webImageCache) { Url = cat.IconUrl } : null;
                categories.Add(new DownloadableModsCategoryViewModel(cat.Name, image, cat.Id.ToString()));
            }
        }

        return categories;
    }

    public override async Task<IEnumerable<DownloadableModsSortOptionViewModel>> LoadDownloadableModsSortOptionsAsync(
        HttpClient httpClient, 
        GameInstallation gameInstallation)
    {
        List<DownloadableModsSortOptionViewModel> sortOptions = new();

        Logger.Info("Loading downloadable GameBanana mod sort options");

        const string url = "https://gamebanana.com/apiv11/Mod/ListFilterConfig";
        GameBananaListFilterConfig filterConfig = await httpClient.GetDeserializedAsync<GameBananaListFilterConfig>(url);

        if (filterConfig.Sorts != null)
        {
            foreach (GameBananaSort sort in filterConfig.Sorts)
            {
                if (!ExcludedSortOptions.Contains(sort.Alias))
                    sortOptions.Add(new DownloadableModsSortOptionViewModel(sort.Title, sort.Alias));
            }
        }

        return sortOptions;
    }

    public override async Task<ModDownload[]> DownloadModDependenciesAsync(
        HttpClient httpClient, 
        GameInstallation gameInstallation, 
        IEnumerable<ModDependencyInfo> dependencies)
    {
        List<GameBananaModDependencyData> dependencyDatas = dependencies.
            Select(x => x.SourceData?.ToObject<GameBananaModDependencyData>()).
            Where(x => x != null).
            ToList()!;

        // Get data for every mod
        GameBananaMod[] mods = await httpClient.GetDeserializedAsync<GameBananaMod[]>(
            $"https://gamebanana.com/apiv11/Mod/Multi?" +
            $"_csvRowIds={dependencyDatas.Select(x => x.ModId).JoinItems(",")}&" + 
            $"_csvProperties=_idRow,_sName,_aFiles,_aModManagerIntegrations");

        List<ModDownload> downloads = new();
        foreach (GameBananaMod mod in mods)
        {
            GameBananaFile? file = null;

            // Try and get the defined file if we have a file id
            long? fileId = dependencyDatas.FirstOrDefault(x => x.ModId == mod.Id)?.FileId;
            if (fileId != null)
                file = mod.Files?.FirstOrDefault(x => x.Id == fileId);

            // Search for valid files if we didn't get one
            if (file == null)
            {
                List<GameBananaFile>? validFiles = mod.Files?.
                    Where(x => !x.IsArchived).
                    Where(x => mod.ModManagerIntegrations is JObject obj &&
                               obj.ToObject<Dictionary<string, GameBananaModManager[]>>()?.TryGetValue(x.Id.ToString(), out GameBananaModManager[] m) == true &&
                               m.Any(mm => mm.ToolId == RaymanControlPanelToolId)).
                    ToList();

                if (validFiles == null || validFiles.Count == 0)
                {
                    Logger.Warn("Mod with ID {0} has no valid files", mod.Id);
                    continue;
                }

                if (validFiles.Count > 1)
                {
                    ItemSelectionDialogResult result = await Services.UI.SelectItemAsync(new ItemSelectionDialogViewModel(validFiles.
                            Select(x => x.Description.IsNullOrWhiteSpace()
                                ? x.File
                                : $"{x.File}{Environment.NewLine}{x.Description}").
                            ToArray(),
                        String.Format(Resources.ModLoader_GameBanana_SelectDependencyFileHeader, mod.Name))
                    {
                        Title = Resources.ModLoader_GameBanana_SelectDependencyFileTitle
                    });

                    if (result.CanceledByUser)
                        continue;

                    file = validFiles[result.SelectedIndex];
                }
                else
                {
                    file = validFiles[0];
                }
            }

            downloads.Add(new ModDownload(file.File, mod.Name, file.DownloadUrl, file.FileSize, new GameBananaInstallData(mod.Id, file.Id)));
        }

        return downloads.ToArray();
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
                !x.IsArchived &&
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
            Where(x => !x.IsArchived && x.ModManagerIntegrations != null && x.ModManagerIntegrations.Any(m => m.ToolId == RaymanControlPanelToolId)).
            ToList();

        if (validFiles.Count == 0)
            throw new Exception("There are no valid files");

        GameBananaFile file;

        if (validFiles.Count > 1)
        {
            ItemSelectionDialogResult result = await Services.UI.SelectItemAsync(new ItemSelectionDialogViewModel(validFiles.
                    Select(x => x.Description.IsNullOrWhiteSpace()
                        ? x.File
                        : $"{x.File}{Environment.NewLine}{x.Description}").
                    ToArray(),
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

        return new ModDownload(file.File, gameBananaMod.Name, file.DownloadUrl, file.FileSize, new GameBananaInstallData(gameBananaMod.Id, file.Id));
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

        using HttpClient httpClient = Services.HttpClientFactory.CreateClient();

        foreach (var g in games)
        {
            StringBuilder url = new();

            // Use the mod index api
            url.Append("https://gamebanana.com/apiv11/Mod/Index?");

            // Set records per page
            url.Append("_nPerpage=10");

            // Set page (index starts at 1)
            url.Append("&_nPage=1");

            // Filter by game
            url.Append($"&_aFilters[Generic_Game]={g.Key}");

            // Sort by new and updated
            url.Append("&_sSort=Generic_NewAndUpdated");

            GameBananaFeed feed = await httpClient.GetDeserializedAsync<GameBananaFeed>(url.ToString());

            foreach (GameBananaMod mod in feed.Records)
            {
                // Make sure the mod is valid
                if (!IsModValid(mod))
                    continue;

                // Get the update date
                DateTime? updateDate = mod.DateUpdated ?? mod.DateAdded;
                if (updateDate == null)
                    continue;

                // Make sure the update is not more than a year old
                if (DateTime.Now - updateDate >= TimeSpan.FromDays(365))
                    continue;

                // Treat as an update if it was modified a day after being added
                bool isUpdate = updateDate - mod.DateAdded > TimeSpan.FromDays(1);

                string? imageUrl = null;
                if (mod.PreviewMedia?.Images is { Length: > 0 } images && images[0] is { File100: not null } img)
                    imageUrl = $"{img.BaseUrl}/{img.File100}";

                yield return new NewModViewModel(
                    name: mod.Name,
                    imageUrl: imageUrl,
                    modificationDate: updateDate.Value,
                    modUrl: $"https://gamebanana.com/mods/{mod.Id}",
                    isUpdate: isUpdate,
                    gameDescriptors: g.Value);
            }
        }
    }

    #endregion
}