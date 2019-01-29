using System.Threading.Tasks;
using RayCarrot.CarrotFramework;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// Extension methods for <see cref="IMessageUIManager"/>
    /// </summary>
    public static class MessageUIManagerExtensions
    {
        /// <summary>
        /// Displays a successful message if set to do so
        /// </summary>
        /// <param name="messageUIManager">The UI manager</param>
        /// <param name="message">The message</param>
        /// <param name="header">The message header</param>
        public static async Task DisplaySuccessfulActionMessageAsync(this IMessageUIManager messageUIManager, string message, string header = "Action succeeded")
        {
            // Make sure the setting to show success messages is on
            if (!RCFRCP.Data.ShowActionComplete)
                return;

            // Show the message
            await messageUIManager.DisplayMessageAsync(message, header, MessageType.Success);
        }
    }
}