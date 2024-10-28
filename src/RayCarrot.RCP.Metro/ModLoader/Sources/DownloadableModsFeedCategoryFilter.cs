namespace RayCarrot.RCP.Metro.ModLoader.Sources;

public class DownloadableModsFeedCategoryFilter : DownloadableModsFeedFilter
{
    public DownloadableModsFeedCategoryFilter(int id)
    {
        Id = id;
    }

    public int Id { get; }
}