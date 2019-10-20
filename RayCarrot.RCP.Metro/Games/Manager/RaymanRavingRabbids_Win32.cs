namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// The Rayman Raving Rabbids (Win32) game manager
    /// </summary>
    public sealed class RaymanRavingRabbids_Win32 : RCPWin32Game
    {
        #region Public Overrides

        /// <summary>
        /// The game
        /// </summary>
        public override Games Game => Games.RaymanRavingRabbids;

        /// <summary>
        /// Gets the game finder item for this game
        /// </summary>
        public override GameFinderItem GameFinderItem => new GameFinderItem(null, "Rayman Raving Rabbids", new string[]
        {
            "Rayman Raving Rabbids",
            "Rayman: Raving Rabbids",
            "RRR",
        });

        #endregion
    }
}