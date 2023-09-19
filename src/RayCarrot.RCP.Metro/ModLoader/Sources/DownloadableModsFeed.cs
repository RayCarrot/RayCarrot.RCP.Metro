namespace RayCarrot.RCP.Metro.ModLoader.Sources;

public record DownloadableModsFeed(IReadOnlyCollection<DownloadableModViewModel> DownloadableMods, int PageCount);