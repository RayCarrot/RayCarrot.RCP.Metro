using RayCarrot.IO;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// Game information for a saved game
    /// </summary>
    public class GameInfo
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="gameType">The game type</param>
        /// <param name="installDirectory">The install directory</param>
        public GameInfo(GameType gameType, FileSystemPath installDirectory)
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