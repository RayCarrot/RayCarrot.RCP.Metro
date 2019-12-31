using System.Collections.Generic;
using System.Linq;
using RayCarrot.RCP.Core;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// The Rayman 2 CNT explorer utility
    /// </summary>
    public class R2CNTExplorerUtility : IRCPUtility
    {
        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        public R2CNTExplorerUtility()
        {
            ViewModel = new R2CNTExplorerUtilityViewModel();
        }

        #endregion

        #region Interface Members

        /// <summary>
        /// The header for the utility. This property is retrieved again when the current culture is changed.
        /// </summary>
        public string DisplayHeader => Resources.R2U_CNTExplorer_Header;

        /// <summary>
        /// The utility information text (optional). This property is retrieved again when the current culture is changed.
        /// </summary>
        public string InfoText => Resources.R2U_CNTExplorer_Info;

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
        public object UIContent => new R2CNTExplorerUtilityUI()
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
        public bool IsAvailable => ViewModel.ArchiveFiles.Any(x => x.FileExists);

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
        public R2CNTExplorerUtilityViewModel ViewModel { get; }

        #endregion
    }
}