using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using RayCarrot.CarrotFramework.Abstractions;
using RayCarrot.IO;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// The base for a Rayman Control Panel game manager
    /// </summary>
    public abstract class RCPGameManager : RCPGame
    {
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
        /// Indicates if using <see cref="GameLaunchMode"/> is supported
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
            new DuoGridItemViewModel(Resources.GameInfo_InstallDir, GameData.InstallDirectory)
        };

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
        /// Finds the install directory for the game
        /// if one was not found automatically
        /// </summary>
        /// <returns>The install directory</returns>
        public virtual FileSystemPath FindInstallDirectory() => FileSystemPath.EmptyPath;

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
        public virtual Task PostLaunchAsync(Process process)
        {
            // Dispose the process
            process?.Dispose();

            // Check if the application should close
            if (RCFRCP.Data.CloseAppOnGameLaunch)
                Application.Current.Shutdown();

            return Task.CompletedTask;
        }

        /// <summary>
        /// Verifies if the game can launch
        /// </summary>
        /// <returns>True if the game can launch, otherwise false</returns>
        public virtual Task<bool> VerifyCanLaunchAsync() => Task.FromResult(true);

        #endregion

        #region Public Abstract Methods

        /// <summary>
        /// Locates the game
        /// </summary>
        /// <returns>Null if the game was not found. Otherwise a valid or empty path for the install directory</returns>
        public abstract Task<FileSystemPath?> LocateAsync();

        /// <summary>
        /// Indicates if the game is valid
        /// </summary>
        /// <param name="installDir">The game install directory, if any</param>
        /// <returns>True if the game is valid, otherwise false</returns>
        // TODO: Update validation system
        public abstract Task<bool> IsValidAsync(FileSystemPath installDir);

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
        /// <returns>The task</returns>
        public abstract Task CreateGameShortcut(FileSystemPath shortcutName, FileSystemPath destinationDirectory);

        #endregion

        #region Public Methods

        /// <summary>
        /// Launches the specified game
        /// </summary>
        /// <param name="forceRunAsAdmin">Indicated if the game should be forced to run as admin</param>
        /// <returns>The task</returns>
        public async Task LaunchGameAsync(bool forceRunAsAdmin)
        {
            RCFCore.Logger?.LogTraceSource($"The game {Game} is being launched...");

            // Verify that the game can launch
            if (!await VerifyCanLaunchAsync())
            {
                RCFCore.Logger?.LogInformationSource($"The game {Game} could not be launched");
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
            var path = await LocateAsync();

            if (path == null)
                return;

            // Add the game
            await RCFRCP.App.AddNewGameAsync(Game, Type, path);

            // Refresh
            await RCFRCP.App.OnRefreshRequiredAsync(new RefreshRequiredEventArgs(Game, true, false, false, false));

            RCFCore.Logger?.LogInformationSource($"The game {Game} has been added");
        }

        #endregion

        #region Protected Classes

        /// <summary>
        /// The result from launching a game
        /// </summary>
        protected class GameLaunchResult
        {
            /// <summary>
            /// Default constructor
            /// </summary>
            /// <param name="gameProcess">The process for the game</param>
            /// <param name="successfulLaunch">True if the game launched successfully, otherwise false</param>
            public GameLaunchResult(Process gameProcess, bool successfulLaunch)
            {
                GameProcess = gameProcess;
                SuccessfulLaunch = successfulLaunch;
            }

            /// <summary>
            /// The process for the game
            /// </summary>
            public Process GameProcess { get; }

            /// <summary>
            /// True if the game launched successfully, otherwise false
            /// </summary>
            public bool SuccessfulLaunch { get; }
        }

        #endregion
    }
}