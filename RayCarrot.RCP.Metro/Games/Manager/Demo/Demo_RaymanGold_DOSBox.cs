namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// The Rayman Gold Demo (DOSBox) game manager
    /// </summary>
    public sealed class Demo_RaymanGold_DOSBox : RCPDOSBoxGame
    {
        #region Public Override Properties

        /// <summary>
        /// The game
        /// </summary>
        public override Games Game => Games.Demo_RaymanGold;

        /// <summary>
        /// The executable name for the game. This is independent of the <see cref="RCPGameInfo.DefaultFileName"/> which is used to launch the game.
        /// </summary>
        public override string ExecutableName => "RAYKIT.EXE";

        /// <summary>
        /// Indicates if the game requires a disc to be mounted in order to play
        /// </summary>
        public override bool RequiresMounting => false;

        #endregion
    }
}