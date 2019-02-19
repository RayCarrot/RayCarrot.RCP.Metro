using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.ApplicationModel;
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

            InfoItems = new ObservableCollection<DuoGridItemViewModel>();

            var info = game.GetInfo();
            var gameType = info.GameType;

            if (gameType == GameType.WinStore)
            {
                Package package = game.GetGamePackage();

                AddDuoGridItem(UserLevel.Debug, "Dependencies", package.Dependencies.Select(x => x.Id.Name).JoinItems(", "));
                AddDuoGridItem(UserLevel.Debug, "Full name", package.Id.FullName);
                AddDuoGridItem(UserLevel.Technical, "Architecture", package.Id.Architecture.ToString());
                AddDuoGridItem(UserLevel.Advanced, "Version", $"{package.Id.Version.Major}.{package.Id.Version.Minor}.{package.Id.Version.Build}.{package.Id.Version.Revision}");
                AddDuoGridItem(UserLevel.Normal, "Installed", package.InstalledDate.DateTime.ToLongDateString());
                AddDuoGridItem(UserLevel.Normal, "Install location", info.InstallDirectory);
            }
            else
            {
                var launchInfo = game.GetLaunchInfo();

                AddDuoGridItem(UserLevel.Technical, "Launch path", launchInfo.Path);
                AddDuoGridItem(UserLevel.Technical, "Launch arguments", launchInfo.Args);
                AddDuoGridItem(UserLevel.Advanced, "Game type", gameType.GetDisplayName());
                AddDuoGridItem(UserLevel.Normal, "Install location", info.InstallDirectory);
            }

            void AddDuoGridItem(UserLevel minUserLevel, string header, string text)
            {
                if (RCFRCP.Data.UserLevel >= minUserLevel)
                    InfoItems.Add(new DuoGridItemViewModel()
                    {
                        Header = header + ":  ",
                        Text = text
                    });
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
        /// The info items to show
        /// </summary>
        public ObservableCollection<DuoGridItemViewModel> InfoItems { get; }

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