using System.Collections.Generic;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// The Rayman 2 save converter utility
    /// </summary>
    public class R2SaveConverterUtility : IUtility
    {
        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        public R2SaveConverterUtility()
        {
            ViewModel = new R2SaveConverterUtilityViewModel();
        }

        #endregion

        #region Interface Members

        /// <summary>
        /// The header for the utility. This property is retrieved again when the current culture is changed.
        /// </summary>
        public string DisplayHeader => Resources.Utilities_Converter_R2SaveHeader;

        /// <summary>
        /// The utility information text (optional). This property is retrieved again when the current culture is changed.
        /// </summary>
        public string InfoText => null;

        /// <summary>
        /// The utility warning text (optional). This property is retrieved again when the current culture is changed.
        /// </summary>
        public string WarningText => null;

        /// <summary>
        /// Indicates if the utility requires additional files to be downloaded remotely
        /// </summary>
        public bool RequiresAdditionalFiles => false;

        /// <summary>
        /// Indicates if the utility is work in process
        /// </summary>
        public bool IsWorkInProcess => true;

        /// <summary>
        /// The utility UI content
        /// </summary>
        public object UIContent => new BaseConverterUtilityUI()
        {
            DataContext = ViewModel
        };

        /// <summary>
        /// Indicates if the utility requires administration privileges
        /// </summary>
        public bool RequiresAdmin => false;

        /// <summary>
        /// Indicates if the utility is available to the user
        /// </summary>
        public bool IsAvailable => true;

        /// <summary>
        /// Retrieves a list of applied utilities from this utility
        /// </summary>
        /// <returns>The applied utilities</returns>
        public IEnumerable<string> GetAppliedUtilities() => new string[0];

        #endregion

        #region Public Properties

        /// <summary>
        /// The view model
        /// </summary>
        public R2SaveConverterUtilityViewModel ViewModel { get; }

        #endregion
    }
}