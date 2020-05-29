using RayCarrot.WPF;
using System.Threading.Tasks;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// The message UI manager for the Rayman Control Panel
    /// </summary>
    public class RCPMessageUIManager : IMessageUIManager
    {
        /// <summary>Displays a message to the user</summary>
        /// <param name="message">The message to display</param>
        /// <param name="header">The header for the message</param>
        /// <param name="messageType">The type of message, determining its visual appearance</param>
        /// <param name="allowCancel">True if the option to cancel is present</param>
        /// <param name="origin">The caller member name (leave at default for compiler-time value)</param>
        /// <param name="filePath">The caller file path (leave at default for compiler-time value)</param>
        /// <param name="lineNumber">The caller line number (leave at default for compiler-time value)</param>
        /// <returns>True if the user accepted the message, otherwise false</returns>
        public Task<bool> DisplayMessageAsync(string message, string header, MessageType messageType, bool allowCancel, string origin = "", string filePath = "", int lineNumber = 0)
        {
            return RCPServices.UI.DisplayMessageAsync(message, header, messageType, allowCancel, new DialogMessageActionViewModel[0], origin, filePath, lineNumber);
        }
    }
}