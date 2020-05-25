using System.Threading.Tasks;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// The Rayman 1 Demo 1 (DOSBox) game manager
    /// </summary>
    public sealed class Demo_Rayman1_1_DOSBox : RCPDOSBoxGame
    {
        #region Public Override Properties

        /// <summary>
        /// The game
        /// </summary>
        public override Games Game => Games.Demo_Rayman1_1;

        /// <summary>
        /// The executable name for the game. This is independent of the <see cref="RCPGameInfo.DefaultFileName"/> which is used to launch the game.
        /// </summary>
        public override string ExecutableName => "RAYMAN.EXE";

        /// <summary>
        /// Indicates if the game requires a disc to be mounted in order to play
        /// </summary>
        public override bool RequiresMounting => false;

        #endregion
    }

    /// <summary>
    /// The Rayman 1 Demo 2 (DOSBox) game manager
    /// </summary>
    public sealed class Demo_Rayman1_2_DOSBox : RCPDOSBoxGame
    {
        #region Public Override Properties

        /// <summary>
        /// The game
        /// </summary>
        public override Games Game => Games.Demo_Rayman1_2;

        /// <summary>
        /// The executable name for the game. This is independent of the <see cref="RCPGameInfo.DefaultFileName"/> which is used to launch the game.
        /// </summary>
        public override string ExecutableName => "RAYMAN.EXE";

        /// <summary>
        /// Gets called as soon as the game is added for the first time
        /// </summary>
        /// <returns>The task</returns>
        public override async Task PostGameAddAsync()
        {
            // Run base
            await base.PostGameAddAsync();

            // Set the default mount path if available
            var mountPath = Game.GetInstallDir() + "Disc" + "RAY1DEMO.cue";

            if (mountPath.FileExists)
                RCPServices.Data.DosBoxGames[Game].MountPath = mountPath;
        }

        #endregion
    }
}