using RayCarrot.Rayman;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// The Rayman Arena (Win32) game manager
    /// </summary>
    public sealed class RaymanArena_Win32 : RCPWin32Game
    {
        #region Public Override Properties

        /// <summary>
        /// The game
        /// </summary>
        public override Games Game => Games.RaymanArena;

        /// <summary>
        /// Gets the game finder item for this game
        /// </summary>
        public override GameFinderItem GameFinderItem => new GameFinderItem(RAUbiIniHandler.SectionName, "Rayman Arena", new string[]
        {
            "Rayman Arena",
            "Rayman: Arena",
        });

        #endregion
    }
}