using System.Threading.Tasks;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// The Rabbids Go Home (Win32) game manager
    /// </summary>
    public sealed class RabbidsGoHome_Win32 : RCPWin32Game
    {
        #region Public Override Properties

        /// <summary>
        /// The game
        /// </summary>
        public override Games Game => Games.RabbidsGoHome;

        /// <summary>
        /// Gets the launch arguments for the game
        /// </summary>
        public override string GetLaunchArgs => RCFRCP.Data.RabbidsGoHomeLaunchData?.ToString();

        /// <summary>
        /// Gets the game finder item for this game
        /// </summary>
        public override GameFinderItem GameFinderItem => new GameFinderItem(null, "Rabbids Go Home", new string[]
        {
            "Rabbids Go Home",
            "Rabbids Go Home - DVD",
            "Rabbids: Go Home",
        });

        #endregion

        #region Public Override Methods

        /// <summary>
        /// Gets called as soon as the game is removed
        /// </summary>
        /// <returns>The task</returns>
        public override Task PostGameRemovedAsync()
        {
            // Remove the game specific data
            RCFRCP.Data.RabbidsGoHomeLaunchData = null;

            return base.PostGameRemovedAsync();
        }

        #endregion
    }
}