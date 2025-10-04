namespace RayCarrot.RCP.Metro.ModLoader.Sources;

public class PlaceholderDownloadableModViewModel : DownloadableModViewModel
{
    public PlaceholderDownloadableModViewModel(int feedVersion, int page)
    {
        FeedVersion = feedVersion;
        Page = page;
    }

    public int FeedVersion { get; }
    public int Page { get; }
}