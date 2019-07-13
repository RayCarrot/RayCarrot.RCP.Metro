using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using MahApps.Metro.IconPacks;
using RayCarrot.CarrotFramework;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// The game manager for a <see cref="GameType.Win32"/> game
    /// </summary>
    public class Win32GameManager : BaseGameManager
    {
        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="game">The game to manage</param>
        /// <param name="type">The game type</param>
        public Win32GameManager(Games game, GameType type = GameType.Win32) : base(game, type)
        {

        }

        #endregion

        #region Protected Overrides

        /// <summary>
        /// Locates the game
        /// </summary>
        /// <returns>Null if the game was not found. Otherwise a valid or empty path for the instal directory</returns>
        protected override async Task<FileSystemPath?> LocateAsync()
        {
            var result = await RCF.BrowseUI.BrowseDirectoryAsync(new DirectoryBrowserViewModel()
            {
                Title = Resources.LocateGame_BrowserHeader,
                DefaultDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),
                MultiSelection = false
            });

            if (result.CanceledByUser)
                return null;

            if (!result.SelectedDirectory.DirectoryExists)
                return null;

            // Make sure the game is valid
            if (!Game.IsValid(GameType.Win32, result.SelectedDirectory))
            {
                RCF.Logger.LogInformationSource($"The selected install directory for {Game} is not valid");

                await RCF.MessageUI.DisplayMessageAsync(Resources.LocateGame_InvalidLocation, Resources.LocateGame_InvalidLocationHeader, MessageType.Error);
                return null;
            }

            return result.SelectedDirectory;
        }

        #endregion

        #region Public Overrides

        /// <summary>
        /// Gets the launch info for the game
        /// </summary>
        /// <returns>The launch info</returns>
        public override GameLaunchInfo GetLaunchInfo()
        {
            return new GameLaunchInfo(Info.InstallDirectory + Game.GetLaunchName(), null);
        }

        #endregion
    }

    /// <summary>
    /// The game manager for a <see cref="GameType.Steam"/> game
    /// </summary>
    public class SteamGameManager : BaseGameManager
    {
        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="game">The game to manage</param>
        public SteamGameManager(Games game) : base(game, GameType.Steam)
        {

        }

        #endregion

        #region Protected Overrides

        /// <summary>
        /// Locates the game
        /// </summary>
        /// <returns>Null if the game was not found. Otherwise a valid or empty path for the instal directory</returns>
        protected override async Task<FileSystemPath?> LocateAsync()
        {
            // Make sure the game is valid
            if (!Game.IsValid(GameType.Steam, FileSystemPath.EmptyPath))
            {
                RCF.Logger.LogInformationSource($"The {Game} was not found under Steam Apps");

                await RCF.MessageUI.DisplayMessageAsync(Resources.LocateGame_InvalidSteamGame, Resources.LocateGame_InvalidSteamGame, MessageType.Error);
                return null;
            }

            return FileSystemPath.EmptyPath;
        }

        #endregion

        #region Public Overrides

        /// <summary>
        /// Gets the additional overflow button items for the game
        /// </summary>
        /// <returns>The items</returns>
        public override OverflowButtonItemViewModel[] GetAdditionalOverflowButtonItems()
        {
            // Add Steam links
            return new OverflowButtonItemViewModel[]
            {
                new OverflowButtonItemViewModel(Resources.GameDisplay_OpenSteamStore, PackIconMaterialKind.Steam, new AsyncRelayCommand(async () =>
                {
                    (await RCFRCP.File.LaunchFileAsync($"https://store.steampowered.com/app/" + Game.GetSteamID()))?.Dispose();
                    RCF.Logger.LogTraceSource($"The game {Game} Steam store page was opened");
                })),
                new OverflowButtonItemViewModel(Resources.GameDisplay_OpenSteamCommunity, PackIconMaterialKind.Steam, new AsyncRelayCommand(async () =>
                {
                    (await RCFRCP.File.LaunchFileAsync($"https://steamcommunity.com/app/" + Game.GetSteamID()))?.Dispose();
                    RCF.Logger.LogTraceSource($"The game {Game} Steam community page was opened");
                }))
            };
        }

        /// <summary>
        /// Gets the launch info for the game
        /// </summary>
        /// <returns>The launch info</returns>
        public override GameLaunchInfo GetLaunchInfo()
        {
            return new GameLaunchInfo(@"steam://rungameid/" + Game.GetSteamID(), null);
        }

        #endregion
    }

    /// <summary>
    /// The game manager for a <see cref="GameType.DosBox"/> game
    /// </summary>
    public class DOSBoxGameManager : Win32GameManager
    {
        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="game">The game to manage</param>
        public DOSBoxGameManager(Games game) : base(game, GameType.DosBox)
        {

        }

        #endregion

        #region Protected Overrides

        /// <summary>
        /// Post launch operations for the game which launched
        /// </summary>
        /// <param name="process">The game process</param>
        /// <returns>The task</returns>
        protected override Task PostLaunchAsync(Process process)
        {
            // Check if TPLS should run
            if (Game == Games.Rayman1 && RCFRCP.Data.TPLSData?.IsEnabled == true)
                // Start TPLS
                new TPLS().Start(process);
            else
                return base.PostLaunchAsync(process);

            return Task.CompletedTask;
        }

        protected override async Task<GameLaunchResult> LaunchAsync(bool forceRunAsAdmin)
        {
            if (Game != Games.Rayman1 || RCFRCP.Data.TPLSData?.IsEnabled != true)
                return await base.LaunchAsync(forceRunAsAdmin);

            // Handle Rayman 1 differently if TPLS is enabled
            // TODO: This should be moved into a utility class which then injects code here

            var launchInfo = new GameLaunchInfo(RCFRCP.Data.DosBoxPath, Games.Rayman1.GetDosBoxArguments(Games.Rayman1.GetInfo().InstallDirectory, RCFRCP.Data.TPLSData.InstallDir + "RayCD.cue", Game.GetLaunchName()));

            RCF.Logger.LogTraceSource($"The game {Game} launch info has been retrieved as Path = {launchInfo.Path}, Args = {launchInfo.Args}");

            // Launch the game
            var process = await RCFRCP.File.LaunchFileAsync(launchInfo.Path, forceRunAsAdmin || Info.LaunchMode == GameLaunchMode.AsAdmin, launchInfo.Args);

            RCF.Logger.LogInformationSource($"The game {Game} has been launched in TPLS mode");

            return new GameLaunchResult(process, process != null);
        }

        /// <summary>
        /// Verifies if the game can launch
        /// </summary>
        /// <returns>True if the game can launch, otherwise false</returns>
        protected override async Task<bool> VerifyCanLaunchAsync()
        {
            // Make sure the DosBox executable exists
            if (!File.Exists(RCFRCP.Data.DosBoxPath))
            {
                await RCF.MessageUI.DisplayMessageAsync(Resources.LaunchGame_DosBoxNotFound, MessageType.Error);
                return false;
            }

            // Make sure the mount path exists, unless the game is Rayman 1 and TPLS is enabled
            if (!RCFRCP.Data.DosBoxGames[Game].MountPath.Exists && !(Game == Games.Rayman1 && RCFRCP.Data.TPLSData?.IsEnabled == true))
            {
                await RCF.MessageUI.DisplayMessageAsync(Resources.LaunchGame_MountPathNotFound, MessageType.Error);
                return false;
            }

            return true;
        }

        #endregion

        #region Public Overrides

        /// <summary>
        /// Gets the launch info for the game
        /// </summary>
        /// <returns>The launch info</returns>
        public override GameLaunchInfo GetLaunchInfo()
        {
            var options = RCFRCP.Data.DosBoxGames[Game];
            return new GameLaunchInfo(RCFRCP.Data.DosBoxPath, Game.GetDosBoxArguments(Info.InstallDirectory, options.MountPath, Game.GetLaunchName()));
        }

        #endregion
    }

    /// <summary>
    /// The game manager for a <see cref="GameType.WinStore"/> game
    /// </summary>
    public class WinStoreGameManager : BaseGameManager
    {
        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="game">The game to manage</param>
        public WinStoreGameManager(Games game) : base(game, GameType.WinStore)
        {

        }

        #endregion

        #region Protected Overrides

        /// <summary>
        /// The implementation for launching the game
        /// </summary>
        /// <param name="forceRunAsAdmin">Indicated if the game should be forced to run as admin</param>
        /// <returns>The launch result</returns>
        protected override async Task<GameLaunchResult> LaunchAsync(bool forceRunAsAdmin)
        {
            try
            {
                // Launch the first app entry for the package
                await Game.LaunchFirstPackageEntryAsync();

                return new GameLaunchResult(null, true);
            }
            catch (Exception ex)
            {
                ex.HandleError("Launching Windows Store application");
                await RCF.MessageUI.DisplayMessageAsync(String.Format(Resources.LaunchGame_WinStoreError, Game.GetDisplayName()), MessageType.Error);

                return new GameLaunchResult(null, false);
            }
        }

        /// <summary>
        /// Locates the game
        /// </summary>
        /// <returns>Null if the game was not found. Otherwise a valid or empty path for the instal directory</returns>
        protected override async Task<FileSystemPath?> LocateAsync()
        {
            // Helper method for finding and adding a Windows Store app
            bool FindWinStoreApp() =>
                // Check if the game is installed
                Game.IsValid(GameType.WinStore, FileSystemPath.EmptyPath);

            bool found;

            if (Game == Games.RaymanFiestaRun)
            {
                RCFRCP.Data.IsFiestaRunWin10Edition = true;

                found = FindWinStoreApp();

                if (!found)
                {
                    RCFRCP.Data.IsFiestaRunWin10Edition = false;

                    found = FindWinStoreApp();
                }
            }
            else
            {
                found = FindWinStoreApp();
            }

            if (!found)
            {
                RCF.Logger.LogInformationSource($"The {Game} was not found under Windows Store packages");

                await RCF.MessageUI.DisplayMessageAsync(Resources.LocateGame_InvalidWinStoreGame, Resources.LocateGame_InvalidWinStoreGameHeader, MessageType.Error);

                return null;
            }

            return FileSystemPath.EmptyPath;
        }

        #endregion

        #region Public Overrides

        /// <summary>
        /// Gets the launch info for the game
        /// </summary>
        /// <returns>The launch info</returns>
        public override GameLaunchInfo GetLaunchInfo()
        {
            // NOTE: This method of launching a WinStore game should only be used when no other method is available. If the package is not found this method will launch a new Windows Explorer window instead. The entry point ("!APP") may not always be correct (that is the default used for most packages with a single entry point).

            return new GameLaunchInfo("shell:appsFolder\\" + $"{Game.GetLaunchName()}!App", null);
        }

        #endregion
    }

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

            RCF.Logger.LogTraceSource($"The game {Game} launch info has been retrieved as Path = {launchInfo.Path}, Args = {launchInfo.Args}");

            // Launch the game
            var process = await RCFRCP.File.LaunchFileAsync(launchInfo.Path, forceRunAsAdmin || Info.LaunchMode == GameLaunchMode.AsAdmin, launchInfo.Args);

            RCF.Logger.LogInformationSource($"The game {Game} has been launched");

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
            RCF.Logger.LogTraceSource($"The game {Game} is being launched...");

            // Verify that the game can launch
            if (!await VerifyCanLaunchAsync())
            {
                RCF.Logger.LogInformationSource($"The game {Game} could not be launched");
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

            RCF.Logger.LogInformationSource($"The game {Game} has been added");
        }

        #endregion

        #region Public Abstract Methods

        /// <summary>
        /// Gets the launch info for the game
        /// </summary>
        /// <returns>The launch info</returns>
        public abstract GameLaunchInfo GetLaunchInfo();

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