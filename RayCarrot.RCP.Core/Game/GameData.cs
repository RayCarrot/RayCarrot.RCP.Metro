using RayCarrot.IO;

namespace RayCarrot.RCP.Core
{
    /// <summary>
    /// Game data for a game
    /// </summary>
    public class GameData
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="gameType">The game type</param>
        /// <param name="installDirectory">The install directory</param>
        public GameData(GameType gameType, FileSystemPath installDirectory)
        {
            GameType = gameType;
            InstallDirectory = installDirectory;
            LaunchMode = GameLaunchMode.AsInvoker;
        }

        /// <summary>
        /// The game type
        /// </summary>
        public GameType GameType { get; }

        /// <summary>
        /// The install directory
        /// </summary>
        public FileSystemPath InstallDirectory { get; }

        /// <summary>
        /// The game launch mode
        /// </summary>
        public GameLaunchMode LaunchMode { get; set; }
    }
}