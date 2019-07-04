//using RayCarrot.CarrotFramework;
//using System.Threading.Tasks;

//namespace RayCarrot.RCP.Metro
//{
//    // TODO: Finish this
//    // Below is my attempt to make the app more modular - WIP for version 5.0.0

//    /// <summary>
//    /// Interface for a RCP utility
//    /// </summary>
//    public interface IRCPUtility
//    {
//        string DisplayHeader { get; }

//        string InfoText { get; }

//        string WarningText { get; }

//        object UIContent { get; }

//        bool RequiresAdmin { get; }

//        bool IsAvailable { get; }

//        string Developers { get; }

//        Games Game { get; }

//        string[] GetAppliedUtilities();
//    }

//    public class Ray1SoundtrackUtility : IRCPUtility
//    {
//        public Ray1SoundtrackUtility()
//        {
//            // Attempt to find the Rayman Forever music directory
//            var dir = GetMusicDirectory();

//            const string dummyFile = "rayman02.ogg";

//            // Set to music path if found
//            MusicDir = dir.DirectoryExists && (dir + dummyFile).FileExists ? dir : FileSystemPath.EmptyPath;

//            IsAvailable = MusicDir.DirectoryExists;

//            RequiresAdmin = IsAvailable && !RCFRCP.File.CheckFileWriteAccess(MusicDir + dummyFile);
//        }

//        public string DisplayHeader => Resources.R1U_CompleteOSTHeader;

//        public string InfoText => Resources.R1U_CompleteOSTInfo;

//        public string WarningText => null;

//        public object UIContent => new object();

//        public bool RequiresAdmin { get; }

//        public bool IsAvailable { get; }

//        public string Developers { get; }

//        public Games Game { get; }

//        public string[] GetAppliedUtilities()
//        {
//            return new string[]
//            {

//            };
//        }

//        /// <summary>
//        /// The Rayman Forever music directory
//        /// </summary>
//        public FileSystemPath MusicDir { get; }

//        #region Public Static Methods

//        /// <summary>
//        /// Gets the game music directory
//        /// </summary>
//        /// <returns>The music director path</returns>
//        public static FileSystemPath GetMusicDirectory()
//        {
//            return Games.Rayman1.GetInfo().InstallDirectory.Parent + "Music";
//        }

//        #endregion
//    }

//    public abstract class RCPGameType
//    {
//        public abstract Task LaunchGame(Games game);

//        public abstract Task CreateShortcut(Games game);
//    }
//}