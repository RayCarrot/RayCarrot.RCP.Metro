using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using RayCarrot.Common;
using RayCarrot.IO;
using RayCarrot.Logging;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// Base for a DOSBox Rayman Control Panel game
    /// </summary>
    public abstract class GameManager_DOSBox : GameManager_Win32
    {
        #region Protected Override Properties

        /// <summary>
        /// Gets the icon resource path for the game based on its launch information
        /// </summary>
        /// <returns>The icon resource path</returns>
        protected override FileSystemPath IconResourcePath => DOSBoxFilePath;

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
        /// Indicates if using <see cref="UserData_GameLaunchMode"/> is supported
        /// </summary>
        public override bool SupportsGameLaunchMode => true;

        /// <summary>
        /// Gets the game finder item for this game
        /// </summary>
        public override GameFinder_GameItem GameFinderItem => RaymanForeverFolderName == null ? null : new GameFinder_GameItem(null, "Rayman Forever", new string[]
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

        /// <summary>
        /// The DOSBox file path
        /// </summary>
        public virtual FileSystemPath DOSBoxFilePath => RCPServices.Data.DosBoxPath;

        /// <summary>
        /// Optional additional config files
        /// </summary>
        public virtual IEnumerable<FileSystemPath> AdditionalConfigFiles => new FileSystemPath[0];

        /// <summary>
        /// Indicates if the game requires a disc to be mounted in order to play
        /// </summary>
        public virtual bool RequiresMounting => true;

        #endregion

        #region Public Abstract Properties

        /// <summary>
        /// The executable name for the game. This is independent of the <see cref="GameInfo.DefaultFileName"/> which is used to launch the game.
        /// </summary>
        public abstract string ExecutableName { get; }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the DosBox configuration file path for the auto config file
        /// </summary>
        /// <returns>The file path</returns>
        public FileSystemPath DosBoxConfigFile => AppFilePaths.UserDataBaseDir + "DosBox" + (Game + ".ini");

        #endregion

        #region Public Override Methods

        /// <summary>
        /// Gets the launch info for the game
        /// </summary>
        /// <returns>The launch info</returns>
        public override GameLaunchInfo GetLaunchInfo()
        {
            var options = RCPServices.Data.DosBoxGames[Game];
            return new GameLaunchInfo(DOSBoxFilePath, GetDosBoxArguments(options.MountPath, Game.GetGameInfo().DefaultFileName));
        }

        /// <summary>
        /// Gets called as soon as the game is added for the first time
        /// </summary>
        /// <returns>The task</returns>
        public override Task PostGameAddAsync()
        {
            // Create config file
            new Emulator_DOSBox_AutoConfigManager(DosBoxConfigFile).Create();

            // Create the options
            var options = new UserData_DosBoxOptions();

            if (!RCPServices.Data.DosBoxGames.ContainsKey(Game))
                RCPServices.Data.DosBoxGames.Add(Game, options);

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
                    RL.Logger?.LogInformationSource($"The mount path for {Game} was automatically found");
                }

                // Find DOSBox path if not already added
                if (!File.Exists(RCPServices.Data.DosBoxPath))
                {
                    var dosBoxPath = foreverInstallDir + "DosBox" + "DOSBox.exe";

                    if (dosBoxPath.FileExists)
                        RCPServices.Data.DosBoxPath = dosBoxPath;
                }

                // Find DOSBox config path if not already added
                if (!File.Exists(RCPServices.Data.DosBoxConfig))
                {
                    var dosBoxConfigPath = foreverInstallDir + "dosboxRayman.conf";

                    if (dosBoxConfigPath.FileExists)
                        RCPServices.Data.DosBoxConfig = dosBoxConfigPath;
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
            if (RCPServices.Data.DosBoxGames.ContainsKey(Game))
            {
                RCPServices.Data.DosBoxGames.Remove(Game);

                try
                {
                    // Remove the config file
                    RCPServices.File.DeleteFile(DosBoxConfigFile);
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

            // Make sure the directory is valid
            if (await IsValidAsync(result.SelectedDirectory))
                return result.SelectedDirectory;

            // If the executable does not exist the location is not valid
            if (!(result.SelectedDirectory + ExecutableName).FileExists)
            {
                RL.Logger?.LogInformationSource($"The selected install directory for {Game} is not valid");

                await Services.MessageUI.DisplayMessageAsync(Resources.LocateGame_InvalidLocation, Resources.LocateGame_InvalidLocationHeader, MessageType.Error);
                return null;
            }

            // Create the .bat file
            File.WriteAllLines(result.SelectedDirectory + Game.GetGameInfo().DefaultFileName, new string[]
            {
                "@echo off",
                $"{Path.GetFileNameWithoutExtension(ExecutableName)} ver=usa"
            });

            RL.Logger?.LogInformationSource($"A batch file was created for {Game}");

            return result.SelectedDirectory;
        }

        /// <summary>
        /// Verifies if the game can launch
        /// </summary>
        /// <returns>True if the game can launch, otherwise false</returns>
        public override async Task<bool> VerifyCanLaunchAsync()
        {
            // Make sure the DosBox executable exists
            if (!File.Exists(DOSBoxFilePath))
            {
                await Services.MessageUI.DisplayMessageAsync(Resources.LaunchGame_DosBoxNotFound, MessageType.Error);
                return false;
            }

            // Make sure the mount path exists
            if (RequiresMounting && !RCPServices.Data.DosBoxGames[Game].MountPath.Exists)
            {
                await Services.MessageUI.DisplayMessageAsync(Resources.LaunchGame_MountPathNotFound, MessageType.Error);
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
            // Create a string builder for the argument
            var str = new StringBuilder();

            // Helper method for adding an argument
            void AddArg(string arg)
            {
                str.Append($"{arg} ");
            }

            // Helper method for adding a config file to the argument
            void AddConfig(FileSystemPath configFile)
            {
                if (configFile.FileExists)
                    AddArg($"-conf \"{configFile.FullPath}\"");
            }

            // Add the primary config file
            AddConfig(RCPServices.Data.DosBoxConfig);

            // Add the RCP config file
            AddConfig(DosBoxConfigFile.FullPath);

            // Add additional config files
            foreach (var config in AdditionalConfigFiles)
                AddConfig(config);

            // Mount the disc if required
            if (RequiresMounting)
            {
                // The mounting differs if it's a physical disc vs. a disc image
                if (mountPath.IsDirectoryRoot)
                    AddArg($"-c \"mount d {mountPath.FullPath} -t cdrom\"");
                else
                    AddArg($"-c \"imgmount d '{mountPath.FullPath}' -t iso -fs iso\"");
            }

            // Mount the game install directory as the C drive
            AddArg($"-c \"MOUNT C '{installDir ?? Game.GetInstallDir().FullPath}'\"");

            // Add commands to launch the game
            AddArg("-c C:");
            AddArg($"-c \"{launchName}\"");
            AddArg("-noconsole");
            AddArg("-c exit");

            // Return the argument
            return str.ToString().Trim();
        }

        #endregion
    }
}