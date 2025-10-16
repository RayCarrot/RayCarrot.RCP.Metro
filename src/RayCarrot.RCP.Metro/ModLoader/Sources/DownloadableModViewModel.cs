using RayCarrot.RCP.Metro.ModLoader.Metadata;

namespace RayCarrot.RCP.Metro.ModLoader.Sources;

public abstract class DownloadableModViewModel : BaseViewModel, IDisposable
{
    protected DownloadableModViewModel(DownloadableModsSource downloadableModsSource, string? modId)
    {
        DownloadableModsSource = downloadableModsSource;
        ModId = modId;

        if (Services.Data.ModLoader_MarkUnseenMods && modId != null)
            HasViewed = Services.Data.ModLoader_ViewedMods.TryGetValue(DownloadableModsSource.Id, out List<ViewedMod> viewedMod) &&
                        viewedMod.Any(x => x.Id == modId);
        else
            HasViewed = true;
    }

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    public DownloadableModsSource DownloadableModsSource { get; }
    public string? ModId { get; }
    public abstract ModVersion? ModVersion { get; }
    public bool HasViewed { get; set; }
    public bool HasLoadedFullDetails { get; set; }
    public bool IsLoadingFullDetails { get; set; }

    public async Task<bool> OnSelectedAsync()
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
                await LoadFullDetailsAsync();
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Loading full mod details");
                await Services.MessageUI.DisplayExceptionMessageAsync(ex, "An error occurred when loading the mod"); // TODO-LOC
                HasLoadedFullDetails = false;
                return false;
            }
            finally
            {
                IsLoadingFullDetails = false;
            }
        }

        return true;
    }

    public virtual Task LoadFullDetailsAsync() => Task.CompletedTask;
    public virtual void Dispose() { }
}