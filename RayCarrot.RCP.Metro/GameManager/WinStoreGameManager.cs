using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using RayCarrot.CarrotFramework.Abstractions;
using RayCarrot.IO;
using RayCarrot.UI;
using RayCarrot.Windows.Shell;

namespace RayCarrot.RCP.Metro
{
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
                await RCFUI.MessageUI.DisplayMessageAsync(String.Format(Resources.LaunchGame_WinStoreError, Game.GetDisplayName()), MessageType.Error);

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
                IsValid(FileSystemPath.EmptyPath);

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
                RCFCore.Logger?.LogInformationSource($"The {Game} was not found under Windows Store packages");

                await RCFUI.MessageUI.DisplayMessageAsync(Resources.LocateGame_InvalidWinStoreGame, Resources.LocateGame_InvalidWinStoreGameHeader, MessageType.Error);

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

        /// <summary>
        /// Indicates if the game is valid
        /// </summary>
        /// <param name="installDir">The game install directory, if any</param>
        /// <returns>True if the game is valid, otherwise false</returns>
        [MethodImpl(MethodImplOptions.NoInlining)]
        public override bool IsValid(FileSystemPath installDir)
        {
            // Make sure version is at least Windows 8
            if (AppViewModel.WindowsVersion < WindowsVersion.Win8)
                return false;

            return Game.GetGamePackage() != null;
        }

        #endregion
    }
}