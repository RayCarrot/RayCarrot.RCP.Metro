using RayCarrot.IO;
using RayCarrot.Rayman;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// Utility view model for decoding Rayman 3 .sav files
    /// </summary>
    public class R3SaveDecoderViewModel : BaseDecoderUtilityViewModel<UtilityPlatforms>
    {
        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        public R3SaveDecoderViewModel()
        {
            GameModeSelection = new EnumSelectionViewModel<UtilityPlatforms>(UtilityPlatforms.PC, new UtilityPlatforms[]
            {
                UtilityPlatforms.PC
            });
        }

        #endregion

        #region Protected Override Properties

        /// <summary>
        /// Gets the file filter to use
        /// </summary>
        protected override string GetFileFilter => new FileFilterItem("*.sav", "SAV").ToString();

        /// <summary>
        /// Gets the game for the current selection
        /// </summary>
        protected override Games? GetGame => Games.Rayman3;

        #endregion

        #region Public Override Properties

        /// <summary>
        /// The game mode selection
        /// </summary>
        public override EnumSelectionViewModel<UtilityPlatforms> GameModeSelection { get; }

        #endregion

        #region Protected Override Methods

        /// <summary>
        /// Gets a new data encoder
        /// </summary>
        protected override IDataEncoder GetEncoder()
        {
            return new Rayman3SaveDataEncoder();
        }

        #endregion
    }
}