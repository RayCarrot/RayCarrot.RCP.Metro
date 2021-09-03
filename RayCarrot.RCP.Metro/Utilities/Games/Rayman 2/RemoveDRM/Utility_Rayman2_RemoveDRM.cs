using System.Collections.Generic;
using System.Linq;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// The Rayman 2 DRM removal utility
    /// </summary>
    public class Utility_Rayman2_RemoveDRM : IUtility
    {
        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        public Utility_Rayman2_RemoveDRM()
        {
            ViewModel = new Utility_Rayman2_RemoveDRM_ViewModel();
        }

        #endregion

        #region Interface Members

        /// <summary>
        /// The header for the utility. This property is retrieved again when the current culture is changed.
        /// </summary>
        public string DisplayHeader => Resources.R2U_RemoveDRM_Header;

        /// <summary>
        /// The utility information text (optional). This property is retrieved again when the current culture is changed.
        /// </summary>
        public string InfoText => Resources.R2U_RemoveDRM_Info;

        /// <summary>
        /// The utility warning text (optional). This property is retrieved again when the current culture is changed.
        /// </summary>
        public string WarningText => Resources.R2U_RemoveDRM_Warning;

        /// <summary>
        /// Indicates if the utility requires additional files to be downloaded remotely
        /// </summary>
        public bool RequiresAdditionalFiles => false;

        /// <summary>
        /// Indicates if the utility is work in process
        /// </summary>
        public bool IsWorkInProcess => false;

        /// <summary>
        /// The utility UI content
        /// </summary>
        public object UIContent => new Utility_Rayman2_RemoveDRM_UI()
        {
            DataContext = ViewModel
        };

        /// <summary>
        /// Indicates if the utility requires administration privileges
        /// </summary>
        public bool RequiresAdmin => !RCPServices.File.CheckFileWriteAccess(ViewModel.SnaOffsets.Keys.FirstOrDefault());

        /// <summary>
        /// Indicates if the utility is available to the user
        /// </summary>
        public bool IsAvailable => ViewModel.SnaOffsets.Any();

        /// <summary>
        /// Retrieves a list of applied utilities from this utility
        /// </summary>
        /// <returns>The applied utilities</returns>
        public IEnumerable<string> GetAppliedUtilities()
        {
            if (ViewModel.HasBeenApplied)
                yield return Resources.R2U_RemoveDRM_Header;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// The view model
        /// </summary>
        public Utility_Rayman2_RemoveDRM_ViewModel ViewModel { get; }

        #endregion
    }
}