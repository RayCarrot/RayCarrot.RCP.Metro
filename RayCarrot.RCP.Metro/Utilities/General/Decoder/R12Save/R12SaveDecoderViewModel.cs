using RayCarrot.IO;
using RayCarrot.Rayman;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// Utility view model for decoding Rayman 1 and 2 .sav files
    /// </summary>
    public class R12SaveDecoderViewModel : BaseDecoderUtilityViewModel<Platform>
    {
        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        public R12SaveDecoderViewModel()
        {
            GameModeSelection = new EnumSelectionViewModel<Platform>(Platform.PC, new Platform[]
            {
                Platform.PC
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

        // TODO: Make two utilities, one for each game?
        /// <summary>
        /// Gets the game for the current selection
        /// </summary>
        protected override Games? GetGame => Games.Rayman1;

        #endregion

        #region Public Override Properties

        /// <summary>
        /// The game mode selection
        /// </summary>
        public override EnumSelectionViewModel<Platform> GameModeSelection { get; }

        #endregion

        #region Protected Override Methods

        /// <summary>
        /// Gets a new data encoder
        /// </summary>
        protected override IDataEncoder GetEncoder()
        {
            return new Rayman12PCSaveDataEncoder();
        }

        #endregion
    }
}