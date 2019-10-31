namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// The available debug data output types
    /// </summary>
    public enum DebugDataOutputTypes
    {
        /// <summary>
        /// Displays a list of the referenced assemblies
        /// </summary>
        ReferencedAssemblies,

        /// <summary>
        /// Displays the app user data file contents
        /// </summary>
        AppUserData,

        /// <summary>
        /// Displays the update manifest from the server
        /// </summary>
        UpdateManifest,

        /// <summary>
        /// Displays a list of windows in the current application
        /// </summary>
        AppWindows,

        /// <summary>
        /// Runs the game finder, searching for all games and displaying the output of found games and their install locations
        /// </summary>
        GameFinder,

        /// <summary>
        /// Display the info available for each game
        /// </summary>
        GameInfo
    }
}