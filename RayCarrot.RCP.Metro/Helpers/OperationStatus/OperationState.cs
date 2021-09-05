namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// The available states of an ongoing operation
    /// </summary>
    public enum OperationState
    {
        /// <summary>
        /// None
        /// </summary>
        None,

        /// <summary>
        /// The operation is running
        /// </summary>
        Running,

        /// <summary>
        /// The operation is paused
        /// </summary>
        Paused,

        /// <summary>
        /// An error has occurred during the operation
        /// </summary>
        Error
    }
}