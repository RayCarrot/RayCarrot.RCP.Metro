using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.ApplicationModel;
using RayCarrot.CarrotFramework;
using RayCarrot.Windows.Shell;

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
            ShortcutCommand = new AsyncRelayCommand(CreateShortcutAsync);

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

                if (gameType != GameType.Steam)
                {
                    AddDuoGridItem(UserLevel.Technical, "Launch path", launchInfo.Path);
                    AddDuoGridItem(UserLevel.Technical, "Launch arguments", launchInfo.Args);
                }
                else
                {
                    AddDuoGridItem(UserLevel.Advanced, "Steam ID", game.GetSteamID());
                }
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

        /// <summary>
        /// The command for creating a shortcut to launch the game
        /// </summary>
        public ICommand ShortcutCommand { get; }

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
                                                         $"your computer or any of its files, including the backups created using this program.", "Confirm remove", MessageType.Question, true))
                return;

            // Remove the game
            await RCFRCP.App.RemoveGameAsync(Game, false);
        }

        /// <summary>
        /// Creates a shortcut to launch the game
        /// </summary>
        /// <returns>The task</returns>
        public async Task CreateShortcutAsync()
        {
            try
            {
                var result = await RCF.BrowseUI.BrowseDirectoryAsync(new DirectoryBrowserViewModel()
                {
                    DefaultDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                    Title = "Select shortcut destination"
                });

                if (result.CanceledByUser)
                    return;

                var gameInfo = Game.GetInfo();
                var shortcutName = $"Launch {Game.GetDisplayName()}";

                if (gameInfo.GameType == GameType.Steam)
                {
                    WindowsHelpers.CreateURLShortcut(shortcutName, result.SelectedDirectory, $@"steam://rungameid/{Game.GetSteamID()}");

                    RCF.Logger.LogTraceSource($"A shortcut was created for {Game} under {result.SelectedDirectory}");

                    await RCF.MessageUI.DisplaySuccessfulActionMessageAsync("Shortcut created successfully");
                }
                else
                {
                    var launchInfo = Game.GetLaunchInfo();

                    await RCFRCP.File.CreateFileShortcutAsync(shortcutName, result.SelectedDirectory, launchInfo.Path, launchInfo.Args);
                }
            }
            catch (Exception ex)
            {
                ex.HandleError("Creating desktop shortcut", Game);
                await RCF.MessageUI.DisplayMessageAsync("The shortcut could not be created", "Shortcut creation failed", MessageType.Error);
            }
        }

        #endregion
    }
}