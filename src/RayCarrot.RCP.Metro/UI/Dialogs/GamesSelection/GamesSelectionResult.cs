namespace RayCarrot.RCP.Metro;

/// <summary>
/// Result for a games selection dialog
/// </summary>
public class GamesSelectionResult : UserInputResult
{
    /// <summary>
    /// The selected game. Use this if only a single game can be selected.
    /// </summary>
    public GameInstallation SelectedGame { get; set; }

    /// <summary>
    /// The selected games. Use this if multiple games can be selected.
    /// </summary>
    public IReadOnlyList<GameInstallation> SelectedGames { get; set; }
}