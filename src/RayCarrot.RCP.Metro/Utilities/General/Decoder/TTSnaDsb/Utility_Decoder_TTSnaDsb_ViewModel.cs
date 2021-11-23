using RayCarrot.IO;
using RayCarrot.Rayman;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// Utility view model for decoding Tonic Trouble .sna/.dsb files
/// </summary>
public class Utility_Decoder_TTSnaDsb_ViewModel : Utility_BaseDecoder_ViewModel<GameMode>
{
    #region Constructor

    /// <summary>
    /// Default constructor
    /// </summary>
    public Utility_Decoder_TTSnaDsb_ViewModel()
    {
        GameModeSelection = new EnumSelectionViewModel<GameMode>(GameMode.TonicTroublePC, new GameMode[]
        {
            GameMode.TonicTroublePC
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
    }.CombineAll("Tonic Trouble").ToString();

    /// <summary>
    /// Gets the game for the current selection
    /// </summary>
    protected override Games? GetGame => null;

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
        return new TonicTroubleSNADataEncoder();
    }

    #endregion
}