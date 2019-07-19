using RayCarrot.IO;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// The result for a game finder action
    /// </summary>
    public class GameFinderActionResult
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="path">The path</param>
        /// <param name="type">The game type</param>
        /// <param name="source">The source, for logging</param>
        public GameFinderActionResult(FileSystemPath path, GameType type, string source)
        {
            Path = path;
            Type = type;
            Source = source;
        }

        /// <summary>
        /// The path
        /// </summary>
        public FileSystemPath Path { get; }

        /// <summary>
        /// The game type
        /// </summary>
        public GameType Type { get; }

        /// <summary>
        /// The source, for logging
        /// </summary>
        public string Source { get; }
    }
}