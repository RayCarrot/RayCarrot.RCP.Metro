using System.Net.Http;
using RayCarrot.RCP.Metro.ModLoader.Dialogs.ModLoader;

namespace RayCarrot.RCP.Metro.ModLoader.Sources;

public abstract class DownloadableModsSource
{
    public abstract string Id { get; }
    public abstract ModSourceIconAsset Icon { get; }

    public abstract Task LoadDownloadableModsAsync(
        ModLoaderViewModel modLoaderViewModel,
        HttpClient httpClient,
        GameInstallation gameInstallation, 
        ObservableCollection<DownloadableModViewModel> modsCollection);
}