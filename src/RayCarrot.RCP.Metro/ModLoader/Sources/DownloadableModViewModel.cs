using RayCarrot.RCP.Metro.ModLoader.Metadata;

namespace RayCarrot.RCP.Metro.ModLoader.Sources;

public abstract class DownloadableModViewModel : BaseViewModel, IDisposable
{
    protected DownloadableModViewModel(DownloadableModsSource downloadableModsSource)
    {
        DownloadableModsSource = downloadableModsSource;
        UpdateHasViewed();
    }

    public DownloadableModsSource DownloadableModsSource { get; }
    public abstract string? ModId { get; }
    public abstract ModVersion? ModVersion { get; }
    public bool HasViewed { get; set; }
    public bool HasLoadedFullDetails { get; set; }
    public bool IsLoadingFullDetails { get; set; }

    public void UpdateHasViewed()
    {
        if (ModId != null)
            HasViewed = Services.Data.ModLoader_ViewedMods.TryGetValue(DownloadableModsSource.Id, out List<ViewedMod> viewedMod) &&
                        viewedMod.Any(x => x.Id == ModId);
    }

    public async Task OnSelectedAsync()
    {
        // Mark as viewed
        if (!HasViewed && ModId != null)
        {
            HasViewed = true;
            if (!Services.Data.ModLoader_ViewedMods.ContainsKey(DownloadableModsSource.Id))
                Services.Data.ModLoader_ViewedMods.Add(DownloadableModsSource.Id, new List<ViewedMod>());
            Services.Data.ModLoader_ViewedMods[DownloadableModsSource.Id].Add(new ViewedMod(ModId, DateTime.Now, ModVersion));
        }

        if (!HasLoadedFullDetails)
        {
            HasLoadedFullDetails = true;
            IsLoadingFullDetails = true;

            try
            {
                // TODO-UPDATE: Catch exceptions
                await LoadFullDetailsAsync();
            }
            finally
            {
                IsLoadingFullDetails = false;
            }
        }
    }

    public virtual Task LoadFullDetailsAsync() => Task.CompletedTask;
    public virtual void Dispose() { }
}