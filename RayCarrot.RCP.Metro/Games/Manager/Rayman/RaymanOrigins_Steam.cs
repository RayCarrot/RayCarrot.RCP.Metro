namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// The Rayman Origins (Steam) game manager
    /// </summary>
    public sealed class RaymanOrigins_Steam : RCPSteamGame
    {
        #region Public Overrides

        /// <summary>
        /// The game
        /// </summary>
        public override Games Game => Games.RaymanOrigins;

        /// <summary>
        /// Gets the Steam ID for the game
        /// </summary>
        public override string SteamID => "207490";

        #endregion
    }
}