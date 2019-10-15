namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// The Rayman by his Fans (DOSBox) game manager
    /// </summary>
    public sealed class RaymanByHisFans_DOSBox : RCPDOSBoxGame
    {
        #region Public Overrides

        /// <summary>
        /// The game
        /// </summary>
        public override Games Game => Games.RaymanByHisFans;

        /// <summary>
        /// The executable name for the game. This is independent of the <see cref="RCPGameInfo.DefaultFileName"/> which is used to launch the game.
        /// </summary>
        public override string ExecutableName => "RAYFAN.EXE";

        #endregion
    }
}