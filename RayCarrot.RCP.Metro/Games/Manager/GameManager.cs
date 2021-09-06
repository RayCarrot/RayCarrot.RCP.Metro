using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using MahApps.Metro.IconPacks;
using RayCarrot.IO;
using NLog;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// The base for a Rayman Control Panel game manager
    /// </summary>
    public abstract class GameManager : BaseGameData
    {
        #region Logger

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        #endregion

        #region Public Abstract Properties

        /// <summary>
        /// The game type
        /// </summary>
        public abstract GameType Type { get; }

        /// <summary>
        /// The display name for the game type
        /// </summary>
        public abstract string GameTypeDisplayName { get; }

        /// <summary>
        /// Indicates if using <see cref="UserData_GameLaunchMode"/> is supported
        /// </summary>
        public abstract bool SupportsGameLaunchMode { get; }

        #endregion

        #region Public Virtual Properties

        /// <summary>
        /// Gets the purchase links for the game for this type
        /// </summary>
        public virtual IList<GamePurchaseLink> GetGamePurchaseLinks => new GamePurchaseLink[0];

        /// <summary>
        /// Gets the additional overflow button items for the game
        /// </summary>
        public virtual IList<OverflowButtonItemViewModel> GetAdditionalOverflowButtonItems => new OverflowButtonItemViewModel[0];

        /// <summary>
        /// Gets the info items for the game
        /// </summary>
        public virtual IList<DuoGridItemViewModel> GetGameInfoItems => new DuoGridItemViewModel[]
        {
            new DuoGridItemViewModel(Resources.GameInfo_GameType, GameTypeDisplayName, UserLevel.Advanced),
            new DuoGridItemViewModel(Resources.GameInfo_InstallDir, Game.GetInstallDir()),
            //new DuoGridItemViewModel("Install size", GameData.InstallDirectory.GetSize().ToString())
        };

        /// <summary>
        /// Gets the game finder item for this game
        /// </summary>
        public virtual GameFinder_GameItem GameFinderItem => null;

        #endregion

        #region Protected Abstract Methods

        /// <summary>
        /// The implementation for launching the game
        /// </summary>
        /// <param name="forceRunAsAdmin">Indicated if the game should be forced to run as admin</param>
        /// <returns>The launch result</returns>
        protected abstract Task<GameLaunchResult> LaunchAsync(bool forceRunAsAdmin);

        #endregion

        #region Public Virtual Methods

        /// <summary>
        /// Gets called as soon as the game is added for the first time
        /// </summary>
        /// <returns>The task</returns>
        public virtual Task PostGameAddAsync() => Task.CompletedTask;

        /// <summary>
        /// Gets called as soon as the game is removed
        /// </summary>
        /// <returns>The task</returns>
        public virtual Task PostGameRemovedAsync() => Task.CompletedTask;

        /// <summary>
        /// Post launch operations for the game which launched
        /// </summary>
        /// <param name="process">The game process</param>
        /// <returns>The task</returns>
        public virtual async Task PostLaunchAsync(Process process)
        {
            // Dispose the process
            process?.Dispose();

            // Check if the application should close
            if (Services.Data.CloseAppOnGameLaunch)
                await App.Current.ShutdownRCFAppAsync(false);
        }

        /// <summary>
        /// Verifies if the game can launch
        /// </summary>
        /// <returns>True if the game can launch, otherwise false</returns>
        public virtual Task<bool> VerifyCanLaunchAsync() => Task.FromResult(true);

        /// <summary>
        /// Indicates if the game is valid
        /// </summary>
        /// <param name="installDir">The game install directory, if any</param>
        /// <param name="parameter">Optional game parameter</param>
        /// <returns>True if the game is valid, otherwise false</returns>
        public virtual Task<bool> IsValidAsync(FileSystemPath installDir, object parameter = null)
        {
            // Make sure the default file exists in the install directory
            return Task.FromResult((installDir + Game.GetGameInfo().DefaultFileName).FileExists);
        }

        #endregion

        #region Public Abstract Methods

        /// <summary>
        /// Locates the game
        /// </summary>
        /// <returns>Null if the game was not found. Otherwise a valid or empty path for the install directory</returns>
        public abstract Task<FileSystemPath?> LocateAsync();

        /// <summary>
        /// Gets the available jump list items for this game
        /// </summary>
        /// <returns>The items</returns>
        public abstract IList<JumpListItemViewModel> GetJumpListItems();

        /// <summary>
        /// Creates a shortcut to launch the game from
        /// </summary>
        /// <param name="shortcutName">The name of the shortcut</param>
        /// <param name="destinationDirectory">The destination directory for the shortcut</param>
        public abstract void CreateGameShortcut(FileSystemPath shortcutName, FileSystemPath destinationDirectory);

        #endregion

        #region Public Methods

        /// <summary>
        /// Launches the specified game
        /// </summary>
        /// <param name="forceRunAsAdmin">Indicated if the game should be forced to run as admin</param>
        /// <returns>The task</returns>
        public async Task LaunchGameAsync(bool forceRunAsAdmin)
        {
            Logger.Trace("The game {0} is being launched...", Game);

            // Verify that the game can launch
            if (!await VerifyCanLaunchAsync())
            {
                Logger.Info("The game {0} could not be launched", Game);
                return;
            }

            // Launch the game from the manager's implementation and get the process if available
            var launchResult = await LaunchAsync(forceRunAsAdmin);

            if (launchResult.SuccessfulLaunch)
                // Run any post launch operations on the process
                await PostLaunchAsync(launchResult.GameProcess);
        }

        /// <summary>
        /// Locates and adds the game
        /// </summary>
        /// <returns>The task</returns>
        public async Task LocateAddGameAsync()
        {
            // Locate the game and get the path
            var path = await LocateAsync();

            if (path == null)
                return;

            // Add the game
            await Services.App.AddNewGameAsync(Game, Type, path.Value);

            // Refresh
            await Services.App.OnRefreshRequiredAsync(new RefreshRequiredEventArgs(Game, RefreshFlags.GameCollection));

            Logger.Info("The game {0} has been added", Game);
        }

        #endregion

        #region Data Types

        /// <summary>
        /// The result from launching a game
        /// </summary>
        protected record GameLaunchResult(Process GameProcess, bool SuccessfulLaunch);

        /// <summary>
        /// A game purchase link which can be accessed from the game
        /// </summary>
        public record GamePurchaseLink(string Header, string Path, PackIconMaterialKind Icon = PackIconMaterialKind.BriefcaseOutline);

        #endregion
    }
}