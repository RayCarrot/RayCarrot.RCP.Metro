﻿using System.Threading.Tasks;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// The Rayman 1 minigames (Win32) game manager
    /// </summary>
    public sealed class Ray1Minigames_Win32 : RCPWin32Game
    {
        #region Public Overrides

        /// <summary>
        /// The game
        /// </summary>
        public override Games Game => Games.Ray1Minigames;

        /// <summary>
        /// Gets called as soon as the game is added for the first time
        /// </summary>
        /// <returns>The task</returns>
        public override Task PostGameAddAsync()
        {
            // Default to run as admin
            RCPServices.Data.Games[Game].LaunchMode = GameLaunchMode.AsAdmin;

            // Call base and return
            return base.PostGameAddAsync();
        }

        #endregion
    }
}