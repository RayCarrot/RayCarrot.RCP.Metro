using System;
using RayCarrot.WPF;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows;
using RayCarrot.Common;
using RayCarrot.Logging;

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
        public async Task<bool> DisplayMessageAsync(string message, string header, MessageType messageType, bool allowCancel, string origin = "", string filePath = "", int lineNumber = 0)
        {
            // Make sure the application has been set up
            if (Application.Current.Dispatcher == null)
                throw new Exception("A message box can not be shown when the application dispatcher is null");

            RL.Logger?.LogTraceSource($"A message of type {messageType} was displayed with the content of: '{message}'", origin: origin, filePath: filePath, lineNumber: lineNumber);

            // Get the header message to use
            var headerMessage = !header.IsNullOrWhiteSpace()
                ? header
                // If we don't have a message, use the default one for the message type
                : messageType switch
                {
                    MessageType.Generic => Resources.MessageHeader_Generic,
                    MessageType.Information => Resources.MessageHeader_Information,
                    MessageType.Error => Resources.MessageHeader_Error,
                    MessageType.Warning => Resources.MessageHeader_Warning,
                    MessageType.Success => Resources.MessageHeader_Success,
                    MessageType.Question => Resources.MessageHeader_Question,
                    _ => Resources.MessageHeader_Generic
                };

            // Helper method for getting the image source for the message type
            static Bitmap GetImgSource(MessageType mt)
            {
                return mt switch
                {
                    MessageType.Generic => Images.Generic,
                    MessageType.Information => Images.Generic,
                    MessageType.Error => Images.Error,
                    MessageType.Warning => Images.Info,
                    MessageType.Question => Images.Question,
                    MessageType.Success => Images.Happy,
                    _ => Images.Generic
                };
            }

            // Create the message actions
            var actions = new List<DialogMessageActionViewModel>();

            // Create a cancel action if available
            if (allowCancel)
                actions.Add(new DialogMessageActionViewModel()
                {
                    DisplayText = Resources.Cancel,
                    DisplayDescription = Resources.Cancel,
                    IsCancel = true,
                    ActionResult = false
                });

            // Create the default action
            actions.Add(new DialogMessageActionViewModel()
            {
                DisplayText = Resources.Ok,
                DisplayDescription = Resources.Ok,
                IsDefault = true,
                ActionResult = true
            });

            // Run on the UI thread
            return await Application.Current.Dispatcher.Invoke(async () =>
            {
                // Create the view model
                var vm = new DialogMessageViewModel()
                {
                    MessageText = message,
                    Title = headerMessage,
                    MessageType = messageType,
                    DialogImageSource = GetImgSource(messageType).ToImageSource(),
                    DialogActions = actions,
                    DefaultActionResult = false
                };

                // Create the message box
                var d = new DialogMessageBox(vm);

                // Show the dialog and return the result
                return await d.ShowDialogAsync(Application.Current.MainWindow) as bool? ?? false;
            });
        }
    }
}