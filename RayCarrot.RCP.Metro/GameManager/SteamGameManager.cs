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
            if (!IsValid(FileSystemPath.EmptyPath))
            {
                RCFCore.Logger?.LogInformationSource($"The {Game} was not found under Steam Apps");

                await RCFUI.MessageUI.DisplayMessageAsync(Resources.LocateGame_InvalidSteamGame, Resources.LocateGame_InvalidSteamGame, MessageType.Error);
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
                    RCFCore.Logger?.LogTraceSource($"The game {Game} Steam store page was opened");
                })),
                new OverflowButtonItemViewModel(Resources.GameDisplay_OpenSteamCommunity, PackIconMaterialKind.Steam, new AsyncRelayCommand(async () =>
                {
                    (await RCFRCP.File.LaunchFileAsync($"https://steamcommunity.com/app/" + Game.GetSteamID()))?.Dispose();
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
            return new GameLaunchInfo(@"steam://rungameid/" + Game.GetSteamID(), null);
        }

        /// <summary>
        /// Indicates if the game is valid
        /// </summary>
        /// <param name="installDir">The game install directory, if any</param>
        /// <returns>True if the game is valid, otherwise false</returns>
        public override bool IsValid(FileSystemPath installDir)
        {
            return RCFWinReg.RegistryManager.KeyExists(RCFWinReg.RegistryManager.CombinePaths(CommonRegistryPaths.InstalledPrograms, $"Steam App {Game.GetSteamID()}"), RegistryView.Registry64);
        }

        #endregion
    }
}