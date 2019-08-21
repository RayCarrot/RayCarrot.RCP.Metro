using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RayCarrot.CarrotFramework.Abstractions;
using RayCarrot.IO;
using RayCarrot.UI;

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

        #region Protected Overrides Properties

        /// <summary>
        /// The display name for the game type
        /// </summary>
        public override string GameTypeDisplayName => Resources.GameType_Desktop;

        /// <summary>
        /// Indicates if using <see cref="GameLaunchMode"/> is supported
        /// </summary>
        public override bool SupportsGameLaunchMode => true;

        #endregion

        #region Public Overrides

        /// <summary>
        /// Locates the game
        /// </summary>
        /// <returns>Null if the game was not found. Otherwise a valid or empty path for the instal directory</returns>
        public override async Task<FileSystemPath?> LocateAsync()
        {
            var result = await RCFUI.BrowseUI.BrowseDirectoryAsync(new DirectoryBrowserViewModel()
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
            if (IsValid(result.SelectedDirectory))
                return result.SelectedDirectory;

            RCFCore.Logger?.LogInformationSource($"The selected install directory for {Game} is not valid");

            await RCFUI.MessageUI.DisplayMessageAsync(Resources.LocateGame_InvalidLocation, Resources.LocateGame_InvalidLocationHeader, MessageType.Error);
            return null;

        }

        /// <summary>
        /// Gets the launch info for the game
        /// </summary>
        /// <returns>The launch info</returns>
        public override GameLaunchInfo GetLaunchInfo()
        {
            return new GameLaunchInfo(Info.InstallDirectory + Game.GetLaunchName(), null);
        }

        /// <summary>
        /// Indicates if the game is valid
        /// </summary>
        /// <param name="installDir">The game install directory, if any</param>
        /// <returns>True if the game is valid, otherwise false</returns>
        public override bool IsValid(FileSystemPath installDir)
        {
            return (installDir + Game.GetLaunchName()).FileExists;
        }

        /// <summary>
        /// Gets the install directory for the game
        /// </summary>
        /// <returns>The install directory</returns>
        public override FileSystemPath GetInstallDirectory() => FileSystemPath.EmptyPath;

        /// <summary>
        /// Gets the info items for the specified game
        /// </summary>
        /// <returns>The info items</returns>
        public override IEnumerable<DuoGridItemViewModel> GetGameInfoItems()
        {
            // Return from base
            foreach (var item in base.GetGameInfoItems())
                yield return item;

            var launchInfo = GetLaunchInfo();

            // Return new items
            yield return new DuoGridItemViewModel(Resources.GameInfo_LaunchPath, launchInfo.Path, UserLevel.Technical);
            yield return new DuoGridItemViewModel(Resources.GameInfo_LaunchArgs, launchInfo.Args, UserLevel.Technical);
        }

        /// <summary>
        /// Gets the icon resource path for the game based on its launch information
        /// </summary>
        /// <returns>The icon resource path</returns>
        public override string GetIconResourcePath() => GetLaunchInfo().Path;

        #endregion
    }
}