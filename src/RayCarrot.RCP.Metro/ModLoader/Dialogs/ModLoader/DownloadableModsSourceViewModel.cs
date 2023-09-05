using RayCarrot.RCP.Metro.ModLoader.Sources;

namespace RayCarrot.RCP.Metro.ModLoader.Dialogs.ModLoader;

public class DownloadableModsSourceViewModel : BaseViewModel
{
    public DownloadableModsSourceViewModel(DownloadableModsSource source)
    {
        Source = source;
        Icon = source.Icon;
    }

    public DownloadableModsSource Source { get; }
    public ModSourceIconAsset Icon { get; }
}