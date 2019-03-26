using RayCarrot.CarrotFramework;
using RayCarrot.WPF;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows;

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
            RCF.Logger.LogTraceSource($"A message of type {messageType} was displayed with the content of: '{message}'", origin: origin, filePath: filePath, lineNumber: lineNumber);

            string headerMessage;
            if (!header.IsNullOrWhiteSpace())
                headerMessage = header;
            else
            {
                switch (messageType)
                {
                    default:
                    case MessageType.Generic:
                        headerMessage = Resources.MessageHeader_Generic;
                        break;

                    case MessageType.Information:
                        headerMessage = Resources.MessageHeader_Information;
                        break;

                    case MessageType.Error:
                        headerMessage = Resources.MessageHeader_Error;
                        break;

                    case MessageType.Warning:
                        headerMessage = Resources.MessageHeader_Warning;
                        break;

                    case MessageType.Success:
                        headerMessage = Resources.MessageHeader_Success;
                        break;

                    case MessageType.Question:
                        headerMessage = Resources.MessageHeader_Question;
                        break;
                }
            }

            Bitmap GetImgSource(MessageType mt)
            {
                switch (mt)
                {
                    default:
                    case MessageType.Generic:
                        return Images.Generic;

                    case MessageType.Information:
                        return Images.Generic;

                    case MessageType.Error:
                        return Images.Error;

                    case MessageType.Warning:
                        return Images.Info;

                    case MessageType.Question:
                        return Images.Question;

                    case MessageType.Success:
                        return Images.Happy;
                }
            }

            var actions = new List<DialogMessageActionViewModel>();

            if (allowCancel)
                actions.Add(new DialogMessageActionViewModel()
                {
                    DisplayText = Resources.Cancel,
                    DisplayDescription = Resources.Cancel,
                    IsCancel = true,
                    ActionResult = false
                });
            actions.Add(new DialogMessageActionViewModel()
            {
                DisplayText = Resources.Ok,
                DisplayDescription = Resources.Ok,
                IsDefault = true,
                ActionResult = true
            });

            return await Application.Current.Dispatcher.Invoke(async () =>
            {
                var vm = new DialogMessageViewModel()
                {
                    MessageText = message,
                    Title = headerMessage,
                    MessageType = messageType,
                    DialogImageSource = GetImgSource(messageType).ToImageSource(),
                    DialogActions = actions,
                    DefaultActionResult = false
                };

                var d = new DialogMessageBox(vm);

                return await d.ShowDialogAsync(Application.Current.MainWindow) as bool? ?? false;
            });
        }
    }
}