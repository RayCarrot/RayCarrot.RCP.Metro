using RayCarrot.CarrotFramework.Abstractions;
using RayCarrot.IO;
using RayCarrot.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

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
        /// Verifies if the game can launch
        /// </summary>
        /// <returns>True if the game can launch, otherwise false</returns>
        protected override async Task<bool> VerifyCanLaunchAsync()
        {
            return await VerifyCanLaunchAsync(RCFRCP.Data.EducationalDosBoxGames.First());
        }

        #endregion

        #region Public Overrides

        /// <summary>
        /// Gets the additional overflow button items for the game
        /// </summary>
        /// <returns>The items</returns>
        public override OverflowButtonItemViewModel[] GetAdditionalOverflowButtonItems()
        {
            if (!RCFRCP.Data.EducationalDosBoxGames.Any())
                return new OverflowButtonItemViewModel[0];

            return RCFRCP.Data.EducationalDosBoxGames.
                Select(x => new OverflowButtonItemViewModel(x.Name, new BitmapImage(new Uri(AppViewModel.ApplicationBasePath + @"img\GameIcons\EducationalDos.png")), new AsyncRelayCommand(async () =>
            {
                RCFCore.Logger?.LogTraceSource($"The educational game {x.ID} is being launched...");

                // Verify that the game can launch
                if (!await VerifyCanLaunchAsync(x))
                {
                    RCFCore.Logger?.LogInformationSource($"The educational game {x.ID} could not be launched");
                    return;
                }

                // Get the launch info
                GameLaunchInfo launchInfo = GetLaunchInfo(x);

                RCFCore.Logger?.LogTraceSource($"The educational game {x.ID} launch info has been retrieved as Path = {launchInfo.Path}, Args = {launchInfo.Args}");

                // Launch the game
                var process = await RCFRCP.File.LaunchFileAsync(launchInfo.Path, Info.LaunchMode == GameLaunchMode.AsAdmin, launchInfo.Args);

                RCFCore.Logger?.LogInformationSource($"The educational game {x.ID} has been launched");

                if (process != null)
                    // Run any post launch operations on the process
                    await PostLaunchAsync(process);

            }))).ToArray();
        }

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
        /// Gets the launch info for the game
        /// </summary>
        /// <returns>The launch info</returns>
        public override GameLaunchInfo GetLaunchInfo()
        {
            // Get the default game
            var defaultGame = RCFRCP.Data.EducationalDosBoxGames.First();

            return GetLaunchInfo(defaultGame);
        }

        /// <summary>
        /// Gets called as soon as the game is added for the first time
        /// </summary>
        /// <returns>The task</returns>
        public override Task PostGameAddAsync()
        {
            // Create config file
            new DosBoxAutoConfigManager(Game.GetDosBoxConfigFile()).Create();

            // Create the collection of games
            RCFRCP.Data.EducationalDosBoxGames = new List<EducationalDosBoxGameInfo>();

            // Add the game to the list of educational games
            AddEducationalDosBoxGameInfo(Game.GetInfo().InstallDirectory);

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
            // TODO: Make sure each added game is valid too, remove if not and return false if no games are left

            return (installDir + "PCMAP").DirectoryExists;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Adds a new educational DOSBox game
        /// </summary>
        /// <param name="installDir">The install directory</param>
        public void AddEducationalDosBoxGameInfo(FileSystemPath installDir)
        {
            // Find the launch name
            FileSystemPath launchName = Directory.EnumerateFiles(installDir, "*.exe", SearchOption.TopDirectoryOnly).FirstOrDefault();

            // Create the collection if it doesn't exist
            if (RCFRCP.Data.EducationalDosBoxGames == null)
                RCFRCP.Data.EducationalDosBoxGames = new List<EducationalDosBoxGameInfo>();

            // Add the game to the list of educational games
            RCFRCP.Data.EducationalDosBoxGames.Add(new EducationalDosBoxGameInfo(null, installDir, launchName.Name)
            {
                Name = installDir.Name
            });
        }

        /// <summary>
        /// Verifies if the game can launch
        /// </summary>
        /// <param name="game">The game</param>
        /// <returns>True if the game can launch, otherwise false</returns>
        public async Task<bool> VerifyCanLaunchAsync(EducationalDosBoxGameInfo game)
        {
            // Make sure the DosBox executable exists
            if (!File.Exists(RCFRCP.Data.DosBoxPath))
            {
                await RCFUI.MessageUI.DisplayMessageAsync(Resources.LaunchGame_DosBoxNotFound, MessageType.Error);
                return false;
            }

            // Make sure the mount path exists, unless the game is Rayman 1 and TPLS is enabled
            if (!game.MountPath.Exists)
            {
                await RCFUI.MessageUI.DisplayMessageAsync(Resources.LaunchGame_MountPathNotFound, MessageType.Error);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Gets the launch info for the game
        /// </summary>
        /// <param name="game">The game</param>
        /// <returns>The launch info</returns>
        public GameLaunchInfo GetLaunchInfo(EducationalDosBoxGameInfo game)
        {
            return new GameLaunchInfo(RCFRCP.Data.DosBoxPath, GetDosBoxArguments(game.MountPath, $"{game.LaunchName} ver={game.LaunchMode}"));
        }

        #endregion
    }
}