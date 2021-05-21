using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using RayCarrot.Logging;
using RayCarrot.WPF;

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
        /// <param name="origin">The caller member name (leave at default for compiler-time value)</param>
        /// <param name="filePath">The caller file path (leave at default for compiler-time value)</param>
        /// <param name="lineNumber">The caller line number (leave at default for compiler-time value)</param>
        public static async Task DisplaySuccessfulActionMessageAsync(this IMessageUIManager messageUIManager, string message, string header = null, [CallerMemberName] string origin = "", [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0)
        {
            // Make sure the setting to show success messages is on
            if (!RCPServices.Data.ShowActionComplete)
            {
                RL.Logger?.LogTraceSource($"A message of type {MessageType.Success} was not displayed with the content of: '{message}'", origin: origin, filePath: filePath, lineNumber: lineNumber);

                return;
            }

            // Show the message
            await messageUIManager.DisplayMessageAsync(message, header ?? Resources.ActionSucceeded, MessageType.Success, origin, filePath, lineNumber);
        }

        /// <summary>
        /// Displays a message from an <see cref="Exception"/>
        /// </summary>
        /// <param name="messageUIManager">The UI manager</param>
        /// <param name="exception">The exception</param>
        /// <param name="message">The message</param>
        /// <param name="header">The message header</param>
        /// <param name="allowCancel">True if the option to cancel is present</param>
        /// <param name="origin">The caller member name (leave at default for compiler-time value)</param>
        /// <param name="filePath">The caller file path (leave at default for compiler-time value)</param>
        /// <param name="lineNumber">The caller line number (leave at default for compiler-time value)</param>
        public static async Task DisplayExceptionMessageAsync(this IMessageUIManager messageUIManager, Exception exception, string message = null, string header = null, bool allowCancel = false, [CallerMemberName] string origin = "", [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0)
        {
            var msg = String.Format(Resources.App_ExceptionMessage, message ?? Resources.App_ExceptionMessageDefaultMessage, exception?.Message);
            header ??= Resources.App_ExceptionMessageDefaultHeader;

            // Show the message
            await messageUIManager.DisplayMessageAsync(msg, header, MessageType.Error, allowCancel, origin, filePath, lineNumber);
        }
    }
}