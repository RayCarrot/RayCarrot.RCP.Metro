namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// Contains information for keeping track of the status of a running operation
    /// </summary>
    public interface IStatusUpdated
    {
        /// <summary>
        /// Occurs when the status for an ongoing operation is updated
        /// </summary>
        event StatusUpdateEventHandler StatusUpdated;
    }
}