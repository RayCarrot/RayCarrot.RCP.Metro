namespace RayCarrot.RCP.Metro.ModLoader.Dialogs.ModLoader;

public class DownloadableModsSortOptionViewModel : BaseViewModel
{
    public DownloadableModsSortOptionViewModel(string name, string? id)
    {
        Name = name;
        Id = id;
    }

    public string Name { get; }
    public string? Id { get; }
}