using System;
using RayCarrot.IO;
using RayCarrot.Rayman;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// Utility view model for decoding Rayman 1 .lng files
    /// </summary>
    public class R1LngDecoderViewModel : BaseDecoderUtilityViewModel<R1LngDecoderViewModel.Rayman1Version>
    {
        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        public R1LngDecoderViewModel()
        {
            GameModeSelection = new EnumSelectionViewModel<Rayman1Version>(Rayman1Version.Ray_1_21, new Rayman1Version[]
            {
                Rayman1Version.Ray_1_00,
                Rayman1Version.Ray_1_10,
                Rayman1Version.Ray_1_12,
                Rayman1Version.Ray_Japan,
                Rayman1Version.Ray_1_21,
            });
        }

        #endregion

        #region Protected Override Properties

        /// <summary>
        /// Gets the file filter to use
        /// </summary>
        protected override string GetFileFilter => new FileFilterItem("*.lng", "LNG").ToString();

        /// <summary>
        /// Gets the game for the current selection
        /// </summary>
        protected override Games? GetGame => Games.Rayman1;

        #endregion

        #region Public Override Properties

        /// <summary>
        /// The game mode selection
        /// </summary>
        public override EnumSelectionViewModel<Rayman1Version> GameModeSelection { get; }

        #endregion

        #region Protected Override Methods

        /// <summary>
        /// Gets a new data encoder
        /// </summary>
        protected override IDataEncoder GetEncoder()
        {
            return GameModeSelection.SelectedValue switch
            {
                Rayman1Version.Ray_1_00 => new SegmentedDataEncoder(new SegmentedDataEncoder.SegmentedDataInfo[]
                {
                    // English
                    new SegmentedDataEncoder.SegmentedDataInfo(new XORDataEncoder(154), 4175 - 0),

                    // French
                    new SegmentedDataEncoder.SegmentedDataInfo(new XORDataEncoder(55), 8791 - 4175),

                    // German
                    new SegmentedDataEncoder.SegmentedDataInfo(new XORDataEncoder(70), 13587 - 8791),
                }),

                Rayman1Version.Ray_1_10 => new SegmentedDataEncoder(new SegmentedDataEncoder.SegmentedDataInfo[]
                {
                    // English
                    new SegmentedDataEncoder.SegmentedDataInfo(new XORDataEncoder(220), 4176 - 0),

                    // French
                    new SegmentedDataEncoder.SegmentedDataInfo(new XORDataEncoder(196), 8796 - 4176),

                    // German
                    new SegmentedDataEncoder.SegmentedDataInfo(new XORDataEncoder(192), 13607 - 8796),
                }),

                Rayman1Version.Ray_1_12 => new SegmentedDataEncoder(new SegmentedDataEncoder.SegmentedDataInfo[]
                {
                    // English
                    new SegmentedDataEncoder.SegmentedDataInfo(new XORDataEncoder(75), 4175 - 0),

                    // French
                    new SegmentedDataEncoder.SegmentedDataInfo(new XORDataEncoder(111), 8795 - 4175),

                    // German
                    new SegmentedDataEncoder.SegmentedDataInfo(new XORDataEncoder(178), 13606 - 8795),
                }),

                Rayman1Version.Ray_Japan => new SegmentedDataEncoder(new SegmentedDataEncoder.SegmentedDataInfo[]
                {
                    // English
                    new SegmentedDataEncoder.SegmentedDataInfo(new XORDataEncoder(252), 4234 - 0),

                    // French
                    new SegmentedDataEncoder.SegmentedDataInfo(new XORDataEncoder(133), 8947 - 4234),

                    // German
                    new SegmentedDataEncoder.SegmentedDataInfo(new XORDataEncoder(213), 13850 - 8947),

                    // Japanese
                    new SegmentedDataEncoder.SegmentedDataInfo(new XORDataEncoder(89), 16361 - 13850),
                }),

                Rayman1Version.Ray_1_21 => new SegmentedDataEncoder(new SegmentedDataEncoder.SegmentedDataInfo[]
                {
                    // English
                    new SegmentedDataEncoder.SegmentedDataInfo(new XORDataEncoder(48), 4234 - 0),

                    // French
                    new SegmentedDataEncoder.SegmentedDataInfo(new XORDataEncoder(130), 8947 - 4234),

                    // German
                    new SegmentedDataEncoder.SegmentedDataInfo(new XORDataEncoder(207), 13850 - 8947),

                    // Japanese
                    new SegmentedDataEncoder.SegmentedDataInfo(new XORDataEncoder(208), 16361 - 13850),

                    // Chinese
                    new SegmentedDataEncoder.SegmentedDataInfo(new XORDataEncoder(149), 18727 - 16361),
                }),

                _ => throw new ArgumentOutOfRangeException(nameof(GameModeSelection.SelectedValue),
                    GameModeSelection.SelectedValue, null)
            };
        }

        #endregion

        #region Enums

        // IDEA: Move to RayCarrot.Rayman?
        /// <summary>
        /// The available Rayman 1 PC versions for the .lng decoder
        /// </summary>
        public enum Rayman1Version
        {
            [GameModeBase("Rayman 1 (PC - 1.00)")]
            Ray_1_00,

            [GameModeBase("Rayman 1 (PC - 1.10)")]
            Ray_1_10,

            [GameModeBase("Rayman 1 (PC - 1.12)")]
            Ray_1_12,

            [GameModeBase("Rayman 1 (PC - Japan)")]
            Ray_Japan,

            [GameModeBase("Rayman 1 (PC - 1.21)")]
            Ray_1_21,
        }

        #endregion
    }
}