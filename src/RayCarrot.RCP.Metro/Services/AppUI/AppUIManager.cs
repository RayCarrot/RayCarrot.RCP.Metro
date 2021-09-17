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

        #region Dialogs

        public async Task<Result> ShowDialogAsync<UserInput, Result>(Func<IDialogWindowControl<UserInput, Result>> createDialogFunc)
            where UserInput : UserInputViewModel
            where Result : UserInputResult
        {
            if (Application.Current.Dispatcher == null)
                throw new Exception("The application does not have a valid dispatcher");

            // Create the dialog on the UI thread
            IDialogWindowControl<UserInput, Result> dialog = Application.Current.Dispatcher.Invoke(createDialogFunc);
            string dialogTypeName = dialog.GetType().Name;

            Logger.Trace("A dialog of type {0} was opened", dialogTypeName);

            // Show the dialog and get the result
            Result result = await Services.DialogBaseManager.ShowDialogWindowAsync(dialog);

            if (result == null)
                Logger.Warn("The dialog of type {0} returned null", dialogTypeName);
            else if (result.CanceledByUser)
                Logger.Trace("The dialog of type {0} was canceled by the user", dialogTypeName);

            // Return the result
            return result;
        }

        public Task<GamesSelectionResult> SelectGamesAsync(GamesSelectionViewModel gamesSelectionViewModel) => ShowDialogAsync(() => new GamesSelectionDialog(gamesSelectionViewModel));

        public Task<GameTypeSelectionResult> SelectGameTypeAsync(GameTypeSelectionViewModel gameTypeSelectionViewModel) => ShowDialogAsync(() => new GameTypeSelectionDialog(gameTypeSelectionViewModel));

        public Task<EducationalDosGameEditResult> EditEducationalDosGameAsync(EducationalDosGameEditViewModel viewModel) => ShowDialogAsync(() => new EducationalDosGameEditDialog(viewModel));

        public Task<JumpListEditResult> EditJumpListAsync(JumpListEditViewModel viewModel) => ShowDialogAsync(() => new JumpListEditDialog(viewModel));

        public async Task<FileExtensionSelectionDialogResult> SelectFileExtensionAsync(FileExtensionSelectionDialogViewModel viewModel)
        {
            // If only one item is available, return it
            if (viewModel.FileFormats.Length == 1)
            {
                return new FileExtensionSelectionDialogResult()
                {
                    CanceledByUser = false,
                    SelectedFileFormat = viewModel.FileFormats.First()
                };
            }

            return await ShowDialogAsync(() => new FileExtensionSelectionDialog(viewModel));
        }

        public Task<StringInputResult> GetStringInput(StringInputViewModel stringInputViewModel) => ShowDialogAsync(() => new StringInputDialog(stringInputViewModel));

        /// <summary>
        /// Allows the user to browse for a drive
        /// </summary>
        /// <param name="driveBrowserModel">The drive browser information</param>
        /// <returns>The browse drive result</returns>
        public Task<DriveBrowserResult> BrowseDriveAsync(DriveBrowserViewModel driveBrowserModel) => ShowDialogAsync(() => new DriveSelectionDialog(driveBrowserModel));

        /// <summary>
        /// Displays a message to the user
        /// </summary>
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

            Logger.Trace("A message of type {0} was displayed with the content of: '{1}'", messageType, message);

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
                    ActionResult = new UserInputResult(),
                });

            // Add additional actions
            actions.AddRange(additionalActions);

            // Create the default action
            actions.Add(new DialogMessageActionViewModel()
            {
                DisplayText = Resources.Ok,
                DisplayDescription = Resources.Ok,
                IsDefault = true,
                ActionResult = new UserInputResult()
                {
                    CanceledByUser = false
                },
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
                    DefaultActionResult = new UserInputResult(),
                };

                // Create the message box
                var dialog = new DialogMessageBox(vm);

                // Show the dialog and get the result
                UserInputResult result = await Services.DialogBaseManager.ShowDialogWindowAsync(dialog);

                // Return the result
                return !result.CanceledByUser;
            });
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

                Logger.Trace("An Archive Explorer window was opened");

                // Run on UI thread
                ArchiveExplorerUI ui = Application.Current.Dispatcher.Invoke(() => new ArchiveExplorerUI(new ArchiveExplorerDialogViewModel(manager, filePaths)));
                await Services.DialogBaseManager.ShowWindowAsync(ui);
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

            Logger.Trace("An Archive Creator window was opened");

            // Run on UI thread
            ArchiveCreatorUI ui = Application.Current.Dispatcher.Invoke(() => new ArchiveCreatorUI(new ArchiveCreatorDialogViewModel(manager)));
            await Services.DialogBaseManager.ShowWindowAsync(ui);
        }

        #endregion
    }
}