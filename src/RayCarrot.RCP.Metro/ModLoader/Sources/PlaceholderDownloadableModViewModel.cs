using RayCarrot.RCP.Metro.ModLoader.Metadata;

namespace RayCarrot.RCP.Metro.ModLoader.Sources;

public class PlaceholderDownloadableModViewModel : DownloadableModViewModel
{
    public PlaceholderDownloadableModViewModel(DownloadableModsSource downloadableModsSource, int feedVersion, int page) 
        : base(downloadableModsSource, null)
    {
        FeedVersion = feedVersion;
        Page = page;
    }

    public override ModVersion? ModVersion => null;

    public int FeedVersion { get; }
    public int Page { get; }
}