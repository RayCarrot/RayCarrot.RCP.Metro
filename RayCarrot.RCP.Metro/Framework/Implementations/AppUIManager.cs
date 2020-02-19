using System;
using System.Collections.Generic;
using System.Linq;
using RayCarrot.CarrotFramework.Abstractions;
using RayCarrot.WPF;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using RayCarrot.Extensions;
using RayCarrot.IO;
using RayCarrot.UI;

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
            RCFCore.Logger?.LogTraceSource($"A games selection dialog was opened", origin: origin, filePath: filePath, lineNumber: lineNumber);

            if (Application.Current.Dispatcher == null)
                throw new Exception("The application does not have a valid dispatcher");

            // Create the dialog and get the result
            var result = await Application.Current.Dispatcher.Invoke(() => new GamesSelectionDialog(gamesSelectionViewModel)).ShowDialogAsync();

            if (result == null)
                RCFCore.Logger?.LogTraceSource($"The games selection dialog returned null");
            else if (result.CanceledByUser)
                RCFCore.Logger?.LogTraceSource($"The games selection dialog was canceled by the user");
            else
                RCFCore.Logger?.LogTraceSource($"The games selection dialog returned the selected games {result.SelectedGames.JoinItems(", ")}");

            // Return the result
            return result;
        }

        public async Task<GameTypeSelectionResult> SelectGameTypeAsync(GameTypeSelectionViewModel gameTypeSelectionViewModel, [CallerMemberName]string origin = "", [CallerFilePath]string filePath = "", [CallerLineNumber]int lineNumber = 0)
        {
            RCFCore.Logger?.LogTraceSource($"A game type selection dialog was opened", origin: origin, filePath: filePath, lineNumber: lineNumber);

            if (Application.Current.Dispatcher == null)
                throw new Exception("The application does not have a valid dispatcher");

            // Create the dialog and get the result
            var result = await Application.Current.Dispatcher.Invoke(() => new GameTypeSelectionDialog(gameTypeSelectionViewModel)).ShowDialogAsync();

            if (result == null)
                RCFCore.Logger?.LogTraceSource($"The game type selection dialog returned null");
            else if (result.CanceledByUser)
                RCFCore.Logger?.LogTraceSource($"The game type selection dialog was canceled by the user");
            else
                RCFCore.Logger?.LogTraceSource($"The game type selection dialog returned the selected type {result.SelectedType}");

            // Return the result
            return result;
        }

        public async Task<EducationalDosGameEditResult> EditEducationalDosGameAsync(EducationalDosGameEditViewModel viewModel, [CallerMemberName]string origin = "", [CallerFilePath]string filePath = "", [CallerLineNumber]int lineNumber = 0)
        {
            RCFCore.Logger?.LogTraceSource($"An educational DOS game edit dialog was opened", origin: origin, filePath: filePath, lineNumber: lineNumber);

            if (Application.Current.Dispatcher == null)
                throw new Exception("The application does not have a valid dispatcher");

            // Create the dialog and get the result
            var result = await Application.Current.Dispatcher.Invoke(() => new EducationalDosGameEditDialog(viewModel)).ShowDialogAsync();

            if (result == null)
                RCFCore.Logger?.LogTraceSource($"The educational DOS game edit dialog returned null");
            else if (result.CanceledByUser)
                RCFCore.Logger?.LogTraceSource($"The educational DOS game edit dialog was canceled by the user");
            else
                RCFCore.Logger?.LogTraceSource($"The educational DOS game edit dialog returned the selected name {result.Name}");

            // Return the result
            return result;
        }

        public async Task<JumpListEditResult> EditJumpListAsync(JumpListEditViewModel viewModel, [CallerMemberName]string origin = "", [CallerFilePath]string filePath = "", [CallerLineNumber]int lineNumber = 0)
        {
            RCFCore.Logger?.LogTraceSource($"A jump list edit dialog was opened", origin: origin, filePath: filePath, lineNumber: lineNumber);

            if (Application.Current.Dispatcher == null)
                throw new Exception("The application does not have a valid dispatcher");

            // Create the dialog and get the result
            var result = await Application.Current.Dispatcher.Invoke(() => new JumpListEditDialog(viewModel)).ShowDialogAsync();

            if (result == null)
                RCFCore.Logger?.LogTraceSource($"The jump list edit dialog returned null");
            else if (result.CanceledByUser)
                RCFCore.Logger?.LogTraceSource($"The jump list edit dialog was canceled by the user");

            // Return the result
            return result;
        }

        public async Task<FileExtensionSelectionDialogResult> SelectFileExtensionAsync(FileExtensionSelectionDialogViewModel viewModel, [CallerMemberName]string origin = "", [CallerFilePath]string filePath = "", [CallerLineNumber]int lineNumber = 0)
        {
            RCFCore.Logger?.LogTraceSource($"A file extension selection dialog was opened", origin: origin, filePath: filePath, lineNumber: lineNumber);

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
                RCFCore.Logger?.LogTraceSource($"The file extension selection dialog returned null");
            else if (result.CanceledByUser)
                RCFCore.Logger?.LogTraceSource($"The file extension selection dialog was canceled by the user");

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

                RCFCore.Logger?.LogTraceSource($"An Archive Explorer window was opened", origin: origin, filePath: filePath, lineNumber: lineNumber);

                // Run on UI thread
                await Application.Current.Dispatcher.Invoke(() => 
                    new ArchiveExplorerUI(new ArchiveExplorerDialogViewModel(manager, filePaths))).ShowWindowAsync();
            }
            catch (Exception ex)
            {
                ex.HandleError("Archive explorer");

                await RCFUI.MessageUI.DisplayExceptionMessageAsync(ex, Resources.Archive_CriticalError);
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

            RCFCore.Logger?.LogTraceSource($"An Archive Creator window was opened", origin: origin, filePath: filePath, lineNumber: lineNumber);

            // Run on UI thread
            await Application.Current.Dispatcher.Invoke(() => new ArchiveCreatorUI(new ArchiveCreatorDialogViewModel(manager))).ShowWindowAsync();
        }

        #endregion
    }
}