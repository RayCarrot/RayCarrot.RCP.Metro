using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MahApps.Metro.IconPacks;
using Microsoft.Win32;
using RayCarrot.CarrotFramework.Abstractions;
using RayCarrot.IO;
using RayCarrot.UI;
using RayCarrot.Windows.Registry;
using RayCarrot.Windows.Shell;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// Base for a Steam Rayman Control Panel game
    /// </summary>
    public abstract class RCPSteamGame : RCPGameManager
    {
        #region Public Override Properties

        /// <summary>
        /// The game type
        /// </summary>
        public override GameType Type => GameType.Steam;

        /// <summary>
        /// The display name for the game type
        /// </summary>
        public override string GameTypeDisplayName => Resources.GameType_Steam;

        /// <summary>
        /// Indicates if using <see cref="GameLaunchMode"/> is supported
        /// </summary>
        public override bool SupportsGameLaunchMode => false;

        /// <summary>
        /// Gets the additional overflow button items for the game
        /// </summary>
        public override IList<OverflowButtonItemViewModel> GetAdditionalOverflowButtonItems => new OverflowButtonItemViewModel[]
        {
            new OverflowButtonItemViewModel(Resources.GameDisplay_OpenSteamStore, PackIconMaterialKind.Steam, new AsyncRelayCommand(async () =>
            {
                (await RCFRCP.File.LaunchFileAsync($"https://store.steampowered.com/app/" + SteamID))?.Dispose();
                RCFCore.Logger?.LogTraceSource($"The game {Game} Steam store page was opened");
            })),
            new OverflowButtonItemViewModel(Resources.GameDisplay_OpenSteamCommunity, PackIconMaterialKind.Steam, new AsyncRelayCommand(async () =>
            {
                (await RCFRCP.File.LaunchFileAsync($"https://steamcommunity.com/app/" + SteamID))?.Dispose();
                RCFCore.Logger?.LogTraceSource($"The game {Game} Steam community page was opened");
            }))
        };

        /// <summary>
        /// Gets the info items for the game
        /// </summary>
        public override IList<DuoGridItemViewModel> GetGameInfoItems => new List<DuoGridItemViewModel>(base.GetGameInfoItems)
        {
            new DuoGridItemViewModel(Resources.GameInfo_SteamID, SteamID, UserLevel.Advanced)
        };

        /// <summary>
        /// Gets the purchase links for the game
        /// </summary>
        public override IList<GamePurchaseLink> GetGamePurchaseLinks => new GamePurchaseLink[]
        {
            new GamePurchaseLink(Resources.GameDisplay_Steam, $"https://store.steampowered.com/app/" + SteamID),
        };

        /// <summary>
        /// Gets the game finder item for this game
        /// </summary>
        public override GameFinderItem GameFinderItem => new GameFinderItem(SteamID);

        #endregion

        #region Public Abstract Properties

        /// <summary>
        /// Gets the Steam ID for the game
        /// </summary>
        public abstract string SteamID { get; }

        #endregion

        #region Public Properties

        /// <summary>
        /// The URL used to launch the Steam game
        /// </summary>
        public string LaunchURL => @"steam://rungameid/" + SteamID;

        #endregion

        #region Protected Override Methods

        /// <summary>
        /// Gets the available jump list items for this game
        /// </summary>
        /// <returns>The items</returns>
        public override IList<JumpListItemViewModel> GetJumpListItems()
        {
            // Get the game info
            var info = Game.GetGameInfo();

            return new JumpListItemViewModel[]
            {
                new JumpListItemViewModel(info.DisplayName, GameData.InstallDirectory + info.DefaultFileName, LaunchURL, null, null, Game.ToString())
            };
        }

        /// <summary>
        /// The implementation for launching the game
        /// </summary>
        /// <param name="forceRunAsAdmin">Indicated if the game should be forced to run as admin</param>
        /// <returns>The launch result</returns>
        protected override async Task<GameLaunchResult> LaunchAsync(bool forceRunAsAdmin)
        {
            RCFCore.Logger?.LogTraceSource($"The game {Game} is launching with Steam ID {SteamID}");

            // Launch the game
            var process = await RCFRCP.File.LaunchFileAsync(LaunchURL);

            RCFCore.Logger?.LogInformationSource($"The game {Game} has been launched");

            return new GameLaunchResult(process, process != null);
        }

        #endregion

        #region Public Override Methods

        /// <summary>
        /// Locates the game
        /// </summary>
        /// <returns>Null if the game was not found. Otherwise a valid or empty path for the install directory</returns>
        public override async Task<FileSystemPath?> LocateAsync()
        {
            // Make sure the game is valid
            if (!await IsValidAsync(FileSystemPath.EmptyPath))
            {
                RCFCore.Logger?.LogInformationSource($"The {Game} was not found under Steam Apps");

                await RCFUI.MessageUI.DisplayMessageAsync(Resources.LocateGame_InvalidSteamGame, Resources.LocateGame_InvalidSteamGameHeader, MessageType.Error);

                return null;
            }

            try
            {
                // Get the key path
                var keyPath = RCFWinReg.RegistryManager.CombinePaths(CommonRegistryPaths.InstalledPrograms, $"Steam App {SteamID}");

                using var key = RCFWinReg.RegistryManager.GetKeyFromFullPath(keyPath, RegistryView.Registry64);

                // Return the install directory
                return key?.GetValue("InstallLocation") as string;
            }
            catch (Exception ex)
            {
                ex.HandleError("Getting Steam game install directory");

                // TODO: Show error message

                return null;
            }
        }

        /// <summary>
        /// Indicates if the game is valid
        /// </summary>
        /// <param name="installDir">The game install directory, if any</param>
        /// <returns>True if the game is valid, otherwise false</returns>
        public override async Task<bool> IsValidAsync(FileSystemPath installDir)
        {
            return await base.IsValidAsync(installDir) && RCFWinReg.RegistryManager.KeyExists(RCFWinReg.RegistryManager.CombinePaths(CommonRegistryPaths.InstalledPrograms, $"Steam App {SteamID}"), RegistryView.Registry64);
        }

        /// <summary>
        /// Creates a shortcut to launch the game from
        /// </summary>
        /// <param name="shortcutName">The name of the shortcut</param>
        /// <param name="destinationDirectory">The destination directory for the shortcut</param>
        /// <returns>The task</returns>
        public override Task CreateGameShortcut(FileSystemPath shortcutName, FileSystemPath destinationDirectory)
        {
            WindowsHelpers.CreateURLShortcut(shortcutName, destinationDirectory, LaunchURL);

            RCFCore.Logger?.LogTraceSource($"An URL shortcut was created for {Game} under {destinationDirectory}");

            return Task.CompletedTask;
        }

        #endregion
    }
}