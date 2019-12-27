using System;
using System.IO;
using System.Threading.Tasks;
using RayCarrot.CarrotFramework.Abstractions;
using RayCarrot.Extensions;
using RayCarrot.IO;
using RayCarrot.RCP.Core;
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

        /// <summary>
        /// Gets the game finder item for this game
        /// </summary>
        public override GameFinderItem GameFinderItem => RaymanForeverFolderName == null ? null : new GameFinderItem(null, "Rayman Forever", new string[]
            {
                "Rayman Forever",
            },
            // Navigate to the sub-directory
            x => x + RaymanForeverFolderName);

        #endregion

        #region Public Virtual Properties

        /// <summary>
        /// The Rayman Forever folder name, if available
        /// </summary>
        public virtual string RaymanForeverFolderName => null;

        #endregion

        #region Public Abstract Properties

        /// <summary>
        /// The executable name for the game. This is independent of the <see cref="RCPGameInfo.DefaultFileName"/> which is used to launch the game.
        /// </summary>
        public abstract string ExecutableName { get; }

        #endregion

        #region Public Properties

        // TODO: Add to app paths an make method for getting path
        /// <summary>
        /// Gets the DosBox configuration file path for the auto config file
        /// </summary>
        /// <returns>The file path</returns>
        public FileSystemPath DosBoxConfigFile => RCFRCP.Path.AppUserDataBaseDir + "DosBox" + (Game + ".ini");

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

            // Create the options
            var options = new DosBoxOptions();

            if (!RCFRCP.Data.DosBoxGames.ContainsKey(Game))
                RCFRCP.Data.DosBoxGames.Add(Game, options);

            // If the game was included in Rayman Forever...
            if (RaymanForeverFolderName != null)
            {
                // Get the parent directory to the install directory
                var foreverInstallDir = Game.GetInstallDir().Parent;

                // Attempt to automatically locate the mount file (based on the Rayman Forever location)
                FileSystemPath[] mountFiles =
                {
                    foreverInstallDir + "game.inst",
                    foreverInstallDir + "Music\\game.inst",
                    foreverInstallDir + "game.ins",
                    foreverInstallDir + "Music\\game.ins",
                };

                var mountPath = mountFiles.FindItem(x => x.FileExists);

                if (mountPath.FileExists)
                {
                    options.MountPath = mountPath;
                    RCFCore.Logger?.LogInformationSource($"The mount path for {Game} was automatically found");
                }

                // Find DOSBox path if not already added
                if (!File.Exists(RCFRCP.Data.DosBoxPath))
                {
                    var dosBoxPath = foreverInstallDir + "DosBox" + "DOSBox.exe";

                    if (dosBoxPath.FileExists)
                        RCFRCP.Data.DosBoxPath = dosBoxPath;
                }

                // Find DOSBox config path if not already added
                if (!File.Exists(RCFRCP.Data.DosBoxConfig))
                {
                    var dosBoxConfigPath = foreverInstallDir + "dosboxRayman.conf";

                    if (dosBoxConfigPath.FileExists)
                        RCFRCP.Data.DosBoxConfig = dosBoxConfigPath;
                }
            }

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
                    RCFRCPA.File.DeleteFile(DosBoxConfigFile);
                }
                catch (Exception ex)
                {
                    ex.HandleError("Removing DosBox auto config file");
                }
            }

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
                DefaultDirectory = Environment.SpecialFolder.ProgramFilesX86.GetFolderPath(),
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

            // Make sure the mount path exists
            if (!RCFRCP.Data.DosBoxGames[Game].MountPath.Exists)
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
                   $"-c \"MOUNT C '{installDir ?? Game.GetInstallDir().FullPath}'\" " +
                   $"-c C: " +
                   $"-c \"{launchName}\" " +
                   $"-noconsole " +
                   $"-c exit";
        }

        #endregion
    }
}