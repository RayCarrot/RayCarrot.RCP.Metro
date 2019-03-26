namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// The result from a popup prompt
    /// </summary>
    public enum PopupPromptResult
    {
        /// <summary>
        /// The request was ignored and will not be shown again
        /// </summary>
        DoNotShowAgain,

        /// <summary>
        /// The request was ignored
        /// </summary>
        Ignore,

        /// <summary>
        /// The request was accepted
        /// </summary>
        Accept
    }
}