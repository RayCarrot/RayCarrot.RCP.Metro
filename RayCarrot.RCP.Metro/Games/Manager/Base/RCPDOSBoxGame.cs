using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using RayCarrot.CarrotFramework.Abstractions;
using RayCarrot.IO;
using RayCarrot.UI;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// Base for a DOSBox Rayman Control Panel game
    /// </summary>
    public abstract class RCPDOSBoxGame : RCPWin32Game
    {
        #region Protected Override Properties

        /// <summary>
        /// Gets the icon resource path for the game based on its launch information
        /// </summary>
        /// <returns>The icon resource path</returns>
        protected override FileSystemPath IconResourcePath => RCFRCP.Data.DosBoxPath;

        #endregion

        #region Public Override Properties

        /// <summary>
        /// The game type
        /// </summary>
        public override GameType Type => GameType.DosBox;

        /// <summary>
        /// The display name for the game type
        /// </summary>
        public override string GameTypeDisplayName => Resources.GameType_DosBox;

        /// <summary>
        /// Indicates if using <see cref="GameLaunchMode"/> is supported
        /// </summary>
        public override bool SupportsGameLaunchMode => true;

        #endregion

        #region Public Abstract Properties

        /// <summary>
        /// The executable name for the game. This is independent of the <see cref="RCPGameInfo.DefaultFileName"/> which is used to launch the game.
        /// </summary>
        public abstract string ExecutableName { get; }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the DosBox configuration file path for the auto config file
        /// </summary>
        /// <returns>The file path</returns>
        public FileSystemPath DosBoxConfigFile => CommonPaths.UserDataBaseDir + "DosBox" + (Game + ".ini");

        #endregion

        #region Public Override Methods

        /// <summary>
        /// Gets the launch info for the game
        /// </summary>
        /// <returns>The launch info</returns>
        public override GameLaunchInfo GetLaunchInfo()
        {
            var options = RCFRCP.Data.DosBoxGames[Game];
            return new GameLaunchInfo(RCFRCP.Data.DosBoxPath, GetDosBoxArguments(options.MountPath, Game.GetGameInfo().DefaultFileName));
        }

        /// <summary>
        /// Gets called as soon as the game is added for the first time
        /// </summary>
        /// <returns>The task</returns>
        public override Task PostGameAddAsync()
        {
            // Create config file
            new DosBoxAutoConfigManager(DosBoxConfigFile).Create();

            if (!RCFRCP.Data.DosBoxGames.ContainsKey(Game))
                RCFRCP.Data.DosBoxGames.Add(Game, new DosBoxOptions());

            return Task.CompletedTask;
        }

        /// <summary>
        /// Gets called as soon as the game is removed
        /// </summary>
        /// <returns>The task</returns>
        public override Task PostGameRemovedAsync()
        {
            // If there is DosBox options saved, remove those as well
            if (RCFRCP.Data.DosBoxGames.ContainsKey(Game))
            {
                RCFRCP.Data.DosBoxGames.Remove(Game);

                try
                {
                    // Remove the config file
                    RCFRCP.File.DeleteFile(DosBoxConfigFile);
                }
                catch (Exception ex)
                {
                    ex.HandleError("Removing DosBox auto config file");
                }
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// Post launch operations for the game which launched
        /// </summary>
        /// <param name="process">The game process</param>
        /// <returns>The task</returns>
        public override Task PostLaunchAsync(Process process)
        {
            // TODO: Move into Rayman 1 class
            // Check if TPLS should run
            if (Game == Games.Rayman1 && RCFRCP.Data.TPLSData?.IsEnabled == true)
                // Start TPLS
                new TPLS().Start(process);
            else
                return base.PostLaunchAsync(process);

            return Task.CompletedTask;
        }

        /// <summary>
        /// Locates the game
        /// </summary>
        /// <returns>Null if the game was not found. Otherwise a valid or empty path for the install directory</returns>
        public override async Task<FileSystemPath?> LocateAsync()
        {
            // Have user browse for directory
            var result = await RCFUI.BrowseUI.BrowseDirectoryAsync(new DirectoryBrowserViewModel()
            {
                Title = Resources.LocateGame_BrowserHeader,
                DefaultDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),
                MultiSelection = false
            });

            // Make sure the user did not cancel
            if (result.CanceledByUser)
                return null;

            // Make sure the selected directory exists
            if (!result.SelectedDirectory.DirectoryExists)
                return null;

            // Make sure the directory is valid
            if (await IsValidAsync(result.SelectedDirectory))
                return result.SelectedDirectory;

            // If the executable does not exist the location is not valid
            if (!(result.SelectedDirectory + ExecutableName).FileExists)
            {
                RCFCore.Logger?.LogInformationSource($"The selected install directory for {Game} is not valid");

                await RCFUI.MessageUI.DisplayMessageAsync(Resources.LocateGame_InvalidLocation, Resources.LocateGame_InvalidLocationHeader, MessageType.Error);
                return null;
            }

            // Create the .bat file
            File.WriteAllLines(result.SelectedDirectory + Game.GetGameInfo().DefaultFileName, new string[]
            {
                "@echo off",
                $"{Path.GetFileNameWithoutExtension(ExecutableName)} ver=usa"
            });

            RCFCore.Logger?.LogInformationSource($"A batch file was created for {Game}");

            return result.SelectedDirectory;
        }

        /// <summary>
        /// Verifies if the game can launch
        /// </summary>
        /// <returns>True if the game can launch, otherwise false</returns>
        public override async Task<bool> VerifyCanLaunchAsync()
        {
            // Make sure the DosBox executable exists
            if (!File.Exists(RCFRCP.Data.DosBoxPath))
            {
                await RCFUI.MessageUI.DisplayMessageAsync(Resources.LaunchGame_DosBoxNotFound, MessageType.Error);
                return false;
            }

            // TODO: Move into Rayman 1 class
            // Make sure the mount path exists, unless the game is Rayman 1 and TPLS is enabled
            if (!RCFRCP.Data.DosBoxGames[Game].MountPath.Exists && !(Game == Games.Rayman1 && RCFRCP.Data.TPLSData?.IsEnabled == true))
            {
                await RCFUI.MessageUI.DisplayMessageAsync(Resources.LaunchGame_MountPathNotFound, MessageType.Error);
                return false;
            }

            return true;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Gets the DosBox launch arguments for the specific game
        /// </summary>
        /// <param name="mountPath">The disc/file to mount</param>
        /// <param name="launchName">The launch name</param>
        /// <param name="installDir">The game install directory or null for the default one</param>
        /// <returns>The launch arguments</returns>
        protected string GetDosBoxArguments(FileSystemPath mountPath, string launchName, FileSystemPath? installDir = null)
        {
            return $"{(File.Exists(RCFRCP.Data.DosBoxConfig) ? $"-conf \"{RCFRCP.Data.DosBoxConfig} \"" : String.Empty)} " +
                   $"-conf \"{DosBoxConfigFile.FullPath}\" " +
                   // The mounting differs if it's a physical disc vs. a disc image
                   $"{(mountPath.IsDirectoryRoot ? $"-c \"mount d {mountPath.FullPath} -t cdrom\"" : $"-c \"imgmount d '{mountPath.FullPath}' -t iso -fs iso\"")} " +
                   $"-c \"MOUNT C '{installDir ?? GameData.InstallDirectory.FullPath}'\" " +
                   $"-c C: " +
                   $"-c \"{launchName}\" " +
                   $"-noconsole " +
                   $"-c exit";
        }

        #endregion
    }
}