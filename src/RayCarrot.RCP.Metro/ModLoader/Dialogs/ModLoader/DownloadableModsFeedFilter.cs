namespace RayCarrot.RCP.Metro.ModLoader.Dialogs.ModLoader;

public class DownloadableModsFeedFilter
{
    public DownloadableModsFeedFilter(string searchText)
    {
        SearchText = searchText;
    }

    public string SearchText { get; }
}