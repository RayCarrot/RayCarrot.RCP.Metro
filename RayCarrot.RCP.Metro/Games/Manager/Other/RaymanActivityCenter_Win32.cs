using RayCarrot.RCP.Core;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// The Rayman Activity Center (Win32) game manager
    /// </summary>
    public sealed class RaymanActivityCenter_Win32 : RCPWin32Game
    {
        #region Public Overrides

        /// <summary>
        /// The game
        /// </summary>
        public override Games Game => Games.RaymanActivityCenter;

        #endregion
    }
}