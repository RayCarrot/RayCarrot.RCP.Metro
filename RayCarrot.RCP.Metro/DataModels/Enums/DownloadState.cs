namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// The state of a download
    /// </summary>
    public enum DownloadState
    {
        /// <summary>
        /// The download is paused or has not started
        /// </summary>
        Paused,

        /// <summary>
        /// The download is currently running
        /// </summary>
        Running,

        /// <summary>
        /// The download failed
        /// </summary>
        Failed,

        /// <summary>
        /// The download was canceled
        /// </summary>
        Canceled,

        /// <summary>
        /// The download succeeded
        /// </summary>
        Succeeded
    }
}