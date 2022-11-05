namespace RayCarrot.RCP.Metro;

/// <summary>
/// Defines an emulator used to launch a game
/// </summary>
public abstract class Emulator
{
    /// <summary>
    /// The display name of the emulator
    /// </summary>
    public abstract LocalizedString DisplayName { get; }

    /// <summary>
    /// Gets the emulator's game configuration view model
    /// </summary>
    /// <param name="gameInstallation">The game installation to get the view model for</param>
    /// <returns>The view model</returns>
    public abstract GameOptionsDialog_EmulatorConfigPageViewModel? GetGameConfigViewModel(GameInstallation gameInstallation);
}