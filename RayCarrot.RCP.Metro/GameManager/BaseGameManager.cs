using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using RayCarrot.CarrotFramework.Abstractions;
using RayCarrot.IO;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// The base game manager
    /// </summary>
    public abstract class BaseGameManager
    {
        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="game">The game to manage</param>
        /// <param name="type">The game type</param>
        protected BaseGameManager(Games game, GameType type)
        {
            Game = game;
            IsAdded = game.IsAdded();
            Info = IsAdded ? Game.GetInfo() : null;
            Type = type;
        }

        #endregion

        #region Protected Properties

        /// <summary>
        /// The game type
        /// </summary>
        protected GameType Type { get; }

        /// <summary>
        /// The game to manage
        /// </summary>
        protected Games Game { get; }

        /// <summary>
        /// The game info
        /// </summary>
        protected GameInfo Info { get; }

        /// <summary>
        /// Indicates if the game has been added
        /// </summary>
        protected bool IsAdded { get; }

        #endregion

        #region Public Abstract Properties

        /// <summary>
        /// The display name for the game type
        /// </summary>
        public abstract string GameTypeDisplayName { get; }

        /// <summary>
        /// Indicates if using <see cref="GameLaunchMode"/> is supported
        /// </summary>
        public abstract bool SupportsGameLaunchMode { get; }

        #endregion

        #region Protected Virtual Methods

        /// <summary>
        /// Verifies if the game can launch
        /// </summary>
        /// <returns>True if the game can launch, otherwise false</returns>
        protected virtual Task<bool> VerifyCanLaunchAsync() => Task.FromResult(true);

        /// <summary>
        /// The implementation for launching the game
        /// </summary>
        /// <param name="forceRunAsAdmin">Indicated if the game should be forced to run as admin</param>
        /// <returns>The launch result</returns>
        protected virtual async Task<GameLaunchResult> LaunchAsync(bool forceRunAsAdmin)
        {
            // Get the launch info
            GameLaunchInfo launchInfo = GetLaunchInfo();

            RCFCore.Logger?.LogTraceSource($"The game {Game} launch info has been retrieved as Path = {launchInfo.Path}, Args = {launchInfo.Args}");

            // Launch the game
            var process = await RCFRCP.File.LaunchFileAsync(launchInfo.Path, forceRunAsAdmin || Info.LaunchMode == GameLaunchMode.AsAdmin, launchInfo.Args);

            RCFCore.Logger?.LogInformationSource($"The game {Game} has been launched");

            return new GameLaunchResult(process, process != null);
        }

        /// <summary>
        /// Post launch operations for the game which launched
        /// </summary>
        /// <param name="process">The game process</param>
        /// <returns>The task</returns>
        protected virtual Task PostLaunchAsync(Process process)
        {
            // Dispose the process
            process?.Dispose();

            // Check if the application should close
            if (RCFRCP.Data.CloseAppOnGameLaunch)
                Application.Current.Shutdown();

            return Task.CompletedTask;
        }

        #endregion

        #region Public Virtual Methods

        /// <summary>
        /// Gets the additional overflow button items for the game
        /// </summary>
        /// <returns>The items</returns>
        public virtual OverflowButtonItemViewModel[] GetAdditionalOverflowButtonItems() => new OverflowButtonItemViewModel[0];

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
        /// Gets the info items for the specified game
        /// </summary>
        /// <returns>The info items</returns>
        public virtual IEnumerable<DuoGridItemViewModel> GetGameInfoItems()
        {
            yield return new DuoGridItemViewModel(Resources.GameInfo_GameType, GameTypeDisplayName, UserLevel.Advanced);
            yield return new DuoGridItemViewModel(Resources.GameInfo_InstallDir, Info.InstallDirectory);
        }

        #endregion

        #region Protected Abstract Methods

        /// <summary>
        /// Locates the game
        /// </summary>
        /// <returns>Null if the game was not found. Otherwise a valid or empty path for the instal directory</returns>
        protected abstract Task<FileSystemPath?> LocateAsync();

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

            RCFCore.Logger?.LogInformationSource($"The game {Game} has been added");
        }

        #endregion

        #region Public Abstract Methods

        /// <summary>
        /// Gets the launch info for the game
        /// </summary>
        /// <returns>The launch info</returns>
        public abstract GameLaunchInfo GetLaunchInfo();

        /// <summary>
        /// Indicates if the game is valid
        /// </summary>
        /// <param name="installDir">The game install directory, if any</param>
        /// <returns>True if the game is valid, otherwise false</returns>
        public abstract bool IsValid(FileSystemPath installDir);

        /// <summary>
        /// Gets the install directory for the game
        /// </summary>
        /// <returns>The install directory</returns>
        public abstract FileSystemPath GetInstallDirectory();

        /// <summary>
        /// Gets the icon resource path for the game based on its launch information
        /// </summary>
        /// <returns>The icon resource path</returns>
        public abstract string GetIconResourcePath();

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