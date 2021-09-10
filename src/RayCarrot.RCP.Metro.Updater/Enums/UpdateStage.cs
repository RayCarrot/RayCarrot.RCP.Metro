namespace RayCarrot.RCP.Updater
{
    /// <summary>
    /// The current stage of the update
    /// </summary>
    public enum UpdateStage
    {
        /// <summary>
        /// No stage - the update has not begun
        /// </summary>
        None,

        /// <summary>
        /// The initial stage
        /// </summary>
        Initial,

        /// <summary>
        /// The update is downloading
        /// </summary>
        Download,

        /// <summary>
        /// The update is installing
        /// </summary>
        Install,

        /// <summary>
        /// The update has finished
        /// </summary>
        Finished
    }
}