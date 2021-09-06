using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using RayCarrot.IO;
using NLog;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// The UI Manager of this application
    /// </summary>
    public class AppUIManager
    {
        #region Logger

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        #endregion

        #region UserInput

        public async Task<GamesSelectionResult> SelectGamesAsync(GamesSelectionViewModel gamesSelectionViewModel)
        {
            Logger.Trace($"A games selection dialog was opened");

            if (Application.Current.Dispatcher == null)
                throw new Exception("The application does not have a valid dispatcher");

            // Create the dialog and get the result
            var result = await Application.Current.Dispatcher.Invoke(() => new GamesSelectionDialog(gamesSelectionViewModel)).ShowDialogAsync();

            if (result == null)
                Logger.Trace($"The games selection dialog returned null");
            else if (result.CanceledByUser)
                Logger.Trace($"The games selection dialog was canceled by the user");
            else
                Logger.Trace($"The games selection dialog returned the selected games {result.SelectedGames.JoinItems(", ")}");

            // Return the result
            return result;
        }

        public async Task<GameTypeSelectionResult> SelectGameTypeAsync(GameTypeSelectionViewModel gameTypeSelectionViewModel)
        {
            Logger.Trace($"A game type selection dialog was opened");

            if (Application.Current.Dispatcher == null)
                throw new Exception("The application does not have a valid dispatcher");

            // Create the dialog and get the result
            var result = await Application.Current.Dispatcher.Invoke(() => new GameTypeSelectionDialog(gameTypeSelectionViewModel)).ShowDialogAsync();

            if (result == null)
                Logger.Trace($"The game type selection dialog returned null");
            else if (result.CanceledByUser)
                Logger.Trace($"The game type selection dialog was canceled by the user");
            else
                Logger.Trace($"The game type selection dialog returned the selected type {result.SelectedType}");

            // Return the result
            return result;
        }

        public async Task<EducationalDosGameEditResult> EditEducationalDosGameAsync(EducationalDosGameEditViewModel viewModel)
        {
            Logger.Trace("An educational DOS game edit dialog was opened");

            if (Application.Current.Dispatcher == null)
                throw new Exception("The application does not have a valid dispatcher");

            // Create the dialog and get the result
            var result = await Application.Current.Dispatcher.Invoke(() => new EducationalDosGameEditDialog(viewModel)).ShowDialogAsync();

            if (result == null)
                Logger.Trace($"The educational DOS game edit dialog returned null");
            else if (result.CanceledByUser)
                Logger.Trace($"The educational DOS game edit dialog was canceled by the user");
            else
                Logger.Trace($"The educational DOS game edit dialog returned the selected name {result.Name}");

            // Return the result
            return result;
        }

        public async Task<JumpListEditResult> EditJumpListAsync(JumpListEditViewModel viewModel)
        {
            Logger.Trace($"A jump list edit dialog was opened");

            if (Application.Current.Dispatcher == null)
                throw new Exception("The application does not have a valid dispatcher");

            // Create the dialog and get the result
            var result = await Application.Current.Dispatcher.Invoke(() => new JumpListEditDialog(viewModel)).ShowDialogAsync();

            if (result == null)
                Logger.Trace($"The jump list edit dialog returned null");
            else if (result.CanceledByUser)
                Logger.Trace($"The jump list edit dialog was canceled by the user");

            // Return the result
            return result;
        }

        public async Task<FileExtensionSelectionDialogResult> SelectFileExtensionAsync(FileExtensionSelectionDialogViewModel viewModel)
        {
            Logger.Trace($"A file extension selection dialog was opened");

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
                Logger.Trace($"The file extension selection dialog returned null");
            else if (result.CanceledByUser)
                Logger.Trace($"The file extension selection dialog was canceled by the user");

            // Return the result
            return result;
        }

        /// <summary>Displays a message to the user</summary>
        /// <param name="message">The message to display</param>
        /// <param name="header">The header for the message</param>
        /// <param name="messageType">The type of message, determining its visual appearance</param>
        /// <param name="allowCancel">True if the option to cancel is present</param>
        /// <param name="additionalActions">Additional actions</param>
        /// <returns>True if the user accepted the message, otherwise false</returns>
        public async Task<bool> DisplayMessageAsync(string message, string header, MessageType messageType, bool allowCancel, IList<DialogMessageActionViewModel> additionalActions)
        {
            // Make sure the application has been set up
            if (Application.Current.Dispatcher == null)
                throw new Exception("A message box can not be shown when the application dispatcher is null");

            Logger.Trace($"A message of type {messageType} was displayed with the content of: '{message}'");

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

        public async Task<StringInputResult> GetStringInput(StringInputViewModel stringInputViewModel)
        {
            Logger.Trace($"A string input dialog was opened");

            if (Application.Current.Dispatcher == null)
                throw new Exception("The application does not have a valid dispatcher");

            // Create the dialog and get the result
            var result = await Application.Current.Dispatcher.Invoke(() => new StringInputDialog(stringInputViewModel)).ShowDialogAsync();

            if (result == null)
                Logger.Trace($"The string input dialog returned null");
            else if (result.CanceledByUser)
                Logger.Trace($"The string input dialog was canceled by the user");
            else
                Logger.Trace($"The string input dialog returned the selected string {result.StringInput}");

            // Return the result
            return result;
        }

        #endregion

        #region Windows

        /// <summary>
        /// Shows a new instance of the Archive Explorer, while handling any potential exceptions
        /// </summary>
        /// <param name="manager">The archive data manager</param>
        /// <param name="filePaths">The archive file paths</param>
        /// <returns>The task</returns>
        public async Task ShowArchiveExplorerAsync(IArchiveDataManager manager, FileSystemPath[] filePaths)
        {
            try
            {
                if (Application.Current.Dispatcher == null)
                    throw new Exception("The application does not have a valid dispatcher");

                Logger.Trace($"An Archive Explorer window was opened");

                // Run on UI thread
                await Application.Current.Dispatcher.Invoke(() => 
                    new ArchiveExplorerUI(new ArchiveExplorerDialogViewModel(manager, filePaths))).ShowWindowAsync();
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Archive explorer");

                await Services.MessageUI.DisplayExceptionMessageAsync(ex, Resources.Archive_CriticalError);
            }
        }

        /// <summary>
        /// Shows a new instance of the Archive Creator
        /// </summary>
        /// <param name="manager">The archive data manager</param>
        /// <returns>The task</returns>
        public async Task ShowArchiveCreatorAsync(IArchiveDataManager manager)
        {
            if (Application.Current.Dispatcher == null)
                throw new Exception("The application does not have a valid dispatcher");

            Logger.Trace($"An Archive Creator window was opened");

            // Run on UI thread
            await Application.Current.Dispatcher.Invoke(() => new ArchiveCreatorUI(new ArchiveCreatorDialogViewModel(manager))).ShowWindowAsync();
        }

        #endregion
    }
}