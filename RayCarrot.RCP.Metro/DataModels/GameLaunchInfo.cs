using RayCarrot.CarrotFramework;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// Contains general launch information for a game
    /// </summary>
    public class GameLaunchInfo
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="path">The path of the file to launch</param>
        /// <param name="args">The launch arguments to pass in</param>
        public GameLaunchInfo(FileSystemPath path, string args)
        {
            Path = path;
            Args = args;
        }

        /// <summary>
        /// The path of the file to launch
        /// </summary>
        public FileSystemPath Path { get; }

        /// <summary>
        /// The launch arguments to pass in
        /// </summary>
        public string Args { get; }
    }
}