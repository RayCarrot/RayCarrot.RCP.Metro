namespace RayCarrot.RCP.Metro.ModLoader.Sources;

public record DownloadableModsFeedPage(IReadOnlyCollection<DownloadableModViewModel> DownloadableMods, int PageCount);