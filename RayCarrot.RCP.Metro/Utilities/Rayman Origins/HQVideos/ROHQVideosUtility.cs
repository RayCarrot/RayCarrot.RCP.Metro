using System.Collections.Generic;
using System.Windows;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// The Rayman Origins HQ videos utility
    /// </summary>
    public class ROHQVideosUtility : IRCPUtility
    {
        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        public ROHQVideosUtility()
        {
            ViewModel = new ROHQVideosUtilityViewModel();
        }

        #endregion

        #region Interface Members

        /// <summary>
        /// The header for the utility. This property is retrieved again when the current culture is changed.
        /// </summary>
        public string DisplayHeader => Resources.ROU_HQVideosHeader;

        /// <summary>
        /// The utility information text (optional). This property is retrieved again when the current culture is changed.
        /// </summary>
        public string InfoText => Resources.ROU_HQVideosInfo;

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
        public UIElement UIContent => new ROHQVideosUtilityUI()
        {
            DataContext = ViewModel
        };

        /// <summary>
        /// Indicates if the utility requires administration privileges
        /// </summary>
        public bool RequiresAdmin => !RCFRCP.File.CheckFileWriteAccess(ViewModel.VideoDir + "intro.bik");

        /// <summary>
        /// Indicates if the utility is available to the user
        /// </summary>
        public bool IsAvailable => ViewModel.CanVideosBeReplaced;

        /// <summary>
        /// Retrieves a list of applied utilities from this utility
        /// </summary>
        /// <returns>The applied utilities</returns>
        public IEnumerable<string> GetAppliedUtilities()
        {
            if (ViewModel.GetIsOriginalVideos() == false)
                yield return Resources.ROU_HQVideosHeader;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// The view model
        /// </summary>
        public ROHQVideosUtilityViewModel ViewModel { get; }

        #endregion
    }
}