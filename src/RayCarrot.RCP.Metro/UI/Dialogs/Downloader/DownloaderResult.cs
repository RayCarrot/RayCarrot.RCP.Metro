namespace RayCarrot.RCP.Metro
{
    public class DownloaderResult : UserInputResult
    {
        public DownloaderResult(DownloaderViewModel.DownloadState downloadState)
        {
            DownloadState = downloadState;
        }

        public DownloaderViewModel.DownloadState DownloadState { get; }
    }
}