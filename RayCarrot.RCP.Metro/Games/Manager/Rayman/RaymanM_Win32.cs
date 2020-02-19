using RayCarrot.Rayman.UbiIni;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// The Rayman M (Win32) game manager
    /// </summary>
    public sealed class RaymanM_Win32 : RCPWin32Game
    {
        #region Public Overrides

        /// <summary>
        /// The game
        /// </summary>
        public override Games Game => Games.RaymanM;

        /// <summary>
        /// Gets the game finder item for this game
        /// </summary>
        public override GameFinderItem GameFinderItem => new GameFinderItem(RMUbiIniHandler.SectionName, "Rayman M", new string[]
        {
            "Rayman M",
            "Rayman: M",
        });

        #endregion
    }
}