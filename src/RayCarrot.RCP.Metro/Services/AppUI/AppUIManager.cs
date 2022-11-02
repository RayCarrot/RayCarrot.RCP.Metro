﻿#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using NLog;
using RayCarrot.RCP.Metro.Archive;
using RayCarrot.RCP.Metro.Patcher;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// The UI Manager of this application
/// </summary>
public class AppUIManager
{
    #region Constructor

    public AppUIManager(IDialogBaseManager dialog)
    {
        Dialog = dialog ?? throw new ArgumentNullException(nameof(dialog));
    }

    #endregion

    #region Logger

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    #endregion

    #region Private Properties

    private IDialogBaseManager Dialog { get; }

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
        Result result = await Dialog.ShowDialogWindowAsync(dialog);

        if (result == null)
            Logger.Warn("The dialog of type {0} returned null", dialogTypeName);
        else if (result.CanceledByUser)
            Logger.Trace("The dialog of type {0} was canceled by the user", dialogTypeName);

        // Return the result
        return result;
    }

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

    public Task<StringInputResult> GetStringInputAsync(StringInputViewModel stringInputViewModel) => ShowDialogAsync(() => new StringInputDialog(stringInputViewModel));

    public Task<ProgramSelectionResult> GetProgramAsync(ProgramSelectionViewModel programSelectionViewModel) => ShowDialogAsync(() => new ProgramSelectionDialog(programSelectionViewModel));

    /// <summary>
    /// Allows the user to browse for a drive
    /// </summary>
    /// <param name="driveBrowserModel">The drive browser information</param>
    /// <returns>The browse drive result</returns>
    public Task<DriveBrowserResult> BrowseDriveAsync(DriveBrowserViewModel driveBrowserModel) => ShowDialogAsync(() => new DriveSelectionDialog(driveBrowserModel));

    public Task<DownloaderResult> DownloadAsync(DownloaderViewModel viewModel) => ShowDialogAsync(() => new Downloader(viewModel));

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
        static string GetImgSource(MessageType mt)
        {
            string name = mt switch
            {
                MessageType.Generic => "Generic",
                MessageType.Information => "Generic",
                MessageType.Error => "Error",
                MessageType.Warning => "Info",
                MessageType.Question => "Question",
                MessageType.Success => "Happy",
                _ => "Generic"
            };

            return $"{AppViewModel.WPFApplicationBasePath}Img/MessageIcons/{name}.png";
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
                DialogImageSource = (ImageSource)new ImageSourceConverter().ConvertFromString(GetImgSource(messageType)),
                DialogActions = actions,
                DefaultActionResult = new UserInputResult(),
            };

            // Create the message box
            var dialog = new DialogMessageBox(vm);

            // Show the dialog and get the result
            UserInputResult result = await Dialog.ShowDialogWindowAsync(dialog);

            // Return the result
            return !result.CanceledByUser;
        });
    }

    #endregion

    #region Windows

#nullable enable

    /// <summary>
    /// Shows a new instance of the app news
    /// </summary>
    /// <returns>The task</returns>
    public async Task ShowAppNewsAsync()
    {
        if (Application.Current.Dispatcher == null)
            throw new Exception("The application does not have a valid dispatcher");

        Logger.Trace("An app news window was opened");

        // Run on UI thread
        // ReSharper disable once AccessToDisposedClosure
        using AppNewsDialog ui = Application.Current.Dispatcher.Invoke(() => new AppNewsDialog());
        await Dialog.ShowWindowAsync(ui, ShowWindowFlags.DuplicateTypesNotAllowed);
    }

    /// <summary>
    /// Shows a new instance of the game options
    /// </summary>
    /// <param name="gameInstallation">The game installation to show the options for</param>
    /// <returns>The task</returns>
    public async Task ShowGameOptionsAsync(GameInstallation gameInstallation)
    {
        if (Application.Current.Dispatcher == null)
            throw new Exception("The application does not have a valid dispatcher");

        Logger.Trace("A game options window was opened");

        // Run on UI thread
        // ReSharper disable once AccessToDisposedClosure
        using GameOptionsDialog ui = Application.Current.Dispatcher.Invoke(() => new GameOptionsDialog(gameInstallation));
        await Dialog.ShowWindowAsync(ui, groupNames: gameInstallation.GameDescriptor.DialogGroupNames.
            // TODO-14: Use install location as group name?
            Append(gameInstallation.Id).ToArray());
    }

    /// <summary>
    /// Shows a new instance of the Archive Explorer
    /// </summary>
    /// <param name="manager">The archive data manager</param>
    /// <param name="filePaths">The archive file paths</param>
    /// <returns>The task</returns>
    public async Task ShowArchiveExplorerAsync(IArchiveDataManager manager, FileSystemPath[] filePaths)
    {
        if (Application.Current.Dispatcher == null)
            throw new Exception("The application does not have a valid dispatcher");

        using ArchiveExplorerDialogViewModel vm = new(manager, filePaths);

        Logger.Trace("An Archive Explorer window was opened");

        // Run on UI thread
        // ReSharper disable once AccessToDisposedClosure
        using ArchiveExplorerDialog ui = Application.Current.Dispatcher.Invoke(() => new ArchiveExplorerDialog(vm));
        await Dialog.ShowWindowAsync(ui);
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

        ArchiveCreatorDialogViewModel vm = new(manager);

        Logger.Trace("An Archive Creator window was opened");

        // Run on UI thread
        using ArchiveCreatorDialog ui = Application.Current.Dispatcher.Invoke(() => new ArchiveCreatorDialog(vm));
        await Dialog.ShowWindowAsync(ui);
    }

    /// <summary>
    /// Shows a new instance of the Patcher from a game installation
    /// </summary>
    /// <param name="gameInstallation">The game installation</param>
    /// <returns>The task</returns>
    public async Task ShowPatcherAsync(GameInstallation gameInstallation)
    {
        if (Application.Current.Dispatcher == null)
            throw new Exception("The application does not have a valid dispatcher");

        using PatcherViewModel vm = new(gameInstallation);
        
        Logger.Trace("A Patcher window was opened");
        
        // Run on UI thread
        // ReSharper disable once AccessToDisposedClosure
        using PatcherDialog dialog = Application.Current.Dispatcher.Invoke(() => new PatcherDialog(vm));
        await Dialog.ShowWindowAsync(dialog, 
            // TODO-14: Use install location as group name?
            groupNames: gameInstallation.Id);
    }

    /// <summary>
    /// Shows a new instance of the Patcher from patch file paths
    /// </summary>
    /// <param name="patchFilePaths">The patch file paths</param>
    /// <returns>The task</returns>
    public async Task ShowPatcherAsync(FileSystemPath[] patchFilePaths)
    {
        if (Application.Current.Dispatcher == null)
            throw new Exception("The application does not have a valid dispatcher");

        using PatcherViewModel? vm = await PatcherViewModel.FromFilesAsync(patchFilePaths);

        if (vm == null)
            return;

        Logger.Trace("A Patcher window was opened");

        // Run on UI thread
        // ReSharper disable once AccessToDisposedClosure
        using PatcherDialog dialog = Application.Current.Dispatcher.Invoke(() => new PatcherDialog(vm));
        await Dialog.ShowWindowAsync(dialog,
            // TODO-14: Use install location as group name?
            groupNames: vm.GameInstallation.Id);
    }

    /// <summary>
    /// Shows a new instance of the Patch Creator
    /// </summary>
    /// <param name="game">The game</param>
    /// <param name="existingPatch">Optionally an existing patch to update</param>
    /// <returns>The task</returns>
    public async Task ShowPatchCreatorAsync(Games game, FileSystemPath? existingPatch)
    {
        if (Application.Current.Dispatcher == null)
            throw new Exception("The application does not have a valid dispatcher");

        using PatchCreatorViewModel vm = new(game);

        Logger.Trace("A Patch Creator window was opened");

        // Run on UI thread
        // ReSharper disable once AccessToDisposedClosure
        using PatchCreatorDialog dialog = Application.Current.Dispatcher.Invoke(() => new PatchCreatorDialog(vm, existingPatch));
        await Dialog.ShowWindowAsync(dialog);
    }

    #endregion
}