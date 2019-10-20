using RayCarrot.IO;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// A game finder result
    /// </summary>
    public class GameFinderResult
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="game">The game</param>
        /// <param name="installLocation">The install location</param>
        /// <param name="gameType">The game type</param>
        public GameFinderResult(Games game, FileSystemPath installLocation, GameType gameType)
        {
            Game = game;
            InstallLocation = installLocation;
            GameType = gameType;
        }

        /// <summary>
        /// The game
        /// </summary>
        public Games Game { get; }

        /// <summary>
        /// The install location
        /// </summary>
        public FileSystemPath InstallLocation { get; }

        /// <summary>
        /// The game type
        /// </summary>
        public GameType GameType { get; }
    }
}