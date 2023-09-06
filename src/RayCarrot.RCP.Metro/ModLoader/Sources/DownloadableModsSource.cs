using System.Net.Http;
using RayCarrot.RCP.Metro.ModLoader.Dialogs.ModLoader;
using RayCarrot.RCP.Metro.ModLoader.Library;
using RayCarrot.RCP.Metro.ModLoader.Sources.GameBanana;

namespace RayCarrot.RCP.Metro.ModLoader.Sources;

public abstract class DownloadableModsSource
{
    private static readonly Dictionary<string, DownloadableModsSource> _downloadableModsSources = new DownloadableModsSource[]
    {
        new GameBananaModsSource(), // GameBanana
    }.ToDictionary(x => x.Id);

    public abstract string Id { get; }
    public abstract ModSourceIconAsset Icon { get; }

    public static IEnumerable<DownloadableModsSource> GetSources() => _downloadableModsSources.Values;

    public static DownloadableModsSource? GetSource(ModInstallInfo installInfo)
    {
        if (installInfo.Source == null)
            return null;

        return _downloadableModsSources.TryGetValue(installInfo.Source, out DownloadableModsSource src) ? src : null;
    }

    public abstract Task LoadDownloadableModsAsync(
        ModLoaderViewModel modLoaderViewModel,
        HttpClient httpClient,
        GameInstallation gameInstallation, 
        ObservableCollection<DownloadableModViewModel> modsCollection);

    public abstract Task<ModUpdateCheckResult> CheckForUpdateAsync(HttpClient httpClient, ModInstallInfo modInstallInfo);
}