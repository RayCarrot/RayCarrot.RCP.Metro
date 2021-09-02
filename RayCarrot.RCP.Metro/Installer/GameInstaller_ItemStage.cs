namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// The specific stage of an item during a game installation
    /// </summary>
    public enum GameInstaller_ItemStage
    {
        /// <summary>
        /// The initial stage
        /// </summary>
        Initial,

        /// <summary>
        /// The item has been verified
        /// </summary>
        Verified,

        /// <summary>
        /// The item has finished being processed
        /// </summary>
        Complete
    }
}