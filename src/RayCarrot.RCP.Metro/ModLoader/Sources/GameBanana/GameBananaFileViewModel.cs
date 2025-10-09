using System.Windows.Input;

namespace RayCarrot.RCP.Metro.ModLoader.Sources.GameBanana;

public class GameBananaFileViewModel : BaseViewModel
{
    public GameBananaFileViewModel(GameBananaFile downloadableFile, Func<GameBananaFileViewModel, Task> downloadAsyncFunc)
    {
        DownloadableFile = downloadableFile;

        FileName = downloadableFile.File;
        FileDescription = downloadableFile.Description ?? String.Empty;

        DownloadCommand = new AsyncRelayCommand(() => downloadAsyncFunc(this));
    }

    public ICommand DownloadCommand { get; }

    public GameBananaFile DownloadableFile { get; }
    public string FileName { get; }
    public string FileDescription { get; }

    public bool IsAddedToLibrary { get; set; }
}