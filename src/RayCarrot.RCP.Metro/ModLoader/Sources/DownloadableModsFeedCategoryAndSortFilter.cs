namespace RayCarrot.RCP.Metro.ModLoader.Sources;

public class DownloadableModsFeedCategoryAndSortFilter : DownloadableModsFeedFilter
{
    public string? Order { get; init; }
    public string? Category { get; init; }
}