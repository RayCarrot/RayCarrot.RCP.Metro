using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using RayCarrot.CarrotFramework.Abstractions;
using RayCarrot.RCP.Core;
using RayCarrot.UI;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// The Rayman 1 (DOSBox) game manager
    /// </summary>
    public sealed class Rayman1_DOSBox : RCPDOSBoxGame
    {
        #region Public Override Properties

        /// <summary>
        /// The game
        /// </summary>
        public override Games Game => Games.Rayman1;

        /// <summary>
        /// The executable name for the game. This is independent of the <see cref="RCPGameInfo.DefaultFileName"/> which is used to launch the game.
        /// </summary>
        public override string ExecutableName => "RAYMAN.EXE";

        /// <summary>
        /// The Rayman Forever folder name, if available
        /// </summary>
        public override string RaymanForeverFolderName => "Rayman";

        /// <summary>
        /// Gets the purchase links for the game
        /// </summary>
        public override IList<GamePurchaseLink> GetGamePurchaseLinks => new GamePurchaseLink[]
        {
            new GamePurchaseLink(Resources.GameDisplay_PurchaseGOG, "https://www.gog.com/game/rayman_forever"),
            new GamePurchaseLink(Resources.GameDisplay_PurchaseUplay, "https://store.ubi.com/eu/rayman--forever/5800d3fc4e016524248b4567.html")
        };

        #endregion

        #region Protected Override Methods

        /// <summary>
        /// The implementation for launching the game
        /// </summary>
        /// <param name="forceRunAsAdmin">Indicated if the game should be forced to run as admin</param>
        /// <returns>The launch result</returns>
        protected override async Task<GameLaunchResult> LaunchAsync(bool forceRunAsAdmin)
        {
            // If TPLS is not enabled, fall back to default implementation
            if (RCFRCP.Data.TPLSData?.IsEnabled != true)
                return await base.LaunchAsync(forceRunAsAdmin);

            var launchInfo = new GameLaunchInfo(RCFRCP.Data.DosBoxPath, GetDosBoxArguments(RCFRCP.Data.TPLSData.InstallDir + "RayCD.cue", Game.GetGameInfo().DefaultFileName));

            RCFCore.Logger?.LogTraceSource($"The game {Game} launch info has been retrieved as Path = {launchInfo.Path}, Args = {launchInfo.Args}");

            // Launch the game
            var process = await RCFRCPC.File.LaunchFileAsync(launchInfo.Path, forceRunAsAdmin || Game.GetLaunchMode() == GameLaunchMode.AsAdmin, launchInfo.Args);

            RCFCore.Logger?.LogInformationSource($"The game {Game} has been launched in TPLS mode");

            return new GameLaunchResult(process, process != null);
        }

        /// <summary>
        /// Post launch operations for the game which launched
        /// </summary>
        /// <param name="process">The game process</param>
        /// <returns>The task</returns>
        public override Task PostLaunchAsync(Process process)
        {
            // Check if TPLS should run
            if (RCFRCP.Data.TPLSData?.IsEnabled == true)
                // Start TPLS
                new TPLS().Start(process);

            return Task.CompletedTask;
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

            // Make sure the mount path exists, unless TPLS is enabled
            if (!RCFRCP.Data.DosBoxGames[Game].MountPath.Exists && RCFRCP.Data.TPLSData?.IsEnabled != true)
            {
                await RCFUI.MessageUI.DisplayMessageAsync(Resources.LaunchGame_MountPathNotFound, MessageType.Error);
                return false;
            }

            return true;
        }

        #endregion
    }
}