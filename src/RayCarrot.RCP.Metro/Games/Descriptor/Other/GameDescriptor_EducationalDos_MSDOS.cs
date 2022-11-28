using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using BinarySerializer.Ray1;
using NLog;
using RayCarrot.RCP.Metro.Archive;
using RayCarrot.RCP.Metro.Archive.Ray1;

namespace RayCarrot.RCP.Metro;

// TODO-14: Change this to not allow multiple games in one

/// <summary>
/// The Educational Dos (MS-DOS) game descriptor
/// </summary>
public sealed class GameDescriptor_EducationalDos_MSDOS : MSDOSGameDescriptor
{
    #region Logger

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    #endregion

    #region Public Properties

    public override string Id => "EducationalDos_MSDOS";
    public override Game Game => Game.EducationalDos;
    public override GameCategory Category => GameCategory.Other;
    public override Games? LegacyGame => Games.EducationalDos;

    public override string DisplayName => "Educational Games";
    public override string BackupName => throw new Exception("A generic backup name can not be obtained for an educational DOS game due to it being a collection of multiple games");
    public override string DefaultFileName => Services.Data.Game_EducationalDosBoxGames?.FirstOrDefault()?.LaunchName ?? String.Empty;

    public override GameIconAsset Icon => GameIconAsset.EducationalDos;

    public override bool AllowPatching => false;
    public override bool HasArchives => true;

    public override string ExecutableName => Services.Data.Game_EducationalDosBoxGames.First().LaunchName;

    #endregion

    #region Private Methods

    /// <summary>
    /// Gets the launch args for the game
    /// </summary>
    /// <param name="game">The game</param>
    /// <returns>The launch info</returns>
    private string GetLaunchArgs(UserData_EducationalDosBoxGameData game) =>
        GetDosBoxArguments(game.MountPath, $"{game.LaunchName} ver={game.LaunchMode}", game.InstallDir);

    /// <summary>
    /// Verifies if the game can launch
    /// </summary>
    /// <param name="game">The game</param>
    /// <returns>True if the game can launch, otherwise false</returns>
    private async Task<bool> VerifyCanLaunchAsync(UserData_EducationalDosBoxGameData game)
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

    #endregion

    #region Protected Methods

    protected override string GetLaunchArgs(GameInstallation gameInstallation) => 
        GetLaunchArgs(Services.Data.Game_EducationalDosBoxGames.First());

    protected override Task<bool> VerifyCanLaunchAsync(GameInstallation gameInstallation)
    {
        return VerifyCanLaunchAsync(Services.Data.Game_EducationalDosBoxGames.First());
    }

    protected override bool IsGameLocationValid(FileSystemPath installLocation)
    {
        if (Services.Data.Game_EducationalDosBoxGames == null)
            return false;

        UserData_EducationalDosBoxGameData[] toRemove = Services.Data.Game_EducationalDosBoxGames.
            Where(game => !IsGameDirValid(game.InstallDir) || game.LaunchName.IsNullOrWhiteSpace()).
            ToArray();

        // Remove invalid games
        foreach (var game in toRemove)
            Services.Data.Game_EducationalDosBoxGames.Remove(game);

        // Notify user
        foreach (var game in toRemove)
        {
            Services.Data.App_JumpListItemIDCollection.RemoveWhere(x => x == game.ID);

            // TODO-14: Fix
            //await Services.MessageUI.DisplayMessageAsync(String.Format(Resources.GameNotFound, game.Name), Resources.GameNotFoundHeader, MessageType.Error);
        }

        // Make sure there is at least one game
        if (Services.Data.Game_EducationalDosBoxGames?.Any() != true)
            return false;

        // If any games were removed, refresh the default game and jump list
        if (toRemove.Any())
        {
            // Reset the game data with new install directory
            RefreshDefault();

            // TODO-14: Fix
            //await Services.App.OnRefreshRequiredAsync(new RefreshRequiredEventArgs(Games.EducationalDos.GetInstallation(), 
            //RefreshFlags.JumpList));
        }

        return true;
    }

    #endregion

    #region Public Methods

    public override IEnumerable<GameAddAction> GetAddActions() => new GameAddAction[]
    {
        new LocateRayman1MSDOSGameAddAction(this),
    };

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
        Services.Data.Game_EducationalDosBoxGames ??= new List<UserData_EducationalDosBoxGameData>();

        // Create the game data
        UserData_EducationalDosBoxGameData info = new(installDir, launchName.Name)
        {
            Name = installDir.Name
        };

        return info;
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
            FileSystemPath engineDir = dir + "PCMAP";

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
        GameInstallation gameInstallation = Games.EducationalDos.GetInstallation();
        Services.Data.Game_GameInstallations.Remove(gameInstallation);
        // TODO-14: Copy over additional data as well for things like launch mode
        Services.Data.Game_GameInstallations.Add(new GameInstallation(gameInstallation.GameDescriptor, Services.Data.Game_EducationalDosBoxGames.First().InstallDir));

        Logger.Info("The default educational game has been refreshed");
    }

    public override FrameworkElement GetOptionsUI(GameInstallation gameInstallation) => 
        new GameOptions_EducationalDos_Control();

    public override GameOptionsDialog_ConfigPageViewModel GetConfigPageViewModel(GameInstallation gameInstallation) => 
        new Config_RaymanEduDos_ViewModel(this, gameInstallation);

    // TODO-14: Use current volume?
    public override RayMapInfo GetRayMapInfo() => new(RayMapViewer.Ray1Map, "RaymanEducationalPC", "r1/edu/pc_gb", "GB1");

    // TODO-14: Fix
    //public override GameProgressionManager? GetGameProgressionManager(GameInstallation gameInstallation) =>
    //    Services.Data.Game_EducationalDosBoxGames.
    //        Where(x => !x.LaunchMode.IsNullOrWhiteSpace()).
    //        Select(x => new ProgressionGameViewModel_EducationalDos(gameInstallation, x));

    public override IArchiveDataManager GetArchiveDataManager(GameInstallation? gameInstallation) =>
        new Ray1PCArchiveDataManager(new Ray1Settings(Ray1EngineVersion.PC_Edu));

    // TODO-14: Based on the selected mode also include SNDSMP.DAT, SPECIAL.DAT and VIGNET.DAT
    public override IEnumerable<string> GetArchiveFilePaths(GameInstallation? gameInstallation) => new[]
    {
        @"PCMAP\COMMON.DAT",
        @"PCMAP\SNDD8B.DAT",
        @"PCMAP\SNDH8B.DAT",
    };

    // TODO-14: Fix
    /*
    public override IEnumerable<ActionItemViewModel> GetGameLinks(GameInstallation gameInstallation) =>
        Services.Data.Game_EducationalDosBoxGames.Select(x => new ImageCommandItemViewModel(
            header: x.Name, 
            imageSource: new BitmapImage(new Uri($@"{AppViewModel.WPFApplicationBasePath}img\GameIcons\EducationalDos.png")), 
            command: new AsyncRelayCommand(async () => 
            {
                Logger.Trace("The educational game {0} is being launched...", x.Name);

                // TODO-14: Fix
                // Verify that the game can launch
                //if (!await this.GetLegacyManager<GameManager_EducationalDos_EducationalDOSBox>().VerifyCanLaunchAsync(x))
                //{
                //    Logger.Info("The educational game {0} could not be launched", x.Name);
                //    return;
                //}

                // Get the launch info
                FileSystemPath launchPath = GetLaunchFilePath(gameInstallation);
                string launchArgs = GetLaunchArgs(x);

                Logger.Trace("The educational game {0} launch info has been retrieved as Path = {1}, Args = {2}", x.Name, launchPath, launchArgs);

                // Launch the game
                var launchMode = gameInstallation.GetValue<UserData_GameLaunchMode>(GameDataKey.Win32LaunchMode);
                var process = await Services.File.LaunchFileAsync(launchPath, launchMode == UserData_GameLaunchMode.AsAdmin, launchArgs);

                Logger.Info("The educational game {0} has been launched", x.Name);

                if (process != null)
                    // Run any post launch operations on the process
                    await PostLaunchAsync(process);

            }))).ToArray();*/

    public override IEnumerable<JumpListItemViewModel> GetJumpListItems(GameInstallation gameInstallation)
    {
        if (Services.Data.Game_EducationalDosBoxGames == null)
            return Enumerable.Empty<JumpListItemViewModel>();

        return Services.Data.Game_EducationalDosBoxGames.Select(x =>
        {
            // Get the launch info
            FileSystemPath launchPath = GetLaunchFilePath(gameInstallation);
            string launchArgs = GetLaunchArgs(x);

            return new JumpListItemViewModel(x.Name, launchPath, launchPath, launchPath.Parent, launchArgs, x.ID);
        }).ToArray();
    }

    // TODO-14: Fix
    /*
    public override async Task<FileSystemPath?> LocateAsync()
    {
        // Have user browse for directory
        DirectoryBrowserResult result = await Services.BrowseUI.BrowseDirectoryAsync(new DirectoryBrowserViewModel()
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
            Logger.Info("The selected install directory for {0} is not valid", Id);

            await Services.MessageUI.DisplayMessageAsync(Resources.LocateGame_InvalidLocation, Resources.LocateGame_InvalidLocationHeader, MessageType.Error);
            return null;
        }

        // Return the valid directory
        return result.SelectedDirectory;
    }*/

    public override Task PostGameAddAsync(GameInstallation gameInstallation)
    {
        // Get the info
        UserData_EducationalDosBoxGameData info = GetNewEducationalDosBoxGameInfo(gameInstallation.InstallLocation);

        // Add the game to the list of educational games
        Services.Data.Game_EducationalDosBoxGames.Add(info);

        // Create config file
        return base.PostGameAddAsync(gameInstallation);
    }

    public override Task PostGameRemovedAsync(GameInstallation gameInstallation)
    {
        // Remove game specific data
        Services.Data.Game_EducationalDosBoxGames = null;

        return base.PostGameRemovedAsync(gameInstallation);
    }

    #endregion
}