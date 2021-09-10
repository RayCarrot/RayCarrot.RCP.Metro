namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// The result for a game installation
    /// </summary>
    public enum GameInstaller_Result
    {
        /// <summary>
        /// The installation was successful
        /// </summary>
        Successful,

        /// <summary>
        /// The installation failed without resources being cleaned up
        /// </summary>
        Failed,

        /// <summary>
        /// The installation was canceled by the user
        /// </summary>
        Canceled,
    }
}