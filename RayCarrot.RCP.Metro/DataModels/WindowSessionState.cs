namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// Contains information regarding the state of a Window
    /// </summary>
    public class WindowSessionState
    {
        /// <summary>
        /// The top position
        /// </summary>
        public double WindowTop { get; set; }

        /// <summary>
        /// The left position
        /// </summary>
        public double WindowLeft { get; set; }

        /// <summary>
        /// The height
        /// </summary>
        public double WindowHeight { get; set; }

        /// <summary>
        /// The width
        /// </summary>
        public double WindowWidth { get; set; }

        /// <summary>
        /// True if maximized
        /// </summary>
        public bool WindowMaximized { get; set; }
    }
}