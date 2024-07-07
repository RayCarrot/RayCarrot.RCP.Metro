namespace RayCarrot.RCP.Metro.Games.OptionsDialog;

/// <summary>
/// View model for the Rayman Jungle Run configuration
/// </summary>
public class RaymanJungleRunConfigViewModel : BaseUbiArtRunConfigViewModel
{
    #region Constructor

    public RaymanJungleRunConfigViewModel(
        GameDescriptor gameDescriptor, 
        GameInstallation gameInstallation,
        FileSystemPath saveDir,
        bool hasPlayableTeensies,
        bool hasMultipleSaveSlots,
        bool isUpc) 
        : base(gameDescriptor, gameInstallation, saveDir, isUpc)
    {
        HasPlayableTeensies = hasPlayableTeensies;
        HasMultipleSaveSlots = hasMultipleSaveSlots;
    }

    #endregion

    #region Private Constants

    private const string SelectedHeroFileName = "ROHeroe";
    private const string SelectedSlotFileName = "ROselectedSlot";

    #endregion

    #region Public Properties

    public bool HasPlayableTeensies { get; }
    public bool HasMultipleSaveSlots { get; }

    public bool IsHeroSettingsAvailable => !IsUpc || UpcStorageHeaders.ContainsKey(SelectedHeroFileName);

    /// <summary>
    /// The selected hero
    /// </summary>
    public JungleRunHero SelectedHero { get; set; }

    /// <summary>
    /// The selected save slot, a value between 0 and 2
    /// </summary>
    public byte SelectedSlot { get; set; }

    #endregion

    #region Protected Override Methods

    protected override Task SetupGameAsync()
    {
        AddConfigLocation(LinkItemViewModel.LinkType.BinaryFile, GetFilePath(SelectedHeroFileName));
        OnPropertyChanged(nameof(IsHeroSettingsAvailable));
        SelectedHero = (JungleRunHero)(ReadSingleByteFile(SelectedHeroFileName) ?? 0);

        if (HasMultipleSaveSlots)
        {
            AddConfigLocation(LinkItemViewModel.LinkType.BinaryFile, GetFilePath(SelectedSlotFileName));
            SelectedSlot = ReadSingleByteFile(SelectedSlotFileName) ?? 0;
        }

        return Task.CompletedTask;
    }

    protected override Task SaveGameAsync()
    {
        if (IsHeroSettingsAvailable)
            WriteSingleByteFile(SelectedHeroFileName, (byte)SelectedHero);
        
        if (HasMultipleSaveSlots)
            WriteSingleByteFile(SelectedSlotFileName, SelectedSlot);

        return Task.CompletedTask;
    }

    protected override void ConfigPropertyChanged(string propertyName)
    {
        base.ConfigPropertyChanged(propertyName);

        if (propertyName is
            nameof(SelectedHero) or 
            nameof(SelectedSlot))
        {
            UnsavedChanges = true;
        }
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

        // Microsoft Store only
        GreenTeensy,
        GothTeensy
    }

    #endregion
}