namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// The Rayman Designer (DOSBox) game manager
    /// </summary>
    public sealed class RaymanDesigner_DOSBox : RCPDOSBoxGame
    {
        #region Public Overrides

        /// <summary>
        /// The game
        /// </summary>
        public override Games Game => Games.RaymanDesigner;

        /// <summary>
        /// The executable name for the game. This is independent of the <see cref="RCPGameInfo.DefaultFileName"/> which is used to launch the game.
        /// </summary>
        public override string ExecutableName => "RAYKIT.EXE";

        #endregion
    }
}