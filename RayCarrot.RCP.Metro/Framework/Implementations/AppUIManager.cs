using RayCarrot.CarrotFramework;
using RayCarrot.WPF;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// The UI Manager of this application
    /// </summary>
    public class AppUIManager
    {
        #region UserInput

        public async Task<GameTypeSelectionResult> SelectGameTypeAsync(GameTypeSelectionViewModel gameTypeSelectionViewModel, [CallerMemberName]string origin = "", [CallerFilePath]string filePath = "", [CallerLineNumber]int lineNumber = 0)
        {
            RCF.Logger.LogTraceSource($"A game type selection dialog was opened", origin: origin, filePath: filePath, lineNumber: lineNumber);

            // Create the dialog and get the result
            var result = await new GameTypeSelectionDialog(gameTypeSelectionViewModel).ShowDialogAsync();

            if (result == null)
                RCF.Logger.LogTraceSource($"The game type selection dialog returned null");
            else if (result.CanceledByUser)
                RCF.Logger.LogTraceSource($"The game type selection dialog was canceled by the user");
            else
                RCF.Logger.LogTraceSource($"The game type selection dialog returned the selected type {result.SelectedType}");

            // Return the result
            return result;
        }

        #endregion
    }
}