namespace RayCarrot.RCP.Metro.ModLoader.Dialogs.ModLoader;

public class DownloadableModsCategoryViewModel : BaseViewModel
{
    public DownloadableModsCategoryViewModel(string name, string? iconUrl, string? id)
    {
        Name = name;
        IconUrl = iconUrl;
        Id = id;
    }

    public string Name { get; }
    public string? IconUrl { get; }
    public string? Id { get; }
}