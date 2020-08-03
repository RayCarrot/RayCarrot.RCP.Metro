using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using RayCarrot.WPF;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using RayCarrot.Common;
using RayCarrot.IO;
using RayCarrot.Logging;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// The UI Manager of this application
    /// </summary>
    public class AppUIManager
    {
        #region UserInput

        public async Task<GamesSelectionResult> SelectGamesAsync(GamesSelectionViewModel gamesSelectionViewModel, [CallerMemberName]string origin = "", [CallerFilePath]string filePath = "", [CallerLineNumber]int lineNumber = 0)
        {
            RL.Logger?.LogTraceSource($"A games selection dialog was opened", origin: origin, filePath: filePath, lineNumber: lineNumber);

            if (Application.Current.Dispatcher == null)
                throw new Exception("The application does not have a valid dispatcher");

            // Create the dialog and get the result
            var result = await Application.Current.Dispatcher.Invoke(() => new GamesSelectionDialog(gamesSelectionViewModel)).ShowDialogAsync();

            if (result == null)
                RL.Logger?.LogTraceSource($"The games selection dialog returned null");
            else if (result.CanceledByUser)
                RL.Logger?.LogTraceSource($"The games selection dialog was canceled by the user");
            else
                RL.Logger?.LogTraceSource($"The games selection dialog returned the selected games {result.SelectedGames.JoinItems(", ")}");

            // Return the result
            return result;
        }

        public async Task<GameTypeSelectionResult> SelectGameTypeAsync(GameTypeSelectionViewModel gameTypeSelectionViewModel, [CallerMemberName]string origin = "", [CallerFilePath]string filePath = "", [CallerLineNumber]int lineNumber = 0)
        {
            RL.Logger?.LogTraceSource($"A game type selection dialog was opened", origin: origin, filePath: filePath, lineNumber: lineNumber);

            if (Application.Current.Dispatcher == null)
                throw new Exception("The application does not have a valid dispatcher");

            // Create the dialog and get the result
            var result = await Application.Current.Dispatcher.Invoke(() => new GameTypeSelectionDialog(gameTypeSelectionViewModel)).ShowDialogAsync();

            if (result == null)
                RL.Logger?.LogTraceSource($"The game type selection dialog returned null");
            else if (result.CanceledByUser)
                RL.Logger?.LogTraceSource($"The game type selection dialog was canceled by the user");
            else
                RL.Logger?.LogTraceSource($"The game type selection dialog returned the selected type {result.SelectedType}");

            // Return the result
            return result;
        }

        public async Task<EducationalDosGameEditResult> EditEducationalDosGameAsync(EducationalDosGameEditViewModel viewModel, [CallerMemberName]string origin = "", [CallerFilePath]string filePath = "", [CallerLineNumber]int lineNumber = 0)
        {
            RL.Logger?.LogTraceSource($"An educational DOS game edit dialog was opened", origin: origin, filePath: filePath, lineNumber: lineNumber);

            if (Application.Current.Dispatcher == null)
                throw new Exception("The application does not have a valid dispatcher");

            // Create the dialog and get the result
            var result = await Application.Current.Dispatcher.Invoke(() => new EducationalDosGameEditDialog(viewModel)).ShowDialogAsync();

            if (result == null)
                RL.Logger?.LogTraceSource($"The educational DOS game edit dialog returned null");
            else if (result.CanceledByUser)
                RL.Logger?.LogTraceSource($"The educational DOS game edit dialog was canceled by the user");
            else
                RL.Logger?.LogTraceSource($"The educational DOS game edit dialog returned the selected name {result.Name}");

            // Return the result
            return result;
        }

        public async Task<JumpListEditResult> EditJumpListAsync(JumpListEditViewModel viewModel, [CallerMemberName]string origin = "", [CallerFilePath]string filePath = "", [CallerLineNumber]int lineNumber = 0)
        {
            RL.Logger?.LogTraceSource($"A jump list edit dialog was opened", origin: origin, filePath: filePath, lineNumber: lineNumber);

            if (Application.Current.Dispatcher == null)
                throw new Exception("The application does not have a valid dispatcher");

            // Create the dialog and get the result
            var result = await Application.Current.Dispatcher.Invoke(() => new JumpListEditDialog(viewModel)).ShowDialogAsync();

            if (result == null)
                RL.Logger?.LogTraceSource($"The jump list edit dialog returned null");
            else if (result.CanceledByUser)
                RL.Logger?.LogTraceSource($"The jump list edit dialog was canceled by the user");

            // Return the result
            return result;
        }

        public async Task<FileExtensionSelectionDialogResult> SelectFileExtensionAsync(FileExtensionSelectionDialogViewModel viewModel, [CallerMemberName]string origin = "", [CallerFilePath]string filePath = "", [CallerLineNumber]int lineNumber = 0)
        {
            RL.Logger?.LogTraceSource($"A file extension selection dialog was opened", origin: origin, filePath: filePath, lineNumber: lineNumber);

            // If only one item is available, return it
            if (viewModel.FileFormats.Length == 1)
            {
                return new FileExtensionSelectionDialogResult()
                {
                    CanceledByUser = false,
                    SelectedFileFormat = viewModel.FileFormats.First()
                };
            }

            if (Application.Current.Dispatcher == null)
                throw new Exception("The application does not have a valid dispatcher");

            // Create the dialog and get the result
            var result = await Application.Current.Dispatcher.Invoke(() => new FileExtensionSelectionDialog(viewModel)).ShowDialogAsync();

            if (result == null)
                RL.Logger?.LogTraceSource($"The file extension selection dialog returned null");
            else if (result.CanceledByUser)
                RL.Logger?.LogTraceSource($"The file extension selection dialog was canceled by the user");

            // Return the result
            return result;
        }

        /// <summary>Displays a message to the user</summary>
        /// <param name="message">The message to display</param>
        /// <param name="header">The header for the message</param>
        /// <param name="messageType">The type of message, determining its visual appearance</param>
        /// <param name="allowCancel">True if the option to cancel is present</param>
        /// <param name="additionalActions">Additional actions</param>
        /// <param name="origin">The caller member name (leave at default for compiler-time value)</param>
        /// <param name="filePath">The caller file path (leave at default for compiler-time value)</param>
        /// <param name="lineNumber">The caller line number (leave at default for compiler-time value)</param>
        /// <returns>True if the user accepted the message, otherwise false</returns>
        public async Task<bool> DisplayMessageAsync(string message, string header, MessageType messageType, bool allowCancel, IList<DialogMessageActionViewModel> additionalActions, string origin = "", string filePath = "", int lineNumber = 0)
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

            // Add additional actions
            actions.AddRange(additionalActions);

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

        #endregion

        #region Windows

        /// <summary>
        /// Shows a new instance of the Archive Explorer, while handling any potential exceptions
        /// </summary>
        /// <param name="manager">The archive data manager</param>
        /// <param name="filePaths">The archive file paths</param>
        /// <param name="origin">The callers member/function name</param>
        /// <param name="filePath">The source code file path</param>
        /// <param name="lineNumber">The line number in the code file of the caller</param>
        /// <returns>The task</returns>
        public async Task ShowArchiveExplorerAsync(IArchiveDataManager manager, IEnumerable<FileSystemPath> filePaths, [CallerMemberName]string origin = "", [CallerFilePath]string filePath = "", [CallerLineNumber]int lineNumber = 0)
        {
            try
            {
                if (Application.Current.Dispatcher == null)
                    throw new Exception("The application does not have a valid dispatcher");

                RL.Logger?.LogTraceSource($"An Archive Explorer window was opened", origin: origin, filePath: filePath, lineNumber: lineNumber);

                // Run on UI thread
                await Application.Current.Dispatcher.Invoke(() => 
                    new ArchiveExplorerUI(new ArchiveExplorerDialogViewModel(manager, filePaths))).ShowWindowAsync();
            }
            catch (Exception ex)
            {
                ex.HandleError("Archive explorer");

                await Services.MessageUI.DisplayExceptionMessageAsync(ex, Resources.Archive_CriticalError);
            }
        }

        /// <summary>
        /// Shows a new instance of the Archive Creator
        /// </summary>
        /// <param name="manager">The archive data manager</param>
        /// <param name="origin">The callers member/function name</param>
        /// <param name="filePath">The source code file path</param>
        /// <param name="lineNumber">The line number in the code file of the caller</param>
        /// <returns>The task</returns>
        public async Task ShowArchiveCreatorAsync(IArchiveDataManager manager, [CallerMemberName]string origin = "", [CallerFilePath]string filePath = "", [CallerLineNumber]int lineNumber = 0)
        {
            if (Application.Current.Dispatcher == null)
                throw new Exception("The application does not have a valid dispatcher");

            RL.Logger?.LogTraceSource($"An Archive Creator window was opened", origin: origin, filePath: filePath, lineNumber: lineNumber);

            // Run on UI thread
            await Application.Current.Dispatcher.Invoke(() => new ArchiveCreatorUI(new ArchiveCreatorDialogViewModel(manager))).ShowWindowAsync();
        }

        #endregion
    }
}