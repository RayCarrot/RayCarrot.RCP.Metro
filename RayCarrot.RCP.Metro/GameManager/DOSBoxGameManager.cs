using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using RayCarrot.CarrotFramework.Abstractions;
using RayCarrot.IO;
using RayCarrot.UI;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// The game manager for a <see cref="GameType.DosBox"/> game
    /// </summary>
    public class DOSBoxGameManager : Win32GameManager
    {
        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="game">The game to manage</param>
        /// <param name="type">The game type</param>
        public DOSBoxGameManager(Games game, GameType type = GameType.DosBox) : base(game, type)
        {

        }

        #endregion

        #region Protected Overrides Properties

        /// <summary>
        /// The display name for the game type
        /// </summary>
        public override string GameTypeDisplayName => Resources.GameType_DosBox;

        #endregion

        #region Protected Methods

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
                   $"-conf \"{Game.GetDosBoxConfigFile()}\" " +
                   // The mounting differs if it's a physical disc vs. a disc image
                   $"{(mountPath.IsDirectoryRoot() ? "-c \"mount d " + mountPath.FullPath + " -t cdrom\"" : "-c \"imgmount d '" + mountPath.FullPath + "' -t iso -fs iso\"")} " +
                   $"-c \"MOUNT C '{installDir ?? Info.InstallDirectory.FullPath}'\" " +
                   $"-c C: " +
                   $"-c \"{launchName}\" " +
                   $"-noconsole " +
                   $"-c exit";
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Gets the game executable name for the specified DOSBox game
        /// </summary>
        /// <returns>The executable file name</returns>
        public string GetGameExectuable()
        {
            switch (Game)
            {
                case Games.Rayman1:
                    return "RAYMAN.EXE";

                case Games.RaymanDesigner:
                    return "RAYKIT.EXE";

                case Games.RaymanByHisFans:
                    return "RAYFAN.EXE";

                case Games.Rayman60Levels:
                    return "RAYPLUS.EXE";

                default:
                    throw new ArgumentOutOfRangeException(nameof(Game));
            }
        }

        #endregion

        #region Protected Overrides

        /// <summary>
        /// Post launch operations for the game which launched
        /// </summary>
        /// <param name="process">The game process</param>
        /// <returns>The task</returns>
        protected override Task PostLaunchAsync(Process process)
        {
            // Check if TPLS should run
            if (Game == Games.Rayman1 && RCFRCP.Data.TPLSData?.IsEnabled == true)
                // Start TPLS
                new TPLS().Start(process);
            else
                return base.PostLaunchAsync(process);

            return Task.CompletedTask;
        }

        /// <summary>
        /// The implementation for launching the game
        /// </summary>
        /// <param name="forceRunAsAdmin">Indicated if the game should be forced to run as admin</param>
        /// <returns>The launch result</returns>
        protected override async Task<GameLaunchResult> LaunchAsync(bool forceRunAsAdmin)
        {
            if (Game != Games.Rayman1 || RCFRCP.Data.TPLSData?.IsEnabled != true)
                return await base.LaunchAsync(forceRunAsAdmin);

            // Handle Rayman 1 differently if TPLS is enabled
            // TODO: This should be moved into a utility class which then injects code here

            var launchInfo = new GameLaunchInfo(RCFRCP.Data.DosBoxPath, GetDosBoxArguments(RCFRCP.Data.TPLSData.InstallDir + "RayCD.cue", Game.GetLaunchName()));

            RCFCore.Logger?.LogTraceSource($"The game {Game} launch info has been retrieved as Path = {launchInfo.Path}, Args = {launchInfo.Args}");

            // Launch the game
            var process = await RCFRCP.File.LaunchFileAsync(launchInfo.Path, forceRunAsAdmin || Info.LaunchMode == GameLaunchMode.AsAdmin, launchInfo.Args);

            RCFCore.Logger?.LogInformationSource($"The game {Game} has been launched in TPLS mode");

            return new GameLaunchResult(process, process != null);
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
            if (!RCFRCP.Data.DosBoxGames[Game].MountPath.Exists && !(Game == Games.Rayman1 && RCFRCP.Data.TPLSData?.IsEnabled == true))
            {
                await RCFUI.MessageUI.DisplayMessageAsync(Resources.LaunchGame_MountPathNotFound, MessageType.Error);
                return false;
            }

            return true;
        }

        #endregion

        #region Public Overrides

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
            if (await IsValidAsync(result.SelectedDirectory))
                return result.SelectedDirectory;

            // Check if the location contains the executable file
            var exe = GetGameExectuable();

            // If the executable does not exist the location is not valid
            if (!(result.SelectedDirectory + exe).FileExists)
            {
                RCFCore.Logger?.LogInformationSource($"The selected install directory for {Game} is not valid");

                await RCFUI.MessageUI.DisplayMessageAsync(Resources.LocateGame_InvalidLocation, Resources.LocateGame_InvalidLocationHeader, MessageType.Error);
                return null;
            }

            // Create the .bat file
            File.WriteAllLines(result.SelectedDirectory + Game.GetLaunchName(), new string[]
            {
                "@echo off",
                $"{Path.GetFileNameWithoutExtension(exe)} ver=usa"
            });

            RCFCore.Logger?.LogInformationSource($"A batch file was created for {Game}");

            return result.SelectedDirectory;
        }

        /// <summary>
        /// Gets the launch info for the game
        /// </summary>
        /// <returns>The launch info</returns>
        public override GameLaunchInfo GetLaunchInfo()
        {
            var options = RCFRCP.Data.DosBoxGames[Game];
            return new GameLaunchInfo(RCFRCP.Data.DosBoxPath, GetDosBoxArguments(options.MountPath, Game.GetLaunchName()));
        }

        /// <summary>
        /// Gets called as soon as the game is added for the first time
        /// </summary>
        /// <returns>The task</returns>
        public override Task PostGameAddAsync()
        {
            // Create config file
            new DosBoxAutoConfigManager(Game.GetDosBoxConfigFile()).Create();

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
                    RCFRCP.File.DeleteFile(Game.GetDosBoxConfigFile());
                }
                catch (Exception ex)
                {
                    ex.HandleError("Removing DosBox auto config file");
                }
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// Gets the icon resource path for the game based on its launch information
        /// </summary>
        /// <returns>The icon resource path</returns>
        public override string GetIconResourcePath() => Info.InstallDirectory + Game.GetLaunchName();

        #endregion
    }
}