using System;
using System.Linq;
using System.Runtime.CompilerServices;
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
            GameInfo = game.GetInfo();
            LaunchInfo = GameInfo.GameType == GameType.Win32 || GameInfo.GameType == GameType.DosBox ? game.GetLaunchInfo() : null;
            InstallDir = GameInfo.InstallDirectory;

            if (GameInfo.GameType == GameType.WinStore)
                AddPackageInfo();
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// The game
        /// </summary>
        public Games Game { get; }

        /// <summary>
        /// The game info
        /// </summary>
        public GameInfo GameInfo { get; }

        /// <summary>
        /// The display name
        /// </summary>
        public string DisplayName { get; }

        /// <summary>
        /// The icons source
        /// </summary>
        public string IconSource { get; }

        /// <summary>
        /// The game launch info, if available
        /// </summary>
        public GameLaunchInfo LaunchInfo { get; }

        /// <summary>
        /// The game's Steam ID
        /// </summary>
        public string SteamID => GameInfo.GameType == GameType.Steam ? Game.GetSteamID() : null;

        /// <summary>
        /// The game install directory
        /// </summary>
        public string InstallDir { get; set; }

        /// <summary>
        /// The Windows Store app dependencies
        /// </summary>
        public string WinStoreDependencies { get; set; }

        /// <summary>
        /// The Windows Store app full name
        /// </summary>
        public string WinStoreFullName { get; set; }

        /// <summary>
        /// The Windows Store app architecture
        /// </summary>
        public string WinStoreArchitecture { get; set; }

        /// <summary>
        /// The Windows Store app version
        /// </summary>
        public string WinStoreVersion { get; set; }

        /// <summary>
        /// The Windows Store app install date
        /// </summary>
        public DateTime WinStoreInstallDate { get; set; }

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

        #region Private Methods

        /// <summary>
        /// Adds Windows store package information
        /// </summary>
        [MethodImpl(MethodImplOptions.NoInlining)]
        private void AddPackageInfo()
        {
            if (!(Game.GetGamePackage() is Package package))
            {
                RCF.Logger.LogErrorSource("Game options WinStore package is null");
                return;
            }

            WinStoreDependencies = package.Dependencies.Select(x => x.Id.Name).JoinItems(", ");
            WinStoreFullName = package.Id.FullName;
            WinStoreArchitecture = package.Id.Architecture.ToString();
            WinStoreVersion = $"{package.Id.Version.Major}.{package.Id.Version.Minor}.{package.Id.Version.Build}.{package.Id.Version.Revision}";
            WinStoreInstallDate = package.InstalledDate.DateTime;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Removes the game from the program
        /// </summary>
        /// <returns>The task</returns>
        public async Task RemoveAsync()
        {
            // Ask the user
            if (!await RCF.MessageUI.DisplayMessageAsync(String.Format(Resources.RemoveGameQuestion, DisplayName), Resources.RemoveGameQuestionHeader,  MessageType.Question, true))
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
                    Title = Resources.GameShortcut_BrowseHeader
                });

                if (result.CanceledByUser)
                    return;

                var gameInfo = Game.GetInfo();
                var shortcutName = String.Format(Resources.GameShortcut_ShortcutName, Game.GetDisplayName());

                if (gameInfo.GameType == GameType.Steam)
                {
                    WindowsHelpers.CreateURLShortcut(shortcutName, result.SelectedDirectory, $@"steam://rungameid/{Game.GetSteamID()}");

                    RCF.Logger.LogTraceSource($"A shortcut was created for {Game} under {result.SelectedDirectory}");

                    await RCF.MessageUI.DisplaySuccessfulActionMessageAsync(Resources.GameShortcut_Success);
                }
                else
                {
                    var launchInfo = Game.GetLaunchInfo();

                    await RCFRCP.File.CreateFileShortcutAsync(shortcutName, result.SelectedDirectory, launchInfo.Path, launchInfo.Args);
                }
            }
            catch (Exception ex)
            {
                ex.HandleError("Creating game shortcut", Game);
                await RCF.MessageUI.DisplayMessageAsync(Resources.GameShortcut_Error, Resources.GameShortcut_ErrorHeader, MessageType.Error);
            }
        }

        #endregion
    }
}