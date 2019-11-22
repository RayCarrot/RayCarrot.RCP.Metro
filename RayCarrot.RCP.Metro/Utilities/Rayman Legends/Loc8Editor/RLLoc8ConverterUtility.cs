using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// The Rayman Legends Loc8 converter utility
    /// </summary>
    public class RLLoc8ConverterUtility : IRCPUtility
    {
        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        public RLLoc8ConverterUtility()
        {
            ViewModel = new RLLoc8ConverterUtilityViewModel();
        }

        #endregion

        #region Interface Members

        /// <summary>
        /// The header for the utility. This property is retrieved again when the current culture is changed.
        /// </summary>
        public string DisplayHeader => Resources.RLU_Loc8ConverterHeader;

        /// <summary>
        /// The utility information text (optional). This property is retrieved again when the current culture is changed.
        /// </summary>
        public string InfoText => Resources.RLU_Loc8ConverterHeaderInfo;

        /// <summary>
        /// The utility warning text (optional). This property is retrieved again when the current culture is changed.
        /// </summary>
        public string WarningText => null;

        /// <summary>
        /// Indicates if the utility requires additional files to be downloaded remotely
        /// </summary>
        public bool RequiresAdditionalFiles => false;

        /// <summary>
        /// The utility UI content
        /// </summary>
        public UIElement UIContent => new RLLoc8ConverterUtilityUI()
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
        public IEnumerable<string> GetAppliedUtilities()
        {
            // Due to this utility being able to convert any file, not just for the current installation, it will not count as an applied utility
            return new string[0];
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// The view model
        /// </summary>
        public RLLoc8ConverterUtilityViewModel ViewModel { get; }

        #endregion
    }
}