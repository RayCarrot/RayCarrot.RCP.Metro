using System.Threading.Tasks;
using System.Windows.Input;
using RayCarrot.CarrotFramework;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// View model for a game options dialog
    /// </summary>
    public class GameOptionsViewModel : BaseRCPViewModel
    {
        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="game">The game to show the options for</param>
        public GameOptionsViewModel(Games game)
        {
            RemoveCommand = new AsyncRelayCommand(RemoveAsync);

            Game = game;
            DisplayName = game.GetDisplayName();
            IconSource = game.GetIconSource();

            var info = game.GetInfo();
            GameType = info.GameType;
            InstallDirectory = info.InstallDirectory;

            if (GameType == GameType.WinStore)
            {
                // TODO: Have other fields for WinStore games like full package name etc.
                LaunchPath = "await (await game.GetGamePackage().GetAppListEntriesAsync()).First().LaunchAsync()";
            }
            else
            {
                var launchInfo = game.GetLaunchInfo();
                LaunchPath = launchInfo.Path;
                LaunchArguments = launchInfo.Args;
            }
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// The game
        /// </summary>
        public Games Game { get; }

        /// <summary>
        /// The display name
        /// </summary>
        public string DisplayName { get; }

        /// <summary>
        /// The icons source
        /// </summary>
        public string IconSource { get; }

        /// <summary>
        /// The launch path
        /// </summary>
        public FileSystemPath LaunchPath { get; }

        /// <summary>
        /// The launch arguments
        /// </summary>
        public string LaunchArguments { get; }

        /// <summary>
        /// The game type
        /// </summary>
        public GameType GameType { get; }

        /// <summary>
        /// The install directory
        /// </summary>
        public FileSystemPath InstallDirectory { get; }

        #endregion

        #region Commands

        /// <summary>
        /// The command for removing the game from the program
        /// </summary>
        public ICommand RemoveCommand { get; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Removes the game from the program
        /// </summary>
        /// <returns>The task</returns>
        public async Task RemoveAsync()
        {
            // Ask the user
            if (!await RCF.MessageUI.DisplayMessageAsync($"Are you sure you want to remove {DisplayName} from the Rayman Control Panel? This will not remove the game from " +
                                                         $"your computer or any of its files, including the backups created using this program. Changes made using the utilities " +
                                                         $"may also remain.", "Confirm remove", MessageType.Question, true))
                return;

            // Remove the game
            RCFRCP.App.RemoveGame(Game);
        }

        #endregion
    }
}