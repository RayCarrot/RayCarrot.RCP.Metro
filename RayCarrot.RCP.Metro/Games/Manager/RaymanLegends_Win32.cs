namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// The Rayman Legends (Win32) game manager
    /// </summary>
    public sealed class RaymanLegends_Win32 : RCPWin32Game
    {
        #region Public Overrides

        /// <summary>
        /// The game
        /// </summary>
        public override Games Game => Games.RaymanLegends;

        #endregion
    }
}