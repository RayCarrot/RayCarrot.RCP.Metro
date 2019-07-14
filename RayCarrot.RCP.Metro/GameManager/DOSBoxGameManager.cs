using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using RayCarrot.CarrotFramework;

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
        public DOSBoxGameManager(Games game) : base(game, GameType.DosBox)
        {

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

        protected override async Task<GameLaunchResult> LaunchAsync(bool forceRunAsAdmin)
        {
            if (Game != Games.Rayman1 || RCFRCP.Data.TPLSData?.IsEnabled != true)
                return await base.LaunchAsync(forceRunAsAdmin);

            // Handle Rayman 1 differently if TPLS is enabled
            // TODO: This should be moved into a utility class which then injects code here

            var launchInfo = new GameLaunchInfo(RCFRCP.Data.DosBoxPath, Games.Rayman1.GetDosBoxArguments(Games.Rayman1.GetInfo().InstallDirectory, RCFRCP.Data.TPLSData.InstallDir + "RayCD.cue", Game.GetLaunchName()));

            RCF.Logger.LogTraceSource($"The game {Game} launch info has been retrieved as Path = {launchInfo.Path}, Args = {launchInfo.Args}");

            // Launch the game
            var process = await RCFRCP.File.LaunchFileAsync(launchInfo.Path, forceRunAsAdmin || Info.LaunchMode == GameLaunchMode.AsAdmin, launchInfo.Args);

            RCF.Logger.LogInformationSource($"The game {Game} has been launched in TPLS mode");

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
                await RCF.MessageUI.DisplayMessageAsync(Resources.LaunchGame_DosBoxNotFound, MessageType.Error);
                return false;
            }

            // Make sure the mount path exists, unless the game is Rayman 1 and TPLS is enabled
            if (!RCFRCP.Data.DosBoxGames[Game].MountPath.Exists && !(Game == Games.Rayman1 && RCFRCP.Data.TPLSData?.IsEnabled == true))
            {
                await RCF.MessageUI.DisplayMessageAsync(Resources.LaunchGame_MountPathNotFound, MessageType.Error);
                return false;
            }

            return true;
        }

        #endregion

        #region Public Overrides

        /// <summary>
        /// Gets the launch info for the game
        /// </summary>
        /// <returns>The launch info</returns>
        public override GameLaunchInfo GetLaunchInfo()
        {
            var options = RCFRCP.Data.DosBoxGames[Game];
            return new GameLaunchInfo(RCFRCP.Data.DosBoxPath, Game.GetDosBoxArguments(Info.InstallDirectory, options.MountPath, Game.GetLaunchName()));
        }

        #endregion
    }
}