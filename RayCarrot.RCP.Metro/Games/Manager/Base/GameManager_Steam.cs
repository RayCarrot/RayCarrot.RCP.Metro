using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MahApps.Metro.IconPacks;
using Microsoft.Win32;
using RayCarrot.IO;
using RayCarrot.Logging;
using RayCarrot.UI;
using RayCarrot.Windows.Registry;
using RayCarrot.WPF;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// Base for a Steam Rayman Control Panel game
    /// </summary>
    public abstract class GameManager_Steam : GameManager
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
        /// Indicates if using <see cref="UserData_GameLaunchMode"/> is supported
        /// </summary>
        public override bool SupportsGameLaunchMode => false;

        /// <summary>
        /// Gets the additional overflow button items for the game
        /// </summary>
        public override IList<OverflowButtonItemViewModel> GetAdditionalOverflowButtonItems => new OverflowButtonItemViewModel[]
        {
            new OverflowButtonItemViewModel(Resources.GameDisplay_OpenSteamStore, PackIconMaterialKind.Steam, new AsyncRelayCommand(async () =>
            {
                (await RCPServices.File.LaunchFileAsync($"https://store.steampowered.com/app/" + SteamID))?.Dispose();
                RL.Logger?.LogTraceSource($"The game {Game} Steam store page was opened");
            })),
            new OverflowButtonItemViewModel(Resources.GameDisplay_OpenSteamCommunity, PackIconMaterialKind.Steam, new AsyncRelayCommand(async () =>
            {
                (await RCPServices.File.LaunchFileAsync($"https://steamcommunity.com/app/" + SteamID))?.Dispose();
                RL.Logger?.LogTraceSource($"The game {Game} Steam community page was opened");
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
        public override GameFinder_GameItem GameFinderItem => new GameFinder_GameItem(SteamID);

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
                new JumpListItemViewModel(info.DisplayName, Game.GetInstallDir() + info.DefaultFileName, LaunchURL, null, null, Game.ToString())
            };
        }

        /// <summary>
        /// The implementation for launching the game
        /// </summary>
        /// <param name="forceRunAsAdmin">Indicated if the game should be forced to run as admin</param>
        /// <returns>The launch result</returns>
        protected override async Task<GameLaunchResult> LaunchAsync(bool forceRunAsAdmin)
        {
            RL.Logger?.LogTraceSource($"The game {Game} is launching with Steam ID {SteamID}");

            // Launch the game
            var process = await RCPServices.File.LaunchFileAsync(LaunchURL);

            RL.Logger?.LogInformationSource($"The game {Game} has been launched");

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
            FileSystemPath installDir;

            try
            {
                // Get the key path
                var keyPath = RegistryHelpers.CombinePaths(CommonRegistryPaths.InstalledPrograms, $"Steam App {SteamID}");

                using var key = RegistryHelpers.GetKeyFromFullPath(keyPath, RegistryView.Registry64);

                // Get the install directory
                if (!(key?.GetValue("InstallLocation") is string dir))
                {
                    RL.Logger?.LogInformationSource($"The {Game} was not found under Steam Apps");

                    await Services.MessageUI.DisplayMessageAsync(Resources.LocateGame_InvalidSteamGame, Resources.LocateGame_InvalidSteamGameHeader, MessageType.Error);

                    return null;
                }

                installDir = dir;
            }
            catch (Exception ex)
            {
                ex.HandleError("Getting Steam game install directory");

                await Services.MessageUI.DisplayMessageAsync(Resources.LocateGame_InvalidSteamGame, Resources.LocateGame_InvalidSteamGameHeader, MessageType.Error);

                return null;
            }

            // Make sure the game is valid
            if (!await IsValidAsync(installDir))
            {
                RL.Logger?.LogInformationSource($"The {Game} install directory was not valid");

                await Services.MessageUI.DisplayMessageAsync(Resources.LocateGame_InvalidSteamGame, Resources.LocateGame_InvalidSteamGameHeader, MessageType.Error);

                return null;
            }

            return installDir;
        }

        /// <summary>
        /// Creates a shortcut to launch the game from
        /// </summary>
        /// <param name="shortcutName">The name of the shortcut</param>
        /// <param name="destinationDirectory">The destination directory for the shortcut</param>
        public override void CreateGameShortcut(FileSystemPath shortcutName, FileSystemPath destinationDirectory)
        {
            RCPServices.File.CreateURLShortcut(shortcutName, destinationDirectory, LaunchURL);

            RL.Logger?.LogTraceSource($"An URL shortcut was created for {Game} under {destinationDirectory}");
        }

        #endregion
    }
}