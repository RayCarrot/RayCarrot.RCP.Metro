namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// The Rayman 1 Demo 1 (DOSBox) game manager
    /// </summary>
    public sealed class Demo_Rayman1_1_DOSBox : RCPDOSBoxGame
    {
        #region Public Override Properties

        /// <summary>
        /// The game
        /// </summary>
        public override Games Game => Games.Demo_Rayman1_1;

        /// <summary>
        /// The executable name for the game. This is independent of the <see cref="RCPGameInfo.DefaultFileName"/> which is used to launch the game.
        /// </summary>
        public override string ExecutableName => "RAYMAN.EXE";

        #endregion
    }

    /// <summary>
    /// The Rayman 1 Demo 2 (DOSBox) game manager
    /// </summary>
    public sealed class Demo_Rayman1_2_DOSBox : RCPDOSBoxGame
    {
        #region Public Override Properties

        /// <summary>
        /// The game
        /// </summary>
        public override Games Game => Games.Demo_Rayman1_2;

        /// <summary>
        /// The executable name for the game. This is independent of the <see cref="RCPGameInfo.DefaultFileName"/> which is used to launch the game.
        /// </summary>
        public override string ExecutableName => "RAYMAN.EXE";

        #endregion
    }
}