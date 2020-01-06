using System.Collections.Generic;
using System.Linq;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// The Rayman Legends UbiRay utility
    /// </summary>
    public class RLUbiRayUtility : IRCPUtility
    {
        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        public RLUbiRayUtility()
        {
            ViewModel = new RLUbiRayUtilityViewModel();
        }

        #endregion

        #region Interface Members

        /// <summary>
        /// The header for the utility. This property is retrieved again when the current culture is changed.
        /// </summary>
        public string DisplayHeader => Resources.RLU_UbiRayHeader;

        /// <summary>
        /// The utility information text (optional). This property is retrieved again when the current culture is changed.
        /// </summary>
        public string InfoText => Resources.RLU_UbiRayInfo;

        /// <summary>
        /// The utility warning text (optional). This property is retrieved again when the current culture is changed.
        /// </summary>
        public string WarningText => Resources.RLU_UbiRayWarning;

        /// <summary>
        /// Indicates if the utility requires additional files to be downloaded remotely
        /// </summary>
        public bool RequiresAdditionalFiles => false;

        /// <summary>
        /// The utility UI content
        /// </summary>
        public object UIContent => new RLUbiRayUtilityUI()
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
        public bool IsAvailable => ViewModel.AvailableSaveFiles?.Any() == true;

        /// <summary>
        /// Retrieves a list of applied utilities from this utility
        /// </summary>
        /// <returns>The applied utilities</returns>
        public IEnumerable<string> GetAppliedUtilities()
        {
            // Due to the changes not being able to be reverted this is not considered an applied utility
            return new string[0];
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// The view model
        /// </summary>
        public RLUbiRayUtilityViewModel ViewModel { get; }

        #endregion
    }
}