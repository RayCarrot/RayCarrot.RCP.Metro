using RayCarrot.UI;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// View model for a game type selection
    /// </summary>
    public class GameTypeSelectionViewModel : UserInputViewModel
    {
        /// <summary>
        /// The selected game type
        /// </summary>
        public GameType SelectedType { get; set; }

        /// <summary>
        /// Indicates if Win32 is a valid selection
        /// </summary>
        public bool AllowWin32 { get; set; }

        /// <summary>
        /// Indicates if Steam is a valid selection
        /// </summary>
        public bool AllowSteam { get; set; }

        /// <summary>
        /// Indicates if WinStore is a valid selection
        /// </summary>
        public bool AllowWinStore { get; set; }

        /// <summary>
        /// Indicates if DosBox is a valid selection
        /// </summary>
        public bool AllowDosBox { get; set; }
    }
}