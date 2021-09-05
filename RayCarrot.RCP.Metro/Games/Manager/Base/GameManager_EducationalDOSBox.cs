using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using RayCarrot.IO;
using RayCarrot.Logging;
using RayCarrot.UI;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// Base for a Educational DOSBox Rayman Control Panel game
    /// </summary>
    public abstract class GameManager_EducationalDOSBox : GameManager_DOSBox
    {
        #region Public Override Properties

        /// <summary>
        /// The game type
        /// </summary>
        public override GameType Type => GameType.EducationalDosBox;

        /// <summary>
        /// The display name for the game type
        /// </summary>
        public override string GameTypeDisplayName => Resources.GameType_EducationalDosBox;

        /// <summary>
        /// Gets the additional overflow button items for the game
        /// </summary>
        public override IList<OverflowButtonItemViewModel> GetAdditionalOverflowButtonItems => RCPServices.Data.EducationalDosBoxGames.
            Select(x => new OverflowButtonItemViewModel(x.Name, new BitmapImage(new Uri(AppViewModel.WPFApplicationBasePath + @"img\GameIcons\EducationalDos.png")), new AsyncRelayCommand(async () =>
        {
            RL.Logger?.LogTraceSource($"The educational game {x.Name} is being launched...");

            // Verify that the game can launch
            if (!await VerifyCanLaunchAsync(x))
            {
                RL.Logger?.LogInformationSource($"The educational game {x.Name} could not be launched");
                return;
            }

            // Get the launch info
            GameLaunchInfo launchInfo = GetLaunchInfo(x);

            RL.Logger?.LogTraceSource($"The educational game {x.Name} launch info has been retrieved as Path = {launchInfo.Path}, Args = {launchInfo.Args}");

            // Launch the game
            var process = await RCPServices.File.LaunchFileAsync(launchInfo.Path, Game.GetLaunchMode() == UserData_GameLaunchMode.AsAdmin, launchInfo.Args);

            RL.Logger?.LogInformationSource($"The educational game {x.Name} has been launched");

            if (process != null)
                // Run any post launch operations on the process
                await PostLaunchAsync(process);

        }))).ToArray();

        #endregion

        #region Public Override Methods

        /// <summary>
        /// Gets the available jump list items for this game
        /// </summary>
        /// <returns>The items</returns>
        public override IList<JumpListItemViewModel> GetJumpListItems()
        {
            if (RCPServices.Data.EducationalDosBoxGames == null)
                return new JumpListItemViewModel[0];

            return RCPServices.Data.EducationalDosBoxGames.Select(x =>
            {
                var launchInfo = Game.GetManager<GameManager_EducationalDOSBox>().GetLaunchInfo(x);

                return new JumpListItemViewModel(x.Name, launchInfo.Path, launchInfo.Path, launchInfo.Path.Parent, launchInfo.Args, x.ID);
            }).ToArray();
        }

        /// <summary>
        /// Gets the launch info for the game
        /// </summary>
        /// <returns>The launch info</returns>
        public override GameLaunchInfo GetLaunchInfo()
        {
            // Get the default game
            var defaultGame = RCPServices.Data.EducationalDosBoxGames.First();

            return GetLaunchInfo(defaultGame);
        }

        /// <summary>
        /// Gets called as soon as the game is added for the first time
        /// </summary>
        /// <returns>The task</returns>
        public override Task PostGameAddAsync()
        {
            // Get the info
            var info = GetNewEducationalDosBoxGameInfo(Game.GetInstallDir());

            // Add the game to the list of educational games
            RCPServices.Data.EducationalDosBoxGames.Add(info);

            // Create config file
            return base.PostGameAddAsync();
        }

        /// <summary>
        /// Gets called as soon as the game is removed
        /// </summary>
        /// <returns>The task</returns>
        public override Task PostGameRemovedAsync()
        {
            // Remove game specific data
            RCPServices.Data.EducationalDosBoxGames = null;
            
            return base.PostGameRemovedAsync();
        }

        /// <summary>
        /// Locates the game
        /// </summary>
        /// <returns>Null if the game was not found. Otherwise a valid or empty path for the install directory</returns>
        public override async Task<FileSystemPath?> LocateAsync()
        {
            // Have user browse for directory
            var result = await Services.BrowseUI.BrowseDirectoryAsync(new DirectoryBrowserViewModel()
            {
                Title = Resources.LocateGame_BrowserHeader,
                DefaultDirectory = Environment.SpecialFolder.ProgramFilesX86.GetFolderPath(),
                MultiSelection = false
            });

            // Make sure the user did not cancel
            if (result.CanceledByUser)
                return null;

            // Make sure the selected directory exists
            if (!result.SelectedDirectory.DirectoryExists)
                return null;

            // Check if the location if valid
            if (!IsGameDirValid(result.SelectedDirectory))
            {
                RL.Logger?.LogInformationSource($"The selected install directory for {Game} is not valid");

                await Services.MessageUI.DisplayMessageAsync(Resources.LocateGame_InvalidLocation, Resources.LocateGame_InvalidLocationHeader, MessageType.Error);
                return null;
            }

            // Return the valid directory
            return result.SelectedDirectory;
        }

        /// <summary>
        /// Indicates if the game is valid
        /// </summary>
        /// <param name="installDir">The game install directory, if any</param>
        /// <param name="parameter">Optional game parameter</param>
        /// <returns>True if the game is valid, otherwise false</returns>
        public override async Task<bool> IsValidAsync(FileSystemPath installDir, object parameter = null)
        {
            if (RCPServices.Data.EducationalDosBoxGames == null)
                return false;

            var toRemove = RCPServices.Data.EducationalDosBoxGames.
                Where(game => !IsGameDirValid(game.InstallDir) || game.LaunchName.IsNullOrWhiteSpace()).
                ToArray();

            // Remove invalid games
            foreach (var game in toRemove)
                RCPServices.Data.EducationalDosBoxGames.Remove(game);

            // Notify user
            foreach (var game in toRemove)
            {
                RCPServices.Data.JumpListItemIDCollection.RemoveWhere(x => x == game.ID);

                await Services.MessageUI.DisplayMessageAsync(String.Format(Resources.GameNotFound, game.Name), Resources.GameNotFoundHeader, MessageType.Error);
            }

            // Make sure there is at least one game
            if (RCPServices.Data.EducationalDosBoxGames?.Any() != true)
                return false;

            // If any games were removed, refresh the default game and jump list
            if (toRemove.Any())
            {
                // Reset the game data with new install directory
                RefreshDefault();
                await RCPServices.App.OnRefreshRequiredAsync(new RefreshRequiredEventArgs(Games.EducationalDos, false, false, false, false, true));
            }

            return true;
        }

        /// <summary>
        /// Verifies if the game can launch
        /// </summary>
        /// <returns>True if the game can launch, otherwise false</returns>
        public override async Task<bool> VerifyCanLaunchAsync()
        {
            return await VerifyCanLaunchAsync(RCPServices.Data.EducationalDosBoxGames.First());
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Get new info for a new educational DOSBox game
        /// </summary>
        /// <param name="installDir">The install directory</param>
        /// <returns>The info</returns>
        public UserData_EducationalDosBoxGameData GetNewEducationalDosBoxGameInfo(FileSystemPath installDir)
        {
            // Find the launch name
            FileSystemPath launchName = Directory.EnumerateFiles(installDir, "*.exe", SearchOption.TopDirectoryOnly).FirstOrDefault();

            // Create the collection if it doesn't exist
            if (RCPServices.Data.EducationalDosBoxGames == null)
                RCPServices.Data.EducationalDosBoxGames = new List<UserData_EducationalDosBoxGameData>();

            // Create the game data
            var info = new UserData_EducationalDosBoxGameData(installDir, launchName.Name)
            {
                Name = installDir.Name
            };

            return info;
        }

        /// <summary>
        /// Gets the launch info for the game
        /// </summary>
        /// <param name="game">The game</param>
        /// <returns>The launch info</returns>
        public GameLaunchInfo GetLaunchInfo(UserData_EducationalDosBoxGameData game)
        {
            return new GameLaunchInfo(RCPServices.Data.DosBoxPath, GetDosBoxArguments(game.MountPath, $"{game.LaunchName} ver={game.LaunchMode}", game.InstallDir));
        }

        /// <summary>
        /// Verifies if the game can launch
        /// </summary>
        /// <param name="game">The game</param>
        /// <returns>True if the game can launch, otherwise false</returns>
        public async Task<bool> VerifyCanLaunchAsync(UserData_EducationalDosBoxGameData game)
        {
            // Make sure the DosBox executable exists
            if (!File.Exists(RCPServices.Data.DosBoxPath))
            {
                await Services.MessageUI.DisplayMessageAsync(Resources.LaunchGame_DosBoxNotFound, MessageType.Error);
                return false;
            }

            // Make sure the mount path exists, unless the game is Rayman 1 and TPLS is enabled
            if (!game.MountPath.Exists)
            {
                await Services.MessageUI.DisplayMessageAsync(Resources.LaunchGame_MountPathNotFound, MessageType.Error);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Indicates if the specified game directory is valid for an educational game
        /// </summary>
        /// <param name="dir">The directory to check</param>
        /// <returns>True if it's valid, false if not</returns>
        public bool IsGameDirValid(FileSystemPath dir)
        {
            try
            {
                var engineDir = dir + "PCMAP";

                return engineDir.DirectoryExists && Directory.EnumerateDirectories(engineDir).Any();
            }
            catch (Exception ex)
            {
                ex.HandleError("Checking if educational game directory is valid");

                return false;
            }
        }

        /// <summary>
        /// Refreshes the default game
        /// </summary>
        public void RefreshDefault()
        {
            // Get the current launch mode
            var launchMode = Games.EducationalDos.GetLaunchMode();

            // Reset the game data with new install directory
            RCPServices.Data.Games[Games.EducationalDos] = new UserData_GameData(GameType.EducationalDosBox, RCPServices.Data.EducationalDosBoxGames.First().InstallDir)
            {
                LaunchMode = launchMode
            };

            RL.Logger?.LogInformationSource($"The default educational game has been refreshed");
        }

        ///// <summary>
        ///// Gets the game ID for an educational game
        ///// </summary>
        ///// <param name="installDir">The game install directory</param>
        ///// <param name="exeName">The executable file name, relative to the install directory</param>
        ///// <returns>The ID</returns>
        //public string GetGameID(FileSystemPath installDir, string exeName)
        //{
        //    //
        //    // The ID is based on the hash of the executable file as well as the available PCMAP files. This is to make sure that the ID
        //    // is the same if you add the same game again.
        //    //

        //    // Get the paths to use for getting the ID
        //    List<string> inputPaths = Directory.GetFiles(installDir + "PCMAP", "*", SearchOption.AllDirectories).Select(Path.GetFileName).ToList();

        //    // Add the hash from the executable file
        //    inputPaths.Add((installDir + exeName).GetSHA256CheckSum());

        //    // Combine the strings into an array of bytes
        //    var bytes = Encoding.ASCII.GetBytes(inputPaths.SelectMany(x => x).ToArray());

        //    // Get the hash
        //    using (SHA256Managed sha = new SHA256Managed())
        //    {
        //        byte[] checksum = sha.ComputeHash(bytes);       
        //        return BitConverter.ToString(checksum).Replace("-", String.Empty);
        //    }
        //}

        #endregion
    }
}