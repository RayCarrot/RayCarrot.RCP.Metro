namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// The Rayman Raving Rabbids 2 (Win32) game manager
    /// </summary>
    public sealed class GameManager_RaymanRavingRabbids2_Win32 : GameManager_Win32
    {
        #region Public Overrides

        /// <summary>
        /// The game
        /// </summary>
        public override Games Game => Games.RaymanRavingRabbids2;

        /// <summary>
        /// Gets the launch arguments for the game
        /// </summary>
        public override string GetLaunchArgs => $"/{Services.Data.RRR2LaunchMode.ToString().ToLower()} /B Rrr2.bf";

        /// <summary>
        /// Gets the game finder item for this game
        /// </summary>
        public override GameFinder_GameItem GameFinderItem => new GameFinder_GameItem(null, "Rayman Raving Rabbids 2", new string[]
        {
            "Rayman Raving Rabbids 2",
            "Rayman: Raving Rabbids 2",
            "Rayman Raving Rabbids 2 Orange",
            "Rayman: Raving Rabbids 2 Orange",
            "RRR2",
        });

        #endregion
    }
}