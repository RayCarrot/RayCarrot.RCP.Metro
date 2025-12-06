using System.Net.Http;
using Newtonsoft.Json.Linq;
using RayCarrot.RCP.Metro.ModLoader.Dialogs.ModLoader;
using RayCarrot.RCP.Metro.ModLoader.Library;
using RayCarrot.RCP.Metro.ModLoader.Metadata;
using RayCarrot.RCP.Metro.ModLoader.Sources.GameBanana;
using RayCarrot.RCP.Metro.Pages.Games;

namespace RayCarrot.RCP.Metro.ModLoader.Sources;

public abstract class DownloadableModsSource
{
    #region Private Static Fields

    private static readonly Dictionary<string, DownloadableModsSource> _downloadableModsSources = new DownloadableModsSource[]
    {
        new GameBananaModsSource(), // GameBanana
    }.ToDictionary(x => x.Id);

    #endregion

    #region Public Properties

    public abstract string Id { get; }
    public abstract LocalizedString DisplayName { get; }
    public abstract ModSourceIconAsset Icon { get; }

    #endregion

    #region Public Static Methods

    public static IEnumerable<DownloadableModsSource> GetSources() => _downloadableModsSources.Values;

    public static DownloadableModsSource? GetSource(ModInstallInfo installInfo)
    {
        if (installInfo.Source == null)
            return null;

        return GetSource(installInfo.Source);
    }

    public static DownloadableModsSource? GetSource(string? id)
    {
        if (id == null)
            return null;

        return _downloadableModsSources.TryGetValue(id, out DownloadableModsSource src) ? src : null;
    }

    #endregion

    #region Public Methods

    public abstract object? ParseInstallData(JObject? installData);

    public abstract object? ParseDependencyDataAsInstallData(JObject? sourceData);

    public abstract int GetModsFeedPageLength();

    public abstract Task<DownloadableModsFeedPage> LoadModsFeedPage(
        ModLoaderViewModel modLoaderViewModel,
        IReadOnlyCollection<DownloadableModViewModel> loadedDownloadableMods,
        WebImageCache webImageCache,
        HttpClient httpClient,
        GameInstallation gameInstallation,
        DownloadableModsFeedFilter? filter,
        int page);

    public abstract Task<DownloadableModViewModel?> LoadModViewModelAsync(
        ModLoaderViewModel modLoaderViewModel, 
        WebImageCache webImageCache, 
        HttpClient httpClient, 
        object? installData,
        GameInstallation gameInstallation);

    public abstract Task<IEnumerable<DownloadableModsCategoryViewModel>> LoadDownloadableModsCategoriesAsync(
        WebImageCache webImageCache, 
        HttpClient httpClient, 
        GameInstallation gameInstallation);

    public abstract Task<IEnumerable<DownloadableModsSortOptionViewModel>> LoadDownloadableModsSortOptionsAsync(
        HttpClient httpClient, 
        GameInstallation gameInstallation);

    public abstract Task<ModDownload[]> DownloadModDependenciesAsync(
        HttpClient httpClient,
        GameInstallation gameInstallation,
        IEnumerable<ModDependencyInfo> dependencies);

    public abstract Task<ModUpdateCheckResult> CheckForUpdateAsync(HttpClient httpClient, ModInstallInfo modInstallInfo);

    public abstract Task<ModDownload?> GetModUpdateDownloadAsync(object? updateData);

    public abstract Task<int> GetDownloadableModsCountAsync(
        HttpClient httpClient,
        GameInstallation gameInstallation);

    public abstract IAsyncEnumerable<NewModViewModel> GetNewModsAsync(GamesManager gamesManager);

    #endregion
}