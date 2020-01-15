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
            ArchiveExplorerViewModel = new UtilitiesArchiveExplorerViewModel();
            ConverterViewModel = new UtilitiesConverterViewModel();
            WIPViewModel = new UtilitiesWIPViewModel();
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// View model for the Archive Explorer utilities
        /// </summary>
        public UtilitiesArchiveExplorerViewModel ArchiveExplorerViewModel { get; }

        /// <summary>
        /// View model for the converter utilities
        /// </summary>
        public UtilitiesConverterViewModel ConverterViewModel { get; }

        /// <summary>
        /// View model for the work in process utilities
        /// </summary>
        public UtilitiesWIPViewModel WIPViewModel { get; }

        #endregion
    }
}