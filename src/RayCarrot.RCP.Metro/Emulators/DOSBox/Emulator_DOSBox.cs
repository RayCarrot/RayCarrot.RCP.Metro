namespace RayCarrot.RCP.Metro;

/// <summary>
/// The DOSBox emulator for emulating MS-DOS games
/// </summary>
public class Emulator_DOSBox : Emulator
{
    /// <summary>
    /// The display name of the emulator
    /// </summary>
    public override LocalizedString DisplayName => new ResourceLocString(nameof(Resources.GameType_DosBox));

    /// <summary>
    /// The emulator's game configuration view model
    /// </summary>
    public override GameOptionsDialog_EmulatorConfigPageViewModel GetGameConfigViewModel(GameInstallation gameInstallation) => 
        new Emulator_DOSBox_ConfigViewModel(gameInstallation);
}