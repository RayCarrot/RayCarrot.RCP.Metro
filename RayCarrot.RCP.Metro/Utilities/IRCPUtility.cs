//using System;
//using RayCarrot.CarrotFramework;
//using System.Threading.Tasks;
//using System.Windows;
//using System.Windows.Controls;

//namespace RayCarrot.RCP.Metro
//{
//    //TODO: Finish this
//    //Below is my attempt to make the app more modular - WIP for version 5.0.0

//    /// <summary>
//    /// Interface for a RCP utility
//    /// </summary>
//    public interface IRCPUtility
//    {
//        /// <summary>
//        /// The header for the utility. This property is retrieved again when the current culture is changed.
//        /// </summary>
//        string DisplayHeader { get; }

//        /// <summary>
//        /// The utility information text (optional). This property is retrieved again when the current culture is changed.
//        /// </summary>
//        string InfoText { get; }

//        /// <summary>
//        /// The utility warning text (optional). This property is retrieved again when the current culture is changed.
//        /// </summary>
//        string WarningText { get; }

//        /// <summary>
//        /// The utility UI content
//        /// </summary>
//        UIElement UIContent { get; }

//        /// <summary>
//        /// Indicates if the utility requires administration privileges
//        /// </summary>
//        bool RequiresAdmin { get; }

//        /// <summary>
//        /// Indicates if the utility is available to the user
//        /// </summary>
//        bool IsAvailable { get; }

//        /// <summary>
//        /// The developers of the utility
//        /// </summary>
//        string Developers { get; }

//        /// <summary>
//        /// The game which the utility was made for
//        /// </summary>
//        Games Game { get; }

//        /// <summary>
//        /// Retrieves a list of applied utilities from this utility
//        /// </summary>
//        /// <returns>The applied utilities</returns>
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
//}