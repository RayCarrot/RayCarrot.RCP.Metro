namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// The Rayman Raving Rabbids (Steam) game manager
    /// </summary>
    public sealed class RaymanRavingRabbids_Steam : RCPSteamGame
    {
        #region Public Overrides

        /// <summary>
        /// The game
        /// </summary>
        public override Games Game => Games.RaymanRavingRabbids;

        /// <summary>
        /// Gets the Steam ID for the game
        /// </summary>
        public override string SteamID => "15080";

        #endregion
    }
}