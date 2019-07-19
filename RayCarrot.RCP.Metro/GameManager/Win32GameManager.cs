using System;
using System.Threading.Tasks;
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
            if (IsValid(result.SelectedDirectory))
                return result.SelectedDirectory;

            RCF.Logger.LogInformationSource($"The selected install directory for {Game} is not valid");

            await RCF.MessageUI.DisplayMessageAsync(Resources.LocateGame_InvalidLocation, Resources.LocateGame_InvalidLocationHeader, MessageType.Error);
            return null;

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

        /// <summary>
        /// Indicates if the game is valid
        /// </summary>
        /// <param name="installDir">The game install directory, if any</param>
        /// <returns>True if the game is valid, otherwise false</returns>
        public override bool IsValid(FileSystemPath installDir)
        {
            return (installDir + Game.GetLaunchName()).FileExists;
        }

        #endregion
    }
}