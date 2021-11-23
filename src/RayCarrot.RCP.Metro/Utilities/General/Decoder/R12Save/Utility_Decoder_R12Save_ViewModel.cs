using RayCarrot.IO;
using RayCarrot.Rayman;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// Utility view model for decoding Rayman 1 and 2 .sav files
/// </summary>
public class Utility_Decoder_R12Save_ViewModel : Utility_BaseDecoder_ViewModel<GameMode>
{
    #region Constructor

    /// <summary>
    /// Default constructor
    /// </summary>
    public Utility_Decoder_R12Save_ViewModel()
    {
        GameModeSelection = new EnumSelectionViewModel<GameMode>(GameMode.Rayman1PC, new GameMode[]
        {
            GameMode.Rayman1PC,
            GameMode.Rayman2PC,
        });
    }

    #endregion

    #region Protected Override Properties

    /// <summary>
    /// Gets the file filter to use
    /// </summary>
    protected override string GetFileFilter => new FileFilterItemCollection()
    {
        new FileFilterItem("*.sav", "SAV"),
        new FileFilterItem("*.cfg", "CFG"),
    }.CombineAll("SAV").ToString();

    /// <summary>
    /// Gets the game for the current selection
    /// </summary>
    protected override Games? GetGame => GameModeSelection.SelectedValue.GetGame();

    #endregion

    #region Public Override Properties

    /// <summary>
    /// The game mode selection
    /// </summary>
    public override EnumSelectionViewModel<GameMode> GameModeSelection { get; }

    #endregion

    #region Protected Override Methods

    /// <summary>
    /// Gets a new data encoder
    /// </summary>
    protected override IDataEncoder GetEncoder() => new Rayman12PCSaveDataEncoder();

    #endregion
}