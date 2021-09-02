using System.IO;
using RayCarrot.IO;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// View model for the DOSBox games options
    /// </summary>
    public class GameOptions_DOSBox_ViewModel : BaseRCPViewModel
    {
        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="game">The game</param>
        public GameOptions_DOSBox_ViewModel(Games game)
        {
            Game = game;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// The game
        /// </summary>
        public Games Game { get; }

        /// <summary>
        /// The allowed drive types when browsing for a mount path
        /// </summary>
        public DriveType[] MountPathAllowedDriveTypes => new DriveType[]
        {
            DriveType.CDRom
        };

        /// <summary>
        /// The file or directory to mount
        /// </summary>
        public FileSystemPath MountPath
        {
            get => Data.DosBoxGames[Game].MountPath;
            set
            {
                Data.DosBoxGames[Game].MountPath = value;
                _ = App.OnRefreshRequiredAsync(new RefreshRequiredEventArgs(Game, false, false, false, true));
            }
        }

        #endregion
    }
}