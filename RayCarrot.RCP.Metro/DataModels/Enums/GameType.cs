namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// Determines the type of a game
    /// </summary>
    public enum GameType
    {
        /// <summary>
        /// A Win32 game
        /// </summary>
        Win32,

        /// <summary>
        /// A Steam game
        /// </summary>
        Steam,

        /// <summary>
        /// A Windows store game
        /// </summary>
        WinStore,

        /// <summary>
        /// A DOSBox game
        /// </summary>
        DosBox,

        /// <summary>
        /// An education DOSBox game
        /// </summary>
        EducationalDosBox,
    }
}