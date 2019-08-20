using RayCarrot.CarrotFramework.Abstractions;
using RayCarrot.IO;
using RayCarrot.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// The game manager for a <see cref="GameType.EducationalDosBox"/> game
    /// </summary>
    public class EducationalDosBoxGameManager : DOSBoxGameManager
    {
        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="game">The game to manage</param>
        /// <param name="type">The game type</param>
        public EducationalDosBoxGameManager(Games game, GameType type = GameType.EducationalDosBox) : base(game, type)
        {

        }

        #endregion

        #region Protected Overrides Properties

        /// <summary>
        /// The display name for the game type
        /// </summary>
        // TODO: Localize as "GameType_EducationalDosBox"
        public override string GameTypeDisplayName => "Educational DOSBox";

        #endregion

        #region Protected Overrides

        /// <summary>
        /// Locates the game
        /// </summary>
        /// <returns>Null if the game was not found. Otherwise a valid or empty path for the instal directory</returns>
        protected override async Task<FileSystemPath?> LocateAsync()
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

            // Check if the location if valid
            if (!IsValid(result.SelectedDirectory))
            {
                RCFCore.Logger?.LogInformationSource($"The selected install directory for {Game} is not valid");

                await RCFUI.MessageUI.DisplayMessageAsync(Resources.LocateGame_InvalidLocation, Resources.LocateGame_InvalidLocationHeader, MessageType.Error);
                return null;
            }

            return result.SelectedDirectory;
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
                await RCFUI.MessageUI.DisplayMessageAsync(Resources.LaunchGame_DosBoxNotFound, MessageType.Error);
                return false;
            }

            // Make sure the mount path exists, unless the game is Rayman 1 and TPLS is enabled
            if (!RCFRCP.Data.EducationalDosBoxGames.First().MountPath.Exists)
            {
                await RCFUI.MessageUI.DisplayMessageAsync(Resources.LaunchGame_MountPathNotFound, MessageType.Error);
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
            // Get the default game
            var defaultGame = RCFRCP.Data.EducationalDosBoxGames.First();

            return new GameLaunchInfo(RCFRCP.Data.DosBoxPath, GetDosBoxArguments(defaultGame.MountPath, defaultGame.LaunchName));
        }

        /// <summary>
        /// Gets called as soon as the game is added for the first time
        /// </summary>
        /// <returns>The task</returns>
        public override Task PostGameAddAsync()
        {
            // Create config file
            new DosBoxAutoConfigManager(Game.GetDosBoxConfigFile()).Create();

            // Get the install directory
            var instDir = Game.GetInfo().InstallDirectory;

            // TODO: Try/catch
            // Find the launch name
            FileSystemPath launchName = Directory.EnumerateFiles(instDir, "*.exe", SearchOption.TopDirectoryOnly).FirstOrDefault();

            // TODO: Make sure launch name is not null

            // Add the game to the list of educational games
            RCFRCP.Data.EducationalDosBoxGames = new List<EducationalDosBoxGameInfo>()
            {
                new EducationalDosBoxGameInfo(null, instDir, launchName.Name)
                {
                    Name = instDir.Name
                }
            };

            return Task.CompletedTask;
        }

        /// <summary>
        /// Gets called as soon as the game is removed
        /// </summary>
        /// <returns>The task</returns>
        public override Task PostGameRemovedAsync()
        {
            RCFRCP.Data.EducationalDosBoxGames = null;
            return base.PostGameRemovedAsync();
        }

        /// <summary>
        /// Indicates if the game is valid
        /// </summary>
        /// <param name="installDir">The game install directory, if any</param>
        /// <returns>True if the game is valid, otherwise false</returns>
        public override bool IsValid(FileSystemPath installDir)
        {
            return (installDir + "PCMAP").DirectoryExists;
        }

        #endregion
    }
}