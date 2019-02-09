using System;
using System.Threading.Tasks;
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

                // TODO: Create UI manager
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
                        break;

                    default:
                        throw new ArgumentOutOfRangeException(nameof(typeResult.SelectedType), typeResult.SelectedType, null);
                }
            }
            catch (Exception ex)
            {
                ex.HandleError("Locating game");
                // TODO: Error message
            }
        }
    }
}