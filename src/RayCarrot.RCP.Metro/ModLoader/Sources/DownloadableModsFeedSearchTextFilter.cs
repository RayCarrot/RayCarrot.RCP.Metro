namespace RayCarrot.RCP.Metro.ModLoader.Sources;

public class DownloadableModsFeedSearchTextFilter : DownloadableModsFeedFilter
{
    public DownloadableModsFeedSearchTextFilter(string searchText)
    {
        SearchText = searchText;
    }

    public string SearchText { get; }
}