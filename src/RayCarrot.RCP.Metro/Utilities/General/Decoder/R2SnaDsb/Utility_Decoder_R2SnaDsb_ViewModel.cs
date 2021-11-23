using RayCarrot.IO;
using RayCarrot.Rayman;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// Utility view model for decoding Rayman 2 .sna/.dsb files
/// </summary>
public class Utility_Decoder_R2SnaDsb_ViewModel : Utility_BaseDecoder_ViewModel<GameMode>
{
    #region Constructor

    /// <summary>
    /// Default constructor
    /// </summary>
    public Utility_Decoder_R2SnaDsb_ViewModel()
    {
        GameModeSelection = new EnumSelectionViewModel<GameMode>(GameMode.Rayman2PC, new GameMode[]
        {
            GameMode.Rayman2PC
        });
    }

    #endregion

    #region Protected Override Properties

    /// <summary>
    /// Gets the file filter to use
    /// </summary>
    protected override string GetFileFilter => new FileFilterItemCollection()
    {
        new FileFilterItem("*.sna", "SNA"),
        new FileFilterItem("*.dsb", "DSB"),
    }.CombineAll("Rayman 2").ToString();

    /// <summary>
    /// Gets the game for the current selection
    /// </summary>
    protected override Games? GetGame => Games.Rayman2;

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
    protected override IDataEncoder GetEncoder()
    {
        return new Rayman2SNADataEncoder();
    }

    #endregion
}