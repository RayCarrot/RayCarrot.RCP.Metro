namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// The DOSBox emulator for emulating MS-DOS games
    /// </summary>
    public class DOSBoxEmulator : Emulator
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="game">The game</param>
        /// <param name="gameType">The game type</param>
        public DOSBoxEmulator(Games game, GameType gameType)
        {
            Game = game;
            GameType = gameType;
        }

        /// <summary>
        /// The game
        /// </summary>
        public Games Game { get; }
        
        /// <summary>
        /// The game type
        /// </summary>
        public GameType GameType { get; }

        /// <summary>
        /// The display name of the emulator
        /// </summary>
        public override LocalizedString DisplayName => new LocalizedString(() => Resources.GameType_DosBox);

        /// <summary>
        /// The emulator's game configuration UI
        /// </summary>
        public override object GameConfigUI => new DOSBoxEmulatorGameConfig(new DOSBoxEmulatorGameConfigViewModel(Game, GameType));
    }
}