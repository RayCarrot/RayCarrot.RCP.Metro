namespace RayCarrot.RCP.Updater
{
    /// <summary>
    /// The current stage of the update
    /// </summary>
    public enum UpdateStage
    {
        Manifest,
        Download,
        Install
    }
}