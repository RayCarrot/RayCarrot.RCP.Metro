namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// The Rayman Garden PLUS game info
    /// </summary>
    public sealed class GameInfo_RaymanGardenPLUS : GameInfo
    {
        #region Public Override Properties

        /// <summary>
        /// The game
        /// </summary>
        public override Games Game => Games.RaymanGardenPLUS;

        /// <summary>
        /// The category for the game
        /// </summary>
        public override GameCategory Category => GameCategory.Fan;

        /// <summary>
        /// The game display name
        /// </summary>
        public override string DisplayName => "Rayman Garden PLUS";

        /// <summary>
        /// The game backup name
        /// </summary>
        public override string BackupName => "Rayman Garden PLUS";

        /// <summary>
        /// Gets the launch name for the game
        /// </summary>
        public override string DefaultFileName => "Rayman Garden PLUS.exe";

        #endregion
    }
}