namespace RayCarrot.RCP.Metro.Games.OptionsDialog;

/// <summary>
/// View model for the Rayman Jungle Run configuration
/// </summary>
public class RaymanJungleRunConfigViewModel : UbiArtRunBaseConfigViewModel
{
    #region Constructor

    public RaymanJungleRunConfigViewModel(WindowsPackageGameDescriptor gameDescriptor, GameInstallation gameInstallation) 
        : base(gameDescriptor, gameInstallation)
    { }

    #endregion

    #region Private Constants

    private const string SelectedHeroFileName = "ROHeroe";

    private const string SelectedSlotFileName = "ROselectedSlot";

    #endregion

    #region Private Fields

    private JungleRunHero _selectedHero;

    private byte _selectedSlot;

    #endregion

    #region Public Properties

    /// <summary>
    /// The selected hero
    /// </summary>
    public JungleRunHero SelectedHero
    {
        get => _selectedHero;
        set
        {
            _selectedHero = value;
            UnsavedChanges = true;
        }
    }

    /// <summary>
    /// The selected save slot, a value between 0 and 2
    /// </summary>
    public byte SelectedSlot
    {
        get => _selectedSlot;
        set
        {
            _selectedSlot = value;
            UnsavedChanges = true;
        }
    }

    #endregion

    #region Protected Override Methods

    /// <summary>
    /// Sets up the game specific values
    /// </summary>
    /// <returns>The task</returns>
    protected override Task SetupGameAsync()
    {
        // Read selected hero and save slot data
        SelectedHero = (JungleRunHero)(ReadSingleByteFile(SelectedHeroFileName) ?? 0);
        SelectedSlot = ReadSingleByteFile(SelectedSlotFileName) ?? 0;

        return Task.CompletedTask;
    }

    /// <summary>
    /// Saves the game specific values
    /// </summary>
    /// <returns>The task</returns>
    protected override Task SaveGameAsync()
    {
        // Save selected hero and save slot data
        WriteSingleByteFile(SelectedHeroFileName, (byte)SelectedHero);
        WriteSingleByteFile(SelectedSlotFileName, SelectedSlot);

        return Task.CompletedTask;
    }

    #endregion

    #region Enums

    /// <summary>
    /// The available heroes in Rayman Jungle Run
    /// </summary>
    public enum JungleRunHero
    {
        Rayman,
        Globox,
        DarkRayman,
        GloboxOutfit,
        GreenTeensy,
        GothTeensy
    }

    #endregion
}