using RayCarrot.RCP.Metro.ModLoader.Sources;

namespace RayCarrot.RCP.Metro.ModLoader.Dialogs.ModLoader;

public class DownloadableModsCategoryViewModel : BaseViewModel
{
    public DownloadableModsCategoryViewModel(string name, WebImageViewModel? iconImage, string? id)
    {
        Name = name;
        IconImage = iconImage;
        Id = id;

        IconImage?.Load();
    }

    public string Name { get; }
    public WebImageViewModel? IconImage { get; }
    public string? Id { get; }
}