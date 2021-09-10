using NLog;
using System;
using System.Threading.Tasks;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// Extension methods for <see cref="IMessageUIManager"/>
    /// </summary>
    public static class MessageUIManagerExtensions
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Displays a message to the user
        /// </summary>
        /// <param name="messageUIManager">The message UI Manager to manage the message request</param>
        /// <param name="message">The message to display</param>
        /// <param name="header">The header for the message</param>
        /// <param name="messageType">The type of message, determining its visual appearance</param>
        /// <returns>True if the user accepted the message, otherwise false</returns>
        public static async Task<bool> DisplayMessageAsync(this IMessageUIManager messageUIManager, string message, string header, MessageType messageType) =>
            await messageUIManager.DisplayMessageAsync(message, header, messageType, false);

        /// <summary>
        /// Displays a message to the user
        /// </summary>
        /// <param name="messageUIManager">The message UI Manager to manage the message request</param>
        /// <param name="message">The message to display</param>
        /// <param name="header">The header for the message</param>
        /// <returns>True if the user accepted the message, otherwise false</returns>
        public static async Task<bool> DisplayMessageAsync(this IMessageUIManager messageUIManager, string message, string header) =>
            await messageUIManager.DisplayMessageAsync(message, header, MessageType.Generic, false);

        /// <summary>
        /// Displays a message to the user
        /// </summary>
        /// <param name="messageUIManager">The message UI Manager to manage the message request</param>
        /// <param name="message">The message to display</param>
        /// <param name="messageType">The type of message, determining its visual appearance</param>
        /// <returns>True if the user accepted the message, otherwise false</returns>
        public static async Task<bool> DisplayMessageAsync(this IMessageUIManager messageUIManager, string message, MessageType messageType) =>
            await messageUIManager.DisplayMessageAsync(message, null, messageType, false);

        /// <summary>
        /// Displays a message to the user
        /// </summary>
        /// <param name="messageUIManager">The message UI Manager to manage the message request</param>
        /// <param name="message">The message to display</param>
        /// <param name="messageType">The type of message, determining its visual appearance</param>
        /// <param name="allowCancel">True if the option to cancel is present</param>
        /// <returns>True if the user accepted the message, otherwise false</returns>
        public static async Task<bool> DisplayMessageAsync(this IMessageUIManager messageUIManager, string message, MessageType messageType, bool allowCancel) =>
            await messageUIManager.DisplayMessageAsync(message, null, messageType, allowCancel);

        /// <summary>
        /// Displays a message to the user
        /// </summary>
        /// <param name="messageUIManager">The message UI Manager to manage the message request</param>
        /// <param name="message">The message to display</param>
        /// <returns>True if the user accepted the message, otherwise false</returns>
        public static async Task<bool> DisplayMessageAsync(this IMessageUIManager messageUIManager, string message) =>
            await messageUIManager.DisplayMessageAsync(message, null, MessageType.Generic, false);

        /// <summary>
        /// Displays a successful message if set to do so
        /// </summary>
        /// <param name="messageUIManager">The UI manager</param>
        /// <param name="message">The message</param>
        /// <param name="header">The message header</param>
        public static async Task DisplaySuccessfulActionMessageAsync(this IMessageUIManager messageUIManager, string message, string header = null)
        {
            // Make sure the setting to show success messages is on
            if (!Services.Data.App_ShowActionComplete)
            {
                Logger.Trace("A message of type {0} was not displayed with the content of: '{1}'", MessageType.Success, message);

                return;
            }

            // Show the message
            await messageUIManager.DisplayMessageAsync(message, header ?? Resources.ActionSucceeded, MessageType.Success);
        }

        /// <summary>
        /// Displays a message from an <see cref="Exception"/>
        /// </summary>
        /// <param name="messageUIManager">The UI manager</param>
        /// <param name="exception">The exception</param>
        /// <param name="message">The message</param>
        /// <param name="header">The message header</param>
        /// <param name="allowCancel">True if the option to cancel is present</param>
        public static async Task DisplayExceptionMessageAsync(this IMessageUIManager messageUIManager, Exception exception, string message = null, string header = null, bool allowCancel = false)
        {
            var msg = String.Format(Resources.App_ExceptionMessage, message ?? Resources.App_ExceptionMessageDefaultMessage, exception?.Message);
            header ??= Resources.App_ExceptionMessageDefaultHeader;

            // Show the message
            await messageUIManager.DisplayMessageAsync(msg, header, MessageType.Error, allowCancel);
        }
    }
}