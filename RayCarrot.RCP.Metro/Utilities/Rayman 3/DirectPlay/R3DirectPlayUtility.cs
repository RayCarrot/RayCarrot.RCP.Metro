using System.Collections.Generic;
using System.Windows;
using RayCarrot.Windows.Shell;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// The Rayman 3 Direct Play utility
    /// </summary>
    public class R3DirectPlayUtility : IRCPUtility
    {
        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        public R3DirectPlayUtility()
        {
            ViewModel = new R3DirectPlayUtilityViewModel();
        }

        #endregion

        #region Interface Members

        /// <summary>
        /// The header for the utility. This property is retrieved again when the current culture is changed.
        /// </summary>
        public string DisplayHeader => Resources.R3U_DirectPlayHeader;

        /// <summary>
        /// The utility information text (optional). This property is retrieved again when the current culture is changed.
        /// </summary>
        public string InfoText => Resources.R3U_DirectPlayInfo;

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
        public UIElement UIContent => new R3DirectPlayUtilityUI()
        {
            DataContext = ViewModel
        };

        /// <summary>
        /// Indicates if the utility requires administration privileges
        /// </summary>
        public bool RequiresAdmin => true;

        /// <summary>
        /// Indicates if the utility is available to the user
        /// </summary>
        public bool IsAvailable => AppViewModel.WindowsVersion >= WindowsVersion.Win8;

        /// <summary>
        /// Retrieves a list of applied utilities from this utility
        /// </summary>
        /// <returns>The applied utilities</returns>
        public IEnumerable<string> GetAppliedUtilities()
        {
            // Due to this being a common Windows setting it is not marked as an applied utility
            return new string[0];
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// The view model
        /// </summary>
        public R3DirectPlayUtilityViewModel ViewModel { get; }

        #endregion
    }
}