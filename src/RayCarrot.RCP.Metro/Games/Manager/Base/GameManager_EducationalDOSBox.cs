#nullable disable
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using NLog;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// Base for a Educational DOSBox Rayman Control Panel game
/// </summary>
public abstract class GameManager_EducationalDOSBox : GameManager_DOSBox
{
    #region Logger

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    #endregion

    #region Public Override Properties

    /// <summary>
    /// Gets the additional overflow button items for the game
    /// </summary>
    public override IList<OverflowButtonItemViewModel> GetAdditionalOverflowButtonItems => Services.Data.Game_EducationalDosBoxGames.
        Select(x => new OverflowButtonItemViewModel(x.Name, new BitmapImage(new Uri(AppViewModel.WPFApplicationBasePath + @"img\GameIcons\EducationalDos.png")), new AsyncRelayCommand(async () =>
        {
            Logger.Trace("The educational game {0} is being launched...", x.Name);

            // Verify that the game can launch
            if (!await VerifyCanLaunchAsync(x))
            {
                Logger.Info("The educational game {0} could not be launched", x.Name);
                return;
            }

            // Get the launch info
            GameLaunchInfo launchInfo = GetLaunchInfo(x);

            Logger.Trace("The educational game {0} launch info has been retrieved as Path = {1}, Args = {2}", x.Name, launchInfo.Path, launchInfo.Args);

            // Launch the game
            var launchMode = Game.GetInstallation().GetValue<UserData_GameLaunchMode>(GameDataKey.Win32LaunchMode);
            var process = await Services.File.LaunchFileAsync(launchInfo.Path, launchMode == UserData_GameLaunchMode.AsAdmin, launchInfo.Args);

            Logger.Info("The educational game {0} has been launched", x.Name);

            if (process != null)
                // Run any post launch operations on the process
                await PostLaunchAsync(process);

        }))).ToArray();

    #endregion

    #region Protected Override Methods

    /// <summary>
    /// Indicates if the game is valid
    /// </summary>
    /// <param name="installDir">The game install directory, if any</param>
    /// <param name="parameter">Optional game parameter</param>
    /// <returns>True if the game is valid, otherwise false</returns>
    protected override async Task<bool> IsDirectoryValidAsync(FileSystemPath installDir, object parameter = null)
    {
        if (Services.Data.Game_EducationalDosBoxGames == null)
            return false;

        var toRemove = Services.Data.Game_EducationalDosBoxGames.
            Where(game => !IsGameDirValid(game.InstallDir) || game.LaunchName.IsNullOrWhiteSpace()).
            ToArray();

        // Remove invalid games
        foreach (var game in toRemove)
            Services.Data.Game_EducationalDosBoxGames.Remove(game);

        // Notify user
        foreach (var game in toRemove)
        {
            Services.Data.App_JumpListItemIDCollection.RemoveWhere(x => x == game.ID);

            await Services.MessageUI.DisplayMessageAsync(String.Format(Resources.GameNotFound, game.Name), Resources.GameNotFoundHeader, MessageType.Error);
        }

        // Make sure there is at least one game
        if (Services.Data.Game_EducationalDosBoxGames?.Any() != true)
            return false;

        // If any games were removed, refresh the default game and jump list
        if (toRemove.Any())
        {
            // Reset the game data with new install directory
            RefreshDefault();
            await Services.App.OnRefreshRequiredAsync(new RefreshRequiredEventArgs(Games.EducationalDos.GetInstallation(), RefreshFlags.JumpList));
        }

        return true;
    }

    #endregion

    #region Public Override Methods

    public override IEnumerable<JumpListItemViewModel> GetJumpListItems(GameInstallation gameInstallation)
    {
        if (Services.Data.Game_EducationalDosBoxGames == null)
            return Enumerable.Empty<JumpListItemViewModel>();

        return Services.Data.Game_EducationalDosBoxGames.Select(x =>
        {
            GameLaunchInfo launchInfo = gameInstallation.GameDescriptor.GetLegacyManager<GameManager_EducationalDOSBox>().GetLaunchInfo(x);

            return new JumpListItemViewModel(x.Name, launchInfo.Path, launchInfo.Path, launchInfo.Path.Parent, launchInfo.Args, x.ID);
        }).ToArray();
    }

    public override GameLaunchInfo GetLaunchInfo(GameInstallation gameInstallation)
    {
        // Get the default game
        UserData_EducationalDosBoxGameData defaultGame = Services.Data.Game_EducationalDosBoxGames.First();

        return GetLaunchInfo(defaultGame);
    }

    public override Task PostGameAddAsync(GameInstallation gameInstallation)
    {
        // Get the info
        var info = GetNewEducationalDosBoxGameInfo(gameInstallation.InstallLocation);

        // Add the game to the list of educational games
        Services.Data.Game_EducationalDosBoxGames.Add(info);

        // Create config file
        return base.PostGameAddAsync(gameInstallation);
    }

    /// <summary>
    /// Gets called as soon as the game is removed
    /// </summary>
    /// <returns>The task</returns>
    public override Task PostGameRemovedAsync()
    {
        // Remove game specific data
        Services.Data.Game_EducationalDosBoxGames = null;
            
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
            Logger.Info("The selected install directory for {0} is not valid", Game);

            await Services.MessageUI.DisplayMessageAsync(Resources.LocateGame_InvalidLocation, Resources.LocateGame_InvalidLocationHeader, MessageType.Error);
            return null;
        }

        // Return the valid directory
        return result.SelectedDirectory;
    }

    /// <summary>
    /// Verifies if the game can launch
    /// </summary>
    /// <returns>True if the game can launch, otherwise false</returns>
    public override async Task<bool> VerifyCanLaunchAsync()
    {
        return await VerifyCanLaunchAsync(Services.Data.Game_EducationalDosBoxGames.First());
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
        if (Services.Data.Game_EducationalDosBoxGames == null)
            Services.Data.Game_EducationalDosBoxGames = new List<UserData_EducationalDosBoxGameData>();

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
        return new GameLaunchInfo(Services.Data.Emu_DOSBox_Path, GetDosBoxArguments(game.MountPath, $"{game.LaunchName} ver={game.LaunchMode}", game.InstallDir));
    }

    /// <summary>
    /// Verifies if the game can launch
    /// </summary>
    /// <param name="game">The game</param>
    /// <returns>True if the game can launch, otherwise false</returns>
    public async Task<bool> VerifyCanLaunchAsync(UserData_EducationalDosBoxGameData game)
    {
        // Make sure the DosBox executable exists
        if (!File.Exists(Services.Data.Emu_DOSBox_Path))
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
            Logger.Error(ex, "Checking if educational game directory is valid");

            return false;
        }
    }

    /// <summary>
    /// Refreshes the default game
    /// </summary>
    public void RefreshDefault()
    {
        // TODO-14: Fix this
        // Reset the game data with new install directory
        var gameInstallation = Games.EducationalDos.GetInstallation();
        Services.Data.Game_GameInstallations.Remove(gameInstallation);
        // TODO-14: Copy over additional data as well for things like launch mode
        Services.Data.Game_GameInstallations.Add(new GameInstallation(gameInstallation.GameDescriptor, Services.Data.Game_EducationalDosBoxGames.First().InstallDir, gameInstallation.IsRCPInstalled));

        Logger.Info("The default educational game has been refreshed");
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