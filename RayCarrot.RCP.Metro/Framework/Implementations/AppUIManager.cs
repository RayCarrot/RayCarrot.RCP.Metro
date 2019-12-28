using RayCarrot.CarrotFramework.Abstractions;
using RayCarrot.WPF;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using RayCarrot.Extensions;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// The UI Manager of this application
    /// </summary>
    public class AppUIManager
    {
        // TODO: Make sure they run on UI thread

        #region UserInput

        public async Task<GamesSelectionResult> SelectGamesAsync(GamesSelectionViewModel gamesSelectionViewModel, [CallerMemberName]string origin = "", [CallerFilePath]string filePath = "", [CallerLineNumber]int lineNumber = 0)
        {
            RCFCore.Logger?.LogTraceSource($"A games selection dialog was opened", origin: origin, filePath: filePath, lineNumber: lineNumber);

            // Create the dialog and get the result
            var result = await new GamesSelectionDialog(gamesSelectionViewModel).ShowDialogAsync();

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

            // Create the dialog and get the result
            var result = await new GameTypeSelectionDialog(gameTypeSelectionViewModel).ShowDialogAsync();

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

            // Create the dialog and get the result
            var result = await new EducationalDosGameEditDialog(viewModel).ShowDialogAsync();

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

            // Create the dialog and get the result
            var result = await new JumpListEditDialog(viewModel).ShowDialogAsync();

            if (result == null)
                RCFCore.Logger?.LogTraceSource($"The jump list edit dialog returned null");
            else if (result.CanceledByUser)
                RCFCore.Logger?.LogTraceSource($"The jump list edit dialog was canceled by the user");

            // Return the result
            return result;
        }

        #endregion
    }
}