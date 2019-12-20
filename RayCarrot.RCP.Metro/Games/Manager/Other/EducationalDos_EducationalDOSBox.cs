using System.Linq;
using RayCarrot.RCP.Core;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// The Educational Dos (EducationalDOSBox) game manager
    /// </summary>
    public sealed class EducationalDos_EducationalDOSBox : RCPEducationalDOSBoxGame
    {
        #region Public Overrides

        /// <summary>
        /// The game
        /// </summary>
        public override Games Game => Games.EducationalDos;

        /// <summary>
        /// The executable name for the game. This is independent of the <see cref="RCPGameInfo.DefaultFileName"/> which is used to launch the game.
        /// </summary>
        public override string ExecutableName => RCFRCP.Data.EducationalDosBoxGames.First().LaunchName;

        #endregion
    }
}