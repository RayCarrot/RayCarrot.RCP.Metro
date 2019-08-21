using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MahApps.Metro.IconPacks;
using Microsoft.Win32;
using RayCarrot.CarrotFramework.Abstractions;
using RayCarrot.IO;
using RayCarrot.UI;
using RayCarrot.Windows.Registry;

namespace RayCarrot.RCP.Metro
{
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
        /// <param name="type">The game type</param>
        public SteamGameManager(Games game, GameType type = GameType.Steam) : base(game, type)
        {

        }

        #endregion

        #region Protected Overrides Properties

        /// <summary>
        /// The display name for the game type
        /// </summary>
        public override string GameTypeDisplayName => Resources.GameType_Steam;

        /// <summary>
        /// Indicates if using <see cref="GameLaunchMode"/> is supported
        /// </summary>
        public override bool SupportsGameLaunchMode => false;

        #endregion

        #region Public Methods

        /// <summary>
        /// Gets a Steam ID for the specified game
        /// </summary>
        /// <returns>The Steam ID</returns>
        public string GetSteamID()
        {
            switch (Game)
            {
                case Games.Rayman2:
                    return "15060";

                case Games.RaymanRavingRabbids:
                    return "15080";

                case Games.RaymanOrigins:
                    return "207490";

                case Games.RaymanLegends:
                    return "242550";

                default:
                    throw new ArgumentOutOfRangeException(nameof(Game), Game, "The game is not a Steam game");
            }
        }

        #endregion

        #region Public Overrides

        /// <summary>
        /// Locates the game
        /// </summary>
        /// <returns>Null if the game was not found. Otherwise a valid or empty path for the instal directory</returns>
        public override async Task<FileSystemPath?> LocateAsync()
        {
            // Make sure the game is valid
            if (!IsValid(FileSystemPath.EmptyPath))
            {
                RCFCore.Logger?.LogInformationSource($"The {Game} was not found under Steam Apps");

                await RCFUI.MessageUI.DisplayMessageAsync(Resources.LocateGame_InvalidSteamGame, Resources.LocateGame_InvalidSteamGame, MessageType.Error);
                return null;
            }

            return FileSystemPath.EmptyPath;
        }

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
                    (await RCFRCP.File.LaunchFileAsync($"https://store.steampowered.com/app/" + GetSteamID()))?.Dispose();
                    RCFCore.Logger?.LogTraceSource($"The game {Game} Steam store page was opened");
                })),
                new OverflowButtonItemViewModel(Resources.GameDisplay_OpenSteamCommunity, PackIconMaterialKind.Steam, new AsyncRelayCommand(async () =>
                {
                    (await RCFRCP.File.LaunchFileAsync($"https://steamcommunity.com/app/" + GetSteamID()))?.Dispose();
                    RCFCore.Logger?.LogTraceSource($"The game {Game} Steam community page was opened");
                }))
            };
        }

        /// <summary>
        /// Gets the launch info for the game
        /// </summary>
        /// <returns>The launch info</returns>
        public override GameLaunchInfo GetLaunchInfo()
        {
            return new GameLaunchInfo(@"steam://rungameid/" + GetSteamID(), null);
        }

        /// <summary>
        /// Indicates if the game is valid
        /// </summary>
        /// <param name="installDir">The game install directory, if any</param>
        /// <returns>True if the game is valid, otherwise false</returns>
        public override bool IsValid(FileSystemPath installDir)
        {
            return RCFWinReg.RegistryManager.KeyExists(RCFWinReg.RegistryManager.CombinePaths(CommonRegistryPaths.InstalledPrograms, $"Steam App {GetSteamID()}"), RegistryView.Registry64);
        }

        /// <summary>
        /// Gets the install directory for the game
        /// </summary>
        /// <returns>The install directory</returns>
        public override FileSystemPath GetInstallDirectory()
        {
            try
            {
                // Get the key path
                var keyPath = RCFWinReg.RegistryManager.CombinePaths(CommonRegistryPaths.InstalledPrograms, $"Steam App {GetSteamID()}");

                using (var key = RCFWinReg.RegistryManager.GetKeyFromFullPath(keyPath, RegistryView.Registry64))
                    return key?.GetValue("InstallLocation") as string;
            }
            catch (Exception ex)
            {
                ex.HandleError("Getting Steam game install directory");
                return FileSystemPath.EmptyPath;
            }
        }

        /// <summary>
        /// Gets the info items for the specified game
        /// </summary>
        /// <returns>The info items</returns>
        public override IEnumerable<DuoGridItemViewModel> GetGameInfoItems()
        {
            // Return from base
            foreach (var item in base.GetGameInfoItems())
                yield return item;

            // Return new items
            yield return new DuoGridItemViewModel(Resources.GameInfo_SteamID, GetSteamID(), UserLevel.Advanced);
        }

        /// <summary>
        /// Gets the icon resource path for the game based on its launch information
        /// </summary>
        /// <returns>The icon resource path</returns>
        public override string GetIconResourcePath() => Info.InstallDirectory + Game.GetLaunchName();

        #endregion
    }
}