using RayCarrot.IO;
using RayCarrot.Rayman;
using RayCarrot.Rayman.OpenSpace;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// Utility view model for decoding Rayman 2 .sna/.dsb files
    /// </summary>
    public class R2SnaDsbDecoderViewModel : BaseDecoderUtilityViewModel<OpenSpaceGameMode>
    {
        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        public R2SnaDsbDecoderViewModel()
        {
            GameModeSelection = new EnumSelectionViewModel<OpenSpaceGameMode>(OpenSpaceGameMode.Rayman2PC, new OpenSpaceGameMode[]
            {
                OpenSpaceGameMode.Rayman2PC
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
        public override EnumSelectionViewModel<OpenSpaceGameMode> GameModeSelection { get; }

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
}