using System.Collections.Generic;
using System.Windows;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// The Rayman 1 complete soundtrack utility
    /// </summary>
    public class R1CompleteSoundtrackUtility : IRCPUtility
    {
        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        public R1CompleteSoundtrackUtility()
        {
            ViewModel = new R1CompleteSoundtrackUtilityViewModel();
        }

        #endregion

        #region Interface Members

        /// <summary>
        /// The header for the utility. This property is retrieved again when the current culture is changed.
        /// </summary>
        public string DisplayHeader => Resources.R1U_CompleteOSTHeader;

        /// <summary>
        /// The utility information text (optional). This property is retrieved again when the current culture is changed.
        /// </summary>
        public string InfoText => Resources.R1U_CompleteOSTInfo;

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
        public UIElement UIContent => new R1CompleteSoundtrackUtilityUI()
        {
            DataContext = ViewModel
        };

        /// <summary>
        /// Indicates if the utility requires administration privileges
        /// </summary>
        public bool RequiresAdmin => !RCFRCP.File.CheckDirectoryWriteAccess(ViewModel.MusicDir);

        /// <summary>
        /// Indicates if the utility is available to the user
        /// </summary>
        public bool IsAvailable => ViewModel.CanMusicBeReplaced;

        /// <summary>
        /// Retrieves a list of applied utilities from this utility
        /// </summary>
        /// <returns>The applied utilities</returns>
        public IEnumerable<string> GetAppliedUtilities()
        {
            if (ViewModel.GetIsOriginalSoundtrack() == false)
                yield return Resources.R1U_CompleteOSTHeader;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// The view model
        /// </summary>
        public R1CompleteSoundtrackUtilityViewModel ViewModel { get; }

        #endregion
    }
}