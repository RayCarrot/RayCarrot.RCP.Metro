using System.Collections.Generic;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// The Rayman 1 complete soundtrack utility
    /// </summary>
    public class Utility_Rayman1_CompleteSoundtrack : IUtility
    {
        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        public Utility_Rayman1_CompleteSoundtrack()
        {
            ViewModel = new Utility_Rayman1_CompleteSoundtrack_ViewModel();
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
        /// Indicates if the utility is work in process
        /// </summary>
        public bool IsWorkInProcess => false;

        /// <summary>
        /// The utility UI content
        /// </summary>
        public object UIContent => new Utility_Rayman1_CompleteSoundtrack_UI()
        {
            DataContext = ViewModel
        };

        /// <summary>
        /// Indicates if the utility requires administration privileges
        /// </summary>
        public bool RequiresAdmin => !RCPServices.File.CheckDirectoryWriteAccess(ViewModel.MusicDir);

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
        public Utility_Rayman1_CompleteSoundtrack_ViewModel ViewModel { get; }

        #endregion
    }
}