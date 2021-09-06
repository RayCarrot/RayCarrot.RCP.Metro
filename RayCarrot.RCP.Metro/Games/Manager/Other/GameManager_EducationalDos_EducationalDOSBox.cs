using System.Linq;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// The Educational Dos (EducationalDOSBox) game manager
    /// </summary>
    public sealed class GameManager_EducationalDos_EducationalDOSBox : GameManager_EducationalDOSBox
    {
        #region Public Overrides

        /// <summary>
        /// The game
        /// </summary>
        public override Games Game => Games.EducationalDos;

        /// <summary>
        /// The executable name for the game. This is independent of the <see cref="GameInfo.DefaultFileName"/> which is used to launch the game.
        /// </summary>
        public override string ExecutableName => Services.Data.EducationalDosBoxGames.First().LaunchName;

        #endregion
    }
}