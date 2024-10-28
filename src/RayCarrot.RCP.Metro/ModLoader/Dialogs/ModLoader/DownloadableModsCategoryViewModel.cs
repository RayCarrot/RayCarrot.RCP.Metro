using RayCarrot.RCP.Metro.ModLoader.Sources;

namespace RayCarrot.RCP.Metro.ModLoader.Dialogs.ModLoader;

public class DownloadableModsCategoryViewModel : BaseViewModel
{
    public DownloadableModsCategoryViewModel(string name, string? iconUrl, DownloadableModsFeedFilter? filter)
    {
        Name = name;
        IconUrl = iconUrl;
        Filter = filter;
    }

    public string Name { get; }
    public string? IconUrl { get; }
    public DownloadableModsFeedFilter? Filter { get; }
}