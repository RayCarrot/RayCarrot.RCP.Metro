using RayCarrot.RCP.Core;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// The base for Rayman Control Panel game
    /// </summary>
    public abstract class RCPGame
    {
        #region Public Abstract Properties

        /// <summary>
        /// The game
        /// </summary>
        public abstract Games Game { get; }

        #endregion

        #region Public Properties

        /// <summary>
        /// Indicates if the game has been added
        /// </summary>
        public bool IsAdded => Game.IsAdded();

        #endregion
    }
}