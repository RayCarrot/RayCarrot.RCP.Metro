using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Windows.Devices.SmartCards;
using RayCarrot.CarrotFramework;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// The game manager
    /// </summary>
    public class GameManager
    {
        /// <summary>
        /// Allows the user to locate the specified game
        /// </summary>
        /// <param name="game">The game to locate</param>
        /// <returns>The task</returns>
        public async Task LocateGameAsync(Games game)
        {
            try
            {
                RCF.Logger.LogTraceSource($"The game {game} is being located...");

                var typeResult = await game.GetGameTypeAsync();

                if (typeResult.CanceledByUser)
                    return;

                RCF.Logger.LogInformationSource($"The game {game} type has been detected as {typeResult.SelectedType}");

                switch (typeResult.SelectedType)
                {
                    case GameType.Win32:
                    case GameType.DosBox:
                        var result = await RCF.BrowseUI.BrowseDirectoryAsync(new DirectoryBrowserViewModel()
                        {
                            Title = "Select Install Directory",
                            DefaultDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),
                            MultiSelection = false
                        });

                        if (result.CanceledByUser)
                            return;

                        if (!result.SelectedDirectory.DirectoryExists)
                            return;

                        // Make sure the game is valid
                        if (!game.IsValid(typeResult.SelectedType, result.SelectedDirectory))
                        {
                            RCF.Logger.LogInformationSource($"The selected install directory for {game} is not valid");

                            await RCF.MessageUI.DisplayMessageAsync("The selected directory is not valid for this game", "Invalid Location", MessageType.Error);
                            return;
                        }

                        // Add the game
                        await RCFRCP.App.AddNewGameAsync(game, typeResult.SelectedType, result.SelectedDirectory);

                        RCF.Logger.LogInformationSource($"The game {game} has been added");

                        break;

                    case GameType.Steam:

                        // Make sure the game is valid
                        if (!game.IsValid(typeResult.SelectedType, FileSystemPath.EmptyPath))
                        {
                            RCF.Logger.LogInformationSource($"The {game} was not found under Steam Apps");

                            await RCF.MessageUI.DisplayMessageAsync("The game could not be found. Try choosing desktop app as the type instead.", "Game not found", MessageType.Error);
                            return;
                        }

                        // Add the game
                        await RCFRCP.App.AddNewGameAsync(game, typeResult.SelectedType);

                        RCF.Logger.LogInformationSource($"The game {game} has been added");

                        break;

                    case GameType.WinStore:
                        // Helper method for finding and adding a Windows Store app
                        async Task<bool> FindWinStoreAppAsync()
                        {
                            // Check if the game is installed
                            if (!game.IsValid(GameType.WinStore, FileSystemPath.EmptyPath))
                                return false;

                            // Add the game
                            await RCFRCP.App.AddNewGameAsync(game, GameType.WinStore);

                            RCF.Logger.LogInformationSource($"The game {game.GetDisplayName()} has been added");

                            return true;
                        }

                        bool found;

                        if (game == Games.RaymanFiestaRun)
                        {
                            RCFRCP.Data.IsFiestaRunWin10Edition = true;

                            found = await FindWinStoreAppAsync();

                            if (!found)
                            {
                                RCFRCP.Data.IsFiestaRunWin10Edition = false;

                                found = await FindWinStoreAppAsync();
                            }
                        }
                        else
                        {
                            found = await FindWinStoreAppAsync();
                        }

                        if (!found)
                        {
                            RCF.Logger.LogInformationSource($"The {game} was not found under Windows Store packages");

                            await RCF.MessageUI.DisplayMessageAsync("The game could not be found.", "Game not found", MessageType.Error);
                        }

                        break;

                    default:
                        throw new ArgumentOutOfRangeException(nameof(typeResult.SelectedType), typeResult.SelectedType, null);
                }
            }
            catch (Exception ex)
            {
                ex.HandleError("Locating game");
                await RCF.MessageUI.DisplayMessageAsync("An error occurred when locating the game", "Error locating game", MessageType.Error);
            }
        }

        /// <summary>
        /// Launches the specified game
        /// </summary>
        /// <param name="game">The game to launch</param>
        /// <returns>The task</returns>
        public async Task LaunchGameAsync(Games game)
        {
            var type = game.GetInfo().GameType;

            // If it's a Windows Store app, launch the first package app entry instead
            if (type == GameType.WinStore)
            {
                try
                {
                    // Launch the first app entry for the package
                    await (await game.GetGamePackage().GetAppListEntriesAsync()).First().LaunchAsync();

                    RCF.Logger.LogInformationSource($"The game {game} has been launched");

                    if (RCFRCP.Data.CloseAppOnGameLaunch)
                        Application.Current.Shutdown();
                }
                catch (Exception ex)
                {
                    ex.HandleError("Launching Windows Store application");
                    await RCF.MessageUI.DisplayMessageAsync($"An error occurred when attempting to run {game.GetDisplayName()}", "Error", MessageType.Error);
                }
                return;
            }

            // Run extra checks if it's a DosBox game
            if (type == GameType.DosBox)
            {
                // Make sure the DosBox executable exists
                if (!File.Exists(RCFRCP.Data.DosBoxPath))
                {
                    await RCF.MessageUI.DisplayMessageAsync("DosBox could not be found. Specify a valid path under settings to run this game.", MessageType.Error);
                    return;
                }

                // Make sure the mount path exists, unless the game is Rayman 1 and TPLS is enabled
                if (!RCFRCP.Data.DosBoxGames[game].MountPath.Exists && !(game == Games.Rayman1 && RCFRCP.Data.TPLSData?.IsEnabled == true))
                {
                    await RCF.MessageUI.DisplayMessageAsync("The mount path could not be found. Specify a valid path under the game options to run this game.", MessageType.Error);
                    return;
                }
            }

            // Get the launch info
            GameLaunchInfo launchInfo;

            // Handle Rayman 1 differently if TPLS is enabled
            if (game == Games.Rayman1 && RCFRCP.Data.TPLSData?.IsEnabled == true)
            {
                DosBoxOptions dosBoxConfig = RCFRCP.Data.DosBoxGames[game];
                launchInfo = new GameLaunchInfo(RCFRCP.Data.DosBoxPath,
                    DosBoxHelpers.GetDosBoxArgument(RCFRCP.Data.DosBoxConfig, Games.Rayman1.GetInfo().InstallDirectory,
                        RCFRCP.Data.TPLSData.InstallDir + "RayCD.cue", dosBoxConfig.GetCommands(), game.GetLaunchName()));
            }
            else
            {
                launchInfo = game.GetLaunchInfo();
            }

            RCF.Logger.LogTraceSource($"The game {game} launch info has been retrieved as Path = {launchInfo.Path}, Args = {launchInfo.Args}");

            // Launch the game
            var process = await RCFRCP.File.LaunchFileAsync(launchInfo.Path, false, launchInfo.Args);

            RCF.Logger.LogInformationSource($"The game {game} has been launched");

            // Check if TPLS should run
            if (game == Games.Rayman1 && RCFRCP.Data.TPLSData?.IsEnabled == true)
            {
                // Start TPLS
                new TPLS().Start(process);
            }
            else
            {
                process?.Dispose();

                // Check if the application should close
                if (RCFRCP.Data.CloseAppOnGameLaunch)
                    Application.Current.Shutdown();
            }
        }
    }
}