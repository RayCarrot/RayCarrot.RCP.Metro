using System.Diagnostics;
using System.Threading.Tasks;
using RayCarrot.RCP.Core;
using RayCarrot.UI;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// The Rayman Raving Rabbids Activity Center (Win32) game manager
    /// </summary>
    public sealed class RaymanRavingRabbidsActivityCenter_Win32 : RCPWin32Game
    {
        #region Public Override Properties

        /// <summary>
        /// The game
        /// </summary>
        public override Games Game => Games.RaymanRavingRabbidsActivityCenter;

        #endregion

        #region Public Override Methods

        /// <summary>
        /// Post launch operations for the game which launched
        /// </summary>
        /// <param name="process">The game process</param>
        /// <returns>The task</returns>
        public override async Task PostLaunchAsync(Process process)
        {
            // Check if the launch message should show
            if (!RCFRCP.Data.ShownRabbidsActivityCenterLaunchMessage)
            {
                await RCFUI.MessageUI.DisplayMessageAsync(Resources.RabbidsActivityCenter_LaunchMessage, MessageType.Information);

                // Flag that the message should not be shown again
                RCFRCP.Data.ShownRabbidsActivityCenterLaunchMessage = true;
            }

            // Run the base code
            await base.PostLaunchAsync(process);
        }

        #endregion
    }
}