using System.Collections.Generic;
using System.Windows;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// The Rayman Origins update utility
    /// </summary>
    public class ROUpdateUtility : IRCPUtility
    {
        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        public ROUpdateUtility()
        {
            ViewModel = new ROUpdateUtilityViewModel();
        }

        #endregion

        #region Interface Members

        /// <summary>
        /// The header for the utility. This property is retrieved again when the current culture is changed.
        /// </summary>
        public string DisplayHeader => Resources.ROU_UpdateHeader;

        /// <summary>
        /// The utility information text (optional). This property is retrieved again when the current culture is changed.
        /// </summary>
        public string InfoText => Resources.ROU_UpdateInfo;

        /// <summary>
        /// The utility warning text (optional). This property is retrieved again when the current culture is changed.
        /// </summary>
        public string WarningText => null;

        /// <summary>
        /// Indicates if the utility requires additional files to be downloaded remotely
        /// </summary>
        public bool RequiresAdditionalFiles => true;

        /// <summary>
        /// The utility UI content
        /// </summary>
        public UIElement UIContent => new ROUpdateUtilityUI()
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
            return new string[0];
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// The view model
        /// </summary>
        public ROUpdateUtilityViewModel ViewModel { get; }

        #endregion
    }
}