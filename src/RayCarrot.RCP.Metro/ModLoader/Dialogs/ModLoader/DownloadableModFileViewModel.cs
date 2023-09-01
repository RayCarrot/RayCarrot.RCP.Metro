using RayCarrot.RCP.Metro.GameBanana;

namespace RayCarrot.RCP.Metro.ModLoader.Dialogs.ModLoader;

public class DownloadableModFileViewModel : BaseViewModel
{
    public DownloadableModFileViewModel(GameBananaFile downloadableFile)
    {
        DownloadableFile = downloadableFile;

        FileName = downloadableFile.File;
        FileDescription = downloadableFile.Description ?? String.Empty;
    }

    public GameBananaFile DownloadableFile { get; }

    public string FileName { get; }
    public string FileDescription { get; }
}