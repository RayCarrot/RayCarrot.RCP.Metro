using RayCarrot.RCP.Metro.ModLoader.Dialogs.ModLoader;

namespace RayCarrot.RCP.Metro.ModLoader.Sources.GameBanana;

public class GameBananaFileViewModel : BaseViewModel
{
    public GameBananaFileViewModel(GameBananaFile downloadableFile)
    {
        DownloadableFile = downloadableFile;

        FileName = downloadableFile.File;
        FileDescription = downloadableFile.Description ?? String.Empty;
    }

    public GameBananaFile DownloadableFile { get; }

    public string FileName { get; }
    public string FileDescription { get; }

    public ModViewModel? DownloadedMod { get; set; }
}