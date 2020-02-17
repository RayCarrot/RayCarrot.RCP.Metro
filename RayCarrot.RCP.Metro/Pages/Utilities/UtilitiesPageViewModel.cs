namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// View model for the utilities page
    /// </summary>
    public class UtilitiesPageViewModel : BaseRCPViewModel
    {
        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        public UtilitiesPageViewModel()
        {
            // Create view models
            ArchiveExplorerViewModels = new UtilityViewModel[]
            {
                new UtilityViewModel(new CNTArchiveExplorerUtility()),
                new UtilityViewModel(new IPKArchiveExplorerUtility()),
            };
            ConverterViewModels = new UtilityViewModel[]
            {
                new UtilityViewModel(new GFConverterUtility()),
                new UtilityViewModel(new R3SaveConverterUtility()),
                new UtilityViewModel(new LOCConverterUtility()),
                new UtilityViewModel(new ROSaveConverterUtility()),
                new UtilityViewModel(new RJRSaveConverterUtility()),
                new UtilityViewModel(new RLSaveConverterUtility()),
            };
            DecoderViewModels = new UtilityViewModel[]
            {
                new UtilityViewModel(new R1LngDecoderUtility()),
                new UtilityViewModel(new R2SnaDsbDecoderUtility()),
                new UtilityViewModel(new R3SaveDecoderUtility()),
            };
            OtherViewModels = new UtilityViewModel[]
            {
                new UtilityViewModel(new R1MapViewerUtility()),
                new UtilityViewModel(new SyncTextureInfoUtility()),
            };
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// View models for the Archive Explorer utilities
        /// </summary>
        public UtilityViewModel[] ArchiveExplorerViewModels { get; }

        /// <summary>
        /// View models for the converter utilities
        /// </summary>
        public UtilityViewModel[] ConverterViewModels { get; }

        /// <summary>
        /// View models for the decoder utilities
        /// </summary>
        public UtilityViewModel[] DecoderViewModels { get; }

        /// <summary>
        /// View models for the other utilities
        /// </summary>
        public UtilityViewModel[] OtherViewModels { get; }

        #endregion
    }
}