using RayCarrot.UI;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// User input result for a game type selection
    /// </summary>
    public class GameTypeSelectionResult : UserInputResult
    {
        /// <summary>
        /// The selected game type
        /// </summary>
        public GameType SelectedType { get; set; }
    }
}